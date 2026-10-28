using UnityEngine;

public class EnemyAdBot : MonoBehaviour
{
    public float moveSpeed = 2.5f;
    public float keepDistance = 6f;
    public float detectRange = 25f;
    public float popupInterval = 4f;
    public float popupDuration = 5f;

    private Transform player;
    private float lastPopupTime;

    void Start()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;
        lastPopupTime = Time.time;
    }

    void Update()
    {
        if (player == null) return;

        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;
        float distance = toPlayer.magnitude;
        if (distance > detectRange) return;

        transform.forward = toPlayer.normalized;
        if (distance > keepDistance)
        {
            transform.position += transform.forward * moveSpeed * Time.deltaTime;
        }

        if (Time.time >= lastPopupTime + popupInterval && AdPopups.Instance != null)
        {
            lastPopupTime = Time.time;
            AdPopups.Instance.Spawn(this, popupDuration);
        }
    }
}
