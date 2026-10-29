using UnityEngine;

public class PlayerUpgrades : MonoBehaviour
{
    public static PlayerUpgrades Instance { get; private set; }

    public float duration = 10f;
    public float dropChance = 0.2f;
    public float dropMultiplier = 1f;

    private float tripleUntil;
    private float rapidUntil;
    private float pierceUntil;

    public static bool TripleShot { get { return Instance != null && Time.time < Instance.tripleUntil; } }
    public static bool RapidFire { get { return Instance != null && Time.time < Instance.rapidUntil; } }
    public static bool Pierce { get { return Instance != null && Time.time < Instance.pierceUntil; } }

    public static void Create()
    {
        if (Instance != null) return;
        new GameObject("PlayerUpgrades").AddComponent<PlayerUpgrades>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void TryDrop(Health enemy)
    {
        bool isBoss = enemy.GetComponent<EnemyBoss>() != null;
        if (!isBoss && Random.value > dropChance * dropMultiplier) return;

        PickupType type = (PickupType)Random.Range(0, System.Enum.GetValues(typeof(PickupType)).Length);
        Pickup.Create(enemy.transform.position, type);
    }

    public void Grant(PickupType type)
    {
        float until = Time.time + duration;
        switch (type)
        {
            case PickupType.TripleShot: tripleUntil = until; break;
            case PickupType.RapidFire: rapidUntil = until; break;
            case PickupType.Pierce: pierceUntil = until; break;
        }
    }

    public void ResetUpgrades()
    {
        tripleUntil = 0f;
        rapidUntil = 0f;
        pierceUntil = 0f;
    }

    void OnGUI()
    {
        if (Time.timeScale == 0f) return;

        float y = Screen.height - 140f;
        y = DrawLine(y, "3WAY", tripleUntil, Pickup.ColorOf(PickupType.TripleShot));
        y = DrawLine(y, "RAPID", rapidUntil, Pickup.ColorOf(PickupType.RapidFire));
        DrawLine(y, "PIERCE", pierceUntil, Pickup.ColorOf(PickupType.Pierce));
    }

    float DrawLine(float y, string label, float until, Color color)
    {
        float left = until - Time.time;
        if (left <= 0f) return y;

        Rect back = new Rect(22f, y, 154f, 22f);
        HudStyle.Frame(back, HudStyle.BG, HudStyle.LINE);
        HudStyle.Fill(new Rect(back.x + 11f, back.y + 8f, 6f, 6f), color);
        HudStyle.Text(new Rect(back.x + 28f, back.y, 77f, back.height), label, 11, HudStyle.TEXT, TextAnchor.MiddleLeft, false);
        HudStyle.Text(new Rect(back.xMax - 11f - 44f, back.y, 44f, back.height), left.ToString("0.0") + "s", 11, HudStyle.TEXT, TextAnchor.MiddleRight, false);
        return y - 28f;
    }
}
