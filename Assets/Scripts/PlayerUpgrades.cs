using UnityEngine;

public class PlayerUpgrades : MonoBehaviour
{
    public static PlayerUpgrades Instance { get; private set; }

    public float duration = 10f;
    public float dropChance = 0.2f;

    private float tripleUntil;
    private float rapidUntil;
    private float pierceUntil;
    private GUIStyle style;

    public static bool TripleShot { get { return Instance != null && Time.time < Instance.tripleUntil; } }
    public static bool RapidFire { get { return Instance != null && Time.time < Instance.rapidUntil; } }
    public static bool Pierce { get { return Instance != null && Time.time < Instance.pierceUntil; } }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Create()
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

    void OnEnable()
    {
        Health.OnEnemyKilled += OnEnemyKilled;
    }

    void OnDisable()
    {
        Health.OnEnemyKilled -= OnEnemyKilled;
    }

    void OnEnemyKilled(Health enemy)
    {
        bool isBoss = enemy.GetComponent<EnemyBoss>() != null;
        if (!isBoss && Random.value > dropChance) return;

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

        if (style == null)
        {
            style = new GUIStyle(GUI.skin.label);
            style.fontSize = 18;
            style.fontStyle = FontStyle.Bold;
        }

        float y = Screen.height - 140f;
        y = DrawLine(y, "3WAY", tripleUntil, Pickup.ColorOf(PickupType.TripleShot));
        y = DrawLine(y, "RAPID", rapidUntil, Pickup.ColorOf(PickupType.RapidFire));
        DrawLine(y, "PIERCE", pierceUntil, Pickup.ColorOf(PickupType.Pierce));
        GUI.color = Color.white;
    }

    float DrawLine(float y, string label, float until, Color color)
    {
        float left = until - Time.time;
        if (left <= 0f) return y;

        GUI.color = color;
        GUI.Label(new Rect(20f, y, 200f, 24f), label + " " + left.ToString("0.0") + "s", style);
        return y - 26f;
    }
}
