using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooter : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 20f;
    public float fireCooldown = 0.2f;

    public float heatPerShot = 8f;
    public float coolPerSecond = 20f;
    public float killCool = 10f;
    public float overheatUnlock = 40f;

    private float lastFireTime = -999f;
    private Camera mainCam;
    private float weaponHeat;
    private bool overheated;

    void Start()
    {
        mainCam = Camera.main;
    }

    void OnEnable()
    {
        Health.OnEnemyKilled += OnEnemyKilled;
    }

    void OnDisable()
    {
        Health.OnEnemyKilled -= OnEnemyKilled;
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;

        weaponHeat = Mathf.Max(0f, weaponHeat - coolPerSecond * Time.deltaTime);
        if (overheated && weaponHeat <= overheatUnlock) overheated = false;
    }

    void OnEnemyKilled(Health enemy)
    {
        weaponHeat = Mathf.Max(0f, weaponHeat - killCool);
    }

    public void ResetWeaponHeat()
    {
        weaponHeat = 0f;
        overheated = false;
    }

    void OnGUI()
    {
        if (Time.timeScale == 0f) return;

        float width = 220f;
        float height = 14f;
        Rect back = new Rect((Screen.width - width) * 0.5f, Screen.height - 60f, width, height);

        GUI.color = new Color(0f, 0f, 0f, 0.6f);
        GUI.DrawTexture(back, Texture2D.whiteTexture);

        float ratio = Mathf.Clamp01(weaponHeat / 100f);
        GUI.color = overheated ? Color.red : Color.Lerp(Color.cyan, new Color(1f, 0.5f, 0f), ratio);
        GUI.DrawTexture(new Rect(back.x + 2f, back.y + 2f, (width - 4f) * ratio, height - 4f), Texture2D.whiteTexture);

        if (overheated)
        {
            GUI.color = Color.red;
            GUI.Label(new Rect(back.x, back.y - 22f, width, 20f), "OVERHEAT");
        }
        GUI.color = Color.white;
    }

    public void OnAttack(InputValue value)
    {
        if (Time.timeScale == 0f) return;
        if (overheated) return;
        float cooldown = PlayerUpgrades.RapidFire ? fireCooldown * 0.5f : fireCooldown;
        if (Time.time < lastFireTime + cooldown) return;
        lastFireTime = Time.time;

        weaponHeat += heatPerShot;
        if (weaponHeat >= 100f)
        {
            weaponHeat = 100f;
            overheated = true;
            Fx.Blip(transform.position, 180f, 0.25f, 0.6f);
        }

        Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        Vector3 targetPoint;
        if (Physics.Raycast(ray, out hit))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(100f);
        }

        Vector3 fireDirection = (targetPoint - firePoint.position).normalized;

        float[] angles = PlayerUpgrades.TripleShot ? new[] { 0f, -12f, 12f } : new[] { 0f };
        foreach (float angle in angles)
        {
            Vector3 dir = Quaternion.AngleAxis(angle, Vector3.up) * fireDirection;
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(dir));
            bullet.GetComponent<Rigidbody>().linearVelocity = dir * bulletSpeed;
            bullet.GetComponent<Bullet>().pierce = PlayerUpgrades.Pierce;
        }
    }
}