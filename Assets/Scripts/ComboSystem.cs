using UnityEngine;

public class ComboSystem : MonoBehaviour
{
    public static ComboSystem Instance { get; private set; }

    public float comboWindow = 3f;
    public int killsPerStep = 3;
    public int maxMultiplier = 5;
    public float scoreBonus;

    [Header("Kill Effect")]
    public float popTime = 0.25f;
    public float stepPopTime = 0.4f;
    public float popScale = 0.3f;
    public float stepPopScale = 0.7f;
    public int rayCount = 10;

    private int combo;
    private float timer;
    private GUIStyle style;
    private float kickTime = -999f;
    private bool steppedUp;
    private float rayOffset;

    public static int Multiplier
    {
        get
        {
            if (Instance == null) return 1;
            return Mathf.Min(1 + Instance.combo / Instance.killsPerStep, Instance.maxMultiplier);
        }
    }

    public static float TotalMultiplier
    {
        get
        {
            if (Instance == null) return 1f;
            return Multiplier * (1f + Instance.scoreBonus);
        }
    }

    public static void Create()
    {
        if (Instance != null) return;
        new GameObject("ComboSystem").AddComponent<ComboSystem>();
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

    void Update()
    {
        if (combo == 0 || Time.timeScale == 0f) return;

        timer -= Time.deltaTime;
        if (timer <= 0f) combo = 0;
    }

    public void AddKill()
    {
        int before = Multiplier;
        combo++;
        timer = comboWindow;

        kickTime = Time.time;
        steppedUp = Multiplier > before;
        rayOffset = Random.value * 30f;
    }

    public void ResetCombo()
    {
        combo = 0;
        timer = 0f;
    }

    void OnGUI()
    {
        if (combo < 2 || Time.timeScale == 0f) return;

        if (style == null)
        {
            style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 28;
            style.fontStyle = FontStyle.Bold;
        }

        float width = 260f;
        Rect area = new Rect((Screen.width - width) * 0.5f, 70f, width, 40f);

        float duration = steppedUp ? stepPopTime : popTime;
        float p = Mathf.Clamp01((Time.time - kickTime) / duration);
        float fade = 1f - p;
        float pop = 1f + (steppedUp ? stepPopScale : popScale) * fade * fade;

        Color baseColor = Color.Lerp(Color.white, Color.yellow, (Multiplier - 1) / (float)Mathf.Max(1, maxMultiplier - 1));
        Color flash = Multiplier < 3 ? new Color(1f, 0.9f, 0.2f) : Color.white;
        Vector2 center = area.center;

        if (p < 1f) DrawEffect(center, p, steppedUp);

        Matrix4x4 saved = GUI.matrix;
        GUIUtility.ScaleAroundPivot(new Vector2(pop, pop), center);
        GUI.color = Color.Lerp(baseColor, flash, fade);
        GUI.Label(area, "COMBO " + combo + "  x" + TotalMultiplier.ToString("0.##"), style);
        GUI.matrix = saved;

        GUI.color = new Color(0f, 0f, 0f, 0.6f);
        GUI.DrawTexture(new Rect(area.x, area.yMax, width, 6f), Texture2D.whiteTexture);
        GUI.color = Color.yellow;
        GUI.DrawTexture(new Rect(area.x, area.yMax, width * Mathf.Clamp01(timer / comboWindow), 6f), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    void DrawEffect(Vector2 center, float p, bool big)
    {
        float alpha = 1f - p;
        float spread = 1f - (1f - p) * (1f - p);
        int count = big ? rayCount * 2 : rayCount;

        GUI.color = new Color(1f, 0.9f, 0.3f, alpha);
        for (int i = 0; i < count; i++)
        {
            float angle = rayOffset + 360f / count * i;
            float rad = angle * Mathf.Deg2Rad;
            float length = 8f + 14f * alpha;
            Vector2 start = center + new Vector2(Mathf.Cos(rad) * (95f + 70f * spread), Mathf.Sin(rad) * (16f + 40f * spread));
            DrawRay(start, angle, length, 3f);
        }

        if (!big) return;

        GUI.color = new Color(1f, 1f, 1f, alpha);
        for (int i = 0; i < 24; i++)
        {
            float rad = (360f / 24f * i) * Mathf.Deg2Rad;
            Vector2 pos = center + new Vector2(Mathf.Cos(rad) * (120f + 110f * spread), Mathf.Sin(rad) * (24f + 60f * spread));
            GUI.DrawTexture(new Rect(pos.x - 2f, pos.y - 2f, 4f, 4f), Texture2D.whiteTexture);
        }
    }

    void DrawRay(Vector2 start, float angle, float length, float width)
    {
        Matrix4x4 saved = GUI.matrix;
        GUIUtility.RotateAroundPivot(angle, start);
        GUI.DrawTexture(new Rect(start.x, start.y - width * 0.5f, length, width), Texture2D.whiteTexture);
        GUI.matrix = saved;
    }
}
