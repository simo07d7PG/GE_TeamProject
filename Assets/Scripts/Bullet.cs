using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 1;
    public float lifeTime = 2f;
    public string targetTag = "Enemy";
    public string ownerTag = "Player";
    public bool pierce;

    public static event System.Action<bool> OnPlayerHit;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return;
        if (other.CompareTag(ownerTag)) return;

        if (other.CompareTag(targetTag))
        {
            Health hp = other.GetComponent<Health>();
            if (hp != null && hp.currentHp > 0)
            {
                hp.TakeDamage(damage, transform.forward);
                if (ownerTag == "Player") OnPlayerHit?.Invoke(hp.currentHp <= 0);
            }
            if (pierce) return;
        }
        Destroy(gameObject);
    }
}
