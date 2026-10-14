using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooter : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 20f;
    public float fireCooldown = 0.2f;

    private float lastFireTime = -999f;

    public void OnAttack(InputValue value)
    {
        if (Time.timeScale == 0f) return;
        if (Time.time < lastFireTime + fireCooldown) return;
        lastFireTime = Time.time;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        bullet.GetComponent<Rigidbody>().linearVelocity = firePoint.forward * bulletSpeed;
    }
}
