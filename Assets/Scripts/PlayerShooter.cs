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
    public float overheatSystemHeat = 10f;

    public float gunHeatMultiplier = 1f;
    public float gunCoolMultiplier = 1f;
    public float damageMultiplier = 1f;
    public float fireRateMultiplier = 1f;
    public float bulletSpeedMultiplier = 1f;
    public bool autoFire;

    private float lastFireTime = -999f;
    private Camera mainCam;
    private float weaponHeat;
    private bool overheated;
    private InputAction attackAction;

    public float GunHeatRatio => Mathf.Clamp01(weaponHeat / 100f);
    public bool Overheated => overheated;

    void Start()
    {
        mainCam = Camera.main;
        PlayerInput playerInput = GetComponentInParent<PlayerInput>();
        if (playerInput != null && playerInput.actions != null) attackAction = playerInput.actions.FindAction("Attack");
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;

        weaponHeat = Mathf.Max(0f, weaponHeat - coolPerSecond * gunCoolMultiplier * Time.deltaTime);
        if (overheated && weaponHeat <= overheatUnlock) overheated = false;

        if (autoFire && attackAction != null && attackAction.IsPressed()) Fire();
    }

    public void CoolOnKill()
    {
        weaponHeat = Mathf.Max(0f, weaponHeat - killCool);
    }

    public void ResetWeaponHeat()
    {
        weaponHeat = 0f;
        overheated = false;
    }

    public void OnAttack(InputValue value)
    {
        Fire();
    }

    void Fire()
    {
        if (Time.timeScale == 0f) return;
        if (overheated) return;
        float cooldown = (PlayerUpgrades.RapidFire ? fireCooldown * 0.5f : fireCooldown) / Mathf.Max(0.1f, fireRateMultiplier);
        if (Time.time < lastFireTime + cooldown) return;
        lastFireTime = Time.time;

        weaponHeat += heatPerShot * gunHeatMultiplier;
        if (weaponHeat >= 100f)
        {
            weaponHeat = 100f;
            overheated = true;
            Fx.Blip(transform.position, 180f, 0.25f, 0.6f);
            if (GameManager.Instance != null && GameManager.Instance.heatGauge != null) GameManager.Instance.heatGauge.AddHeat(overheatSystemHeat);
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
            bullet.GetComponent<Rigidbody>().linearVelocity = dir * bulletSpeed * bulletSpeedMultiplier;
            Bullet b = bullet.GetComponent<Bullet>();
            b.pierce = PlayerUpgrades.Pierce;
            float scaled = b.damage * damageMultiplier;
            int damage = Mathf.FloorToInt(scaled);
            if (Random.value < scaled - damage) damage++;
            b.damage = damage;
        }
    }
}