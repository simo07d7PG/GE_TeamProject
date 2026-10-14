using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float moveSpeed = 2f;
    public float keepDistance = 8f;
    public float fireRange = 12f;
    public float fireInterval = 2f;
    public float bulletSpeed = 10f;

    private Transform player;
    private float lastFireTime;

    void Start()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;
        lastFireTime = Time.time;
    }

    void Update()
    {
        if (player == null) return;

        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0;
        float distance = toPlayer.magnitude;
        transform.forward = toPlayer.normalized;

        if (distance > keepDistance)
        {
            transform.position += transform.forward * moveSpeed * Time.deltaTime;
        }

        bool inRange = distance < fireRange;
        bool ready = Time.time >= lastFireTime + fireInterval;
        if (inRange && ready)
        {
            lastFireTime = Time.time;
            Shoot();
        }
    }

    void Shoot()
    {
        Vector3 target = player.position + Vector3.up * 1f;
        Vector3 dir = (target - firePoint.position).normalized;
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(dir));
        bullet.GetComponent<Rigidbody>().linearVelocity = dir * bulletSpeed;
    }
}
