using System.Collections.Generic;
using UnityEngine;

public class EnemyBoss : MonoBehaviour
{
    public GameObject bulletPrefab;
    public float moveSpeed = 1.5f;
    public float keepDistance = 10f;
    public float fireInterval = 3f;
    public int ringBullets = 10;
    public float bulletSpeed = 8f;

    public const float BarHeight = 33f;
    public const float BarStride = 44f;

    private static readonly List<EnemyBoss> bosses = new List<EnemyBoss>();

    private Transform player;
    private Health health;
    private CharacterController controller;
    private EnemyMove mover;
    private float lastFireTime;
    private float ringOffset;

    public static int BarCount
    {
        get
        {
            Health finalBoss = WaveManager.Instance != null ? WaveManager.Instance.FinalBoss : null;
            int count = 0;
            foreach (EnemyBoss b in bosses)
            {
                if (b.health != null && b.health != finalBoss) count++;
            }
            return count;
        }
    }

    void OnEnable()
    {
        bosses.Add(this);
    }

    void OnDisable()
    {
        bosses.Remove(this);
    }

    void Start()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;
        health = GetComponent<Health>();
        controller = GetComponent<CharacterController>();
        mover = GetComponent<EnemyMove>();
        lastFireTime = Time.time;
    }

    void Update()
    {
        if (player == null) return;

        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;
        float distance = toPlayer.magnitude;
        if (distance > 0.01f) transform.forward = toPlayer.normalized;

        Vector3 move = distance > keepDistance ? transform.forward : Vector3.zero;
        mover.Fly(move, moveSpeed);

        if (Time.time >= lastFireTime + fireInterval)
        {
            lastFireTime = Time.time;
            FireRing();
        }
    }

    void FireRing()
    {
        if (bulletPrefab == null) return;

        Vector3 origin = transform.position + controller.center;
        float tilt = (player.position.y + 1f - origin.y) / keepDistance;
        ringOffset += 180f / ringBullets;
        for (int i = 0; i < ringBullets; i++)
        {
            float angle = ringOffset + 360f / ringBullets * i;
            Vector3 dir = (Quaternion.Euler(0f, angle, 0f) * Vector3.forward + Vector3.up * tilt).normalized;
            GameObject bullet = Instantiate(bulletPrefab, origin, Quaternion.LookRotation(dir));
            bullet.GetComponent<Rigidbody>().linearVelocity = dir * bulletSpeed;
        }
        Fx.Blip(transform.position, 140f, 0.3f, 0.7f);
    }

    void OnGUI()
    {
        if (Time.timeScale == 0f || health == null) return;
        if (WaveManager.Instance != null && WaveManager.Instance.FinalBoss == health) return;

        float width = 330f;
        int index = bosses.IndexOf(this);
        float top = WaveManager.StackBottom + HudStyle.Grid;
        Rect back = new Rect((Screen.width - width) * 0.5f, top + index * BarStride, width, BarHeight);
        HudStyle.Frame(back, HudStyle.BG, HudStyle.LINE);

        float pad = HudStyle.Grid;
        HudStyle.Text(new Rect(back.x + pad, back.y + 6f, width * 0.5f, 13f), "BOSS", 11, HudStyle.DIM, TextAnchor.MiddleLeft, false);
        string hp = "HP " + Mathf.Max(0, health.currentHp) + "/" + health.maxHp;
        HudStyle.Text(new Rect(back.xMax - pad - width * 0.5f, back.y + 6f, width * 0.5f, 13f), hp, 11, HudStyle.TEXT, TextAnchor.MiddleRight, false);

        float ratio = Mathf.Clamp01((float)health.currentHp / health.maxHp);
        HudStyle.Bar(new Rect(back.x + pad, back.y + 21f, width - pad * 2f, 6f), ratio, HudStyle.DANGER);
    }
}
