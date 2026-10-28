using UnityEngine;

public class EnemyChaser : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float detectRange = 15f;
    public float attackRange = 1.5f;
    public int damage = 1;
    public float attackCooldown = 1f;

    private Transform player;
    private EnemyMove mover;
    private float lastAttackTime = -999f;

    void Start()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;
        mover = GetComponent<EnemyMove>();
    }

    void Update()
    {
        if (player == null) return;

        Vector3 toPlayer = player.position + Vector3.up - transform.position;
        float distance = toPlayer.magnitude;
        if (distance > detectRange) return;

        transform.forward = toPlayer.normalized;
        if (distance > attackRange)
        {
            mover.Fly(transform.forward, moveSpeed);
        }
        else if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            player.GetComponent<Health>().TakeDamage(damage, transform.forward);
        }
    }
}
