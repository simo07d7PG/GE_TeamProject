using UnityEngine;

public enum PickupType
{
    TripleShot,
    RapidFire,
    Pierce,
    Coolant,
    Repair,
}

public class Pickup : MonoBehaviour
{
    public PickupType type;
    public float lifeTime = 12f;
    public float pickupRadius = 1.8f;

    private float spawnTime;
    private float baseY;
    private Transform player;
    private Renderer rend;
    private GUIStyle labelStyle;

    public static void Create(Vector3 position, PickupType type)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(go.GetComponent<Collider>());
        go.name = "Pickup_" + type;
        go.transform.position = position;
        go.transform.localScale = Vector3.one * 0.5f;

        Renderer r = go.GetComponent<Renderer>();
        r.sharedMaterial = Fx.MakeMaterial(ColorOf(type));
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        go.AddComponent<Pickup>().type = type;
    }

    public static Color ColorOf(PickupType type)
    {
        switch (type)
        {
            case PickupType.TripleShot: return new Color(1f, 0.6f, 0f);
            case PickupType.RapidFire: return new Color(1f, 1f, 0.2f);
            case PickupType.Pierce: return new Color(0.7f, 0.3f, 1f);
            case PickupType.Coolant: return new Color(0.2f, 0.8f, 1f);
            default: return new Color(0.3f, 1f, 0.4f);
        }
    }

    public static string LabelOf(PickupType type)
    {
        switch (type)
        {
            case PickupType.TripleShot: return "3WAY";
            case PickupType.RapidFire: return "RAPID";
            case PickupType.Pierce: return "PIERCE";
            case PickupType.Coolant: return "COOL";
            default: return "HP+";
        }
    }

    void Start()
    {
        spawnTime = Time.time;
        baseY = transform.position.y;
        rend = GetComponent<Renderer>();
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;

        float age = Time.time - spawnTime;
        if (age >= lifeTime)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 pos = transform.position;
        pos.y = baseY + Mathf.Sin(age * 4f) * 0.15f;
        transform.position = pos;
        transform.Rotate(0f, 120f * Time.deltaTime, 0f);

        if (rend != null) rend.enabled = age < lifeTime - 3f || Mathf.Repeat(age * 6f, 1f) < 0.5f;

        if (player == null) return;
        Vector3 diff = player.position - transform.position;
        diff.y = 0f;
        if (diff.magnitude <= pickupRadius) Collect();
    }

    void Collect()
    {
        switch (type)
        {
            case PickupType.Coolant:
                PlayerShooter shooter = player.GetComponentInChildren<PlayerShooter>();
                if (shooter != null) shooter.ResetWeaponHeat();
                break;
            case PickupType.Repair:
                Health hp = player.GetComponent<Health>();
                if (hp != null) hp.Heal(1);
                break;
            default:
                if (PlayerUpgrades.Instance != null) PlayerUpgrades.Instance.Grant(type);
                break;
        }

        Fx.Burst(transform.position, ColorOf(type), 10);
        Fx.Blip(transform.position, 760f, 0.12f, 0.5f);
        Destroy(gameObject);
    }

    void OnGUI()
    {
        if (Time.timeScale == 0f || Camera.main == null) return;

        Vector3 screen = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 0.6f);
        if (screen.z <= 0f) return;

        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.alignment = TextAnchor.MiddleCenter;
            labelStyle.fontSize = 14;
            labelStyle.fontStyle = FontStyle.Bold;
        }

        GUI.color = ColorOf(type);
        GUI.Label(new Rect(screen.x - 40f, Screen.height - screen.y - 10f, 80f, 20f), LabelOf(type), labelStyle);
        GUI.color = Color.white;
    }
}
