using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 1;
    public float lifeTime = 2f;
    public string targetTag = "Enemy";
    public string ownerTag = "Player";

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
            if (hp != null) hp.TakeDamage(damage);
        }
        Destroy(gameObject);
    }
}
