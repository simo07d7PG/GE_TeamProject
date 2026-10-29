using UnityEngine;

public class ComboSystem : MonoBehaviour
{
    public static ComboSystem Instance { get; private set; }

    public float comboWindow = 3f;
    public int killsPerStep = 3;
    public int maxMultiplier = 5;
    public float scoreBonus;

    [Header("Kill Effect")]
    public float flashTime = 0.15f;
    public float popTime = 0.18f;
    public float popScale = 0.15f;
    public float sweepTime = 0.2f;

    private int combo;
    private float timer;
    private float kickTime = -999f;
    private bool steppedUp;

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
    }

    public void ResetCombo()
    {
        combo = 0;
        timer = 0f;
    }

    void OnGUI()
    {
        if (combo < 2 || Time.timeScale == 0f) return;

        float width = 198f;
        float y = WaveManager.StackBottom + HudStyle.Grid + EnemyBoss.BarCount * EnemyBoss.BarStride;
        Rect back = new Rect((Screen.width - width) * 0.5f, y, width, 33f);

        float since = Time.time - kickTime;
        float flash = 1f - Mathf.Clamp01(since / flashTime);
        float pop = 1f + popScale * Mathf.Sin(Mathf.Clamp01(since / popTime) * Mathf.PI);

        HudStyle.Frame(back, HudStyle.BG, Color.Lerp(HudStyle.LINE, Color.white, flash));

        float pad = HudStyle.Grid;
        HudStyle.Text(new Rect(back.x + pad, back.y, 99f, back.height), "COMBO " + combo, 11, Color.Lerp(HudStyle.TEXT, Color.white, flash), TextAnchor.MiddleLeft, false);

        Rect number = new Rect(back.xMax - pad - 77f, back.y, 77f, back.height);
        Matrix4x4 saved = GUI.matrix;
        GUIUtility.ScaleAroundPivot(new Vector2(pop, pop), new Vector2(number.xMax - 22f, number.center.y));
        HudStyle.Text(number, "x" + TotalMultiplier.ToString("0.##"), 22, Color.Lerp(HudStyle.ACCENT, Color.white, flash), TextAnchor.MiddleRight, true);
        GUI.matrix = saved;

        HudStyle.Bar(new Rect(back.x, back.yMax + 1f, width, 2f), timer / comboWindow, HudStyle.ACCENT);

        if (steppedUp && since < sweepTime * 1.75f)
        {
            Color sweep = HudStyle.ACCENT;
            sweep.a = 1f - Mathf.Clamp01((since - sweepTime) / (sweepTime * 0.75f));
            HudStyle.Fill(new Rect(back.x, back.yMax + 5f, width * Mathf.Clamp01(since / sweepTime), 2f), sweep);
        }
    }
}
