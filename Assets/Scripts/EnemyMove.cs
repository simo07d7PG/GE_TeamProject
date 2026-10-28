using UnityEngine;

public class EnemyMove : MonoBehaviour
{
    public float blockedRatio = 0.25f;
    public bool holdHeight = true;

    private CharacterController controller;
    private float homeY;
    private Vector3 detour;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        controller.detectCollisions = false;
        homeY = transform.position.y;

        Collider[] mine = GetComponents<Collider>();
        foreach (EnemyMove other in FindObjectsByType<EnemyMove>(FindObjectsSortMode.None))
        {
            if (other == this) continue;

            foreach (Collider a in mine)
            {
                foreach (Collider b in other.GetComponents<Collider>()) Physics.IgnoreCollision(a, b);
            }
        }
    }

    public void Fly(Vector3 direction, float speed)
    {
        float distance = speed * Time.deltaTime;
        float needed = distance * blockedRatio;

        if (direction.sqrMagnitude <= 0f)
        {
            detour = Vector3.zero;
            Descend(distance);
            return;
        }

        Vector3 dir = direction.normalized;

        if (detour != Vector3.zero)
        {
            float around = Step((dir + detour).normalized, distance);
            if (around >= distance * 0.9f) detour = Vector3.zero;
            else if (around < needed) detour = -detour;
            return;
        }

        if (Step(dir, distance) >= needed)
        {
            Descend(distance);
            return;
        }

        float startY = transform.position.y;
        controller.Move(Vector3.up * distance);
        if (transform.position.y - startY >= needed) return;

        detour = Vector3.Cross(Vector3.up, dir);
        if (detour.sqrMagnitude < 0.01f) detour = transform.right;
        detour.Normalize();
    }

    float Step(Vector3 dir, float distance)
    {
        Vector3 before = transform.position;
        controller.Move(dir * distance);
        return Vector3.Dot(transform.position - before, dir);
    }

    void Descend(float distance)
    {
        if (!holdHeight || transform.position.y <= homeY) return;

        controller.Move(Vector3.down * Mathf.Min(distance, transform.position.y - homeY));
    }
}
