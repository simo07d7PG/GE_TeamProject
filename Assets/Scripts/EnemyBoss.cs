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

    private static readonly List<EnemyBoss> bosses = new List<EnemyBoss>();

    private Transform player;
    private Health health;
    private float lastFireTime;
    private float ringOffset;

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
        lastFireTime = Time.time;
    }

    void Update()
    {
        if (player == null) return;

        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;
        float distance = toPlayer.magnitude;
        if (distance > 0.01f) transform.forward = toPlayer.normalized;

        if (distance > keepDistance)
        {
            transform.position += transform.forward * moveSpeed * Time.deltaTime;
        }

        if (Time.time >= lastFireTime + fireInterval)
        {
            lastFireTime = Time.time;
            FireRing();
        }
    }

    void FireRing()
    {
        if (bulletPrefab == null) return;

        ringOffset += 180f / ringBullets;
        for (int i = 0; i < ringBullets; i++)
        {
            float angle = ringOffset + 360f / ringBullets * i;
            Vector3 dir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.LookRotation(dir));
            bullet.GetComponent<Rigidbody>().linearVelocity = dir * bulletSpeed;
        }
        Fx.Blip(transform.position, 140f, 0.3f, 0.7f);
    }

    void OnGUI()
    {
        if (Time.timeScale == 0f || health == null) return;

        float width = 360f;
        float height = 16f;
        int index = bosses.IndexOf(this);
        Rect back = new Rect((Screen.width - width) * 0.5f, 20f + index * 26f, width, height);

        GUI.color = new Color(0f, 0f, 0f, 0.7f);
        GUI.DrawTexture(back, Texture2D.whiteTexture);
        GUI.color = new Color(0.8f, 0.1f, 0.9f);
        float ratio = Mathf.Clamp01((float)health.currentHp / health.maxHp);
        GUI.DrawTexture(new Rect(back.x + 2f, back.y + 2f, (width - 4f) * ratio, height - 4f), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }
}
