using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }
    public static bool SpawnPaused { get; private set; }

    public int totalPhases = 6;
    public int midBossPhase = 3;
    public float phaseDuration = 40f;
    public int finalBossHpMultiplier = 4;
    public int maxAliveStep = 2;
    public Vector3 bossSpawn = new Vector3(20f, 2f, -10f);
    public float bannerTime = 2f;

    private bool active;
    private float timer;
    private int phase;
    private string banner = "";
    private Color bannerColor;
    private float bannerUntil;
    private const float TagHeight = 33f;
    private const float BossBarHeight = 44f;
    private const float BannerHeight = 22f;
    private GameObject bossPrefab;
    private GameObject boss;
    private Health finalBoss;
    private readonly Dictionary<EnemySpawner, int> baseMaxAlive = new Dictionary<EnemySpawner, int>();

    public int CurrentPhase => phase;
    public bool FinalBossActive => active && phase == totalPhases && finalBoss != null;
    public Health FinalBoss => finalBoss != null ? finalBoss : null;
    public bool BannerVisible => active && Time.time < bannerUntil;

    public static float StackBottom
    {
        get
        {
            if (Instance == null) return HudStyle.Grid + TagHeight;
            float bottom = HudStyle.Grid + (Instance.FinalBossActive ? BossBarHeight : TagHeight);
            if (Instance.BannerVisible) bottom += HudStyle.Grid + BannerHeight;
            return bottom;
        }
    }

    public static void Create()
    {
        if (Instance != null) return;
        new GameObject("WaveManager").AddComponent<WaveManager>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        bossPrefab = Resources.Load<GameObject>("EnemyBoss");
    }

    public void Begin()
    {
        baseMaxAlive.Clear();
        foreach (EnemySpawner sp in FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None))
        {
            baseMaxAlive[sp] = sp.maxAlive;
        }

        if (boss != null) Destroy(boss);
        boss = null;
        finalBoss = null;
        phase = 0;
        active = true;
        StartPhase();
    }

    public void End()
    {
        active = false;
        SpawnPaused = false;
        foreach (KeyValuePair<EnemySpawner, int> pair in baseMaxAlive)
        {
            if (pair.Key != null) pair.Key.maxAlive = pair.Value;
        }
    }

    bool IsBossPhase()
    {
        return phase == midBossPhase || phase == totalPhases;
    }

    void StartPhase()
    {
        phase++;
        SpawnPaused = false;
        timer = phaseDuration;
        boss = null;
        finalBoss = null;

        foreach (KeyValuePair<EnemySpawner, int> pair in baseMaxAlive)
        {
            if (pair.Key != null) pair.Key.maxAlive = pair.Value + (phase - 1) * maxAliveStep;
        }

        if (phase == totalPhases)
        {
            ShowBanner("FINAL BOSS", HudStyle.DANGER);
            SpawnBoss(finalBossHpMultiplier);
            if (boss != null) finalBoss = boss.GetComponent<Health>();
        }
        else if (phase == midBossPhase)
        {
            ShowBanner("BOSS DETECTED", HudStyle.DANGER);
            SpawnBoss(1);
        }
        else
        {
            ShowBanner("STAGE " + phase + "  START", HudStyle.ACCENT);
        }
    }

    void EndPhase()
    {
        if (AugmentManager.Instance != null && AugmentManager.Instance.Open(phase))
        {
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            StartPhase();
        }
    }

    public void ContinueAfterAugment()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        if (active) StartPhase();
    }

    void SpawnBoss(int hpMultiplier)
    {
        if (bossPrefab == null) return;

        boss = Instantiate(bossPrefab, bossSpawn, Quaternion.identity);
        Health hp = boss.GetComponent<Health>();
        if (hp != null) hp.maxHp *= hpMultiplier;
    }

    void ShowBanner(string text, Color color)
    {
        banner = text;
        bannerColor = color;
        bannerUntil = Time.time + bannerTime;
    }

    void Update()
    {
        if (!active || Time.timeScale == 0f) return;

        if (IsBossPhase())
        {
            if (boss != null) return;

            if (phase == totalPhases)
            {
                active = false;
                SpawnPaused = false;
                if (GameManager.Instance != null) GameManager.Instance.GameClear();
            }
            else
            {
                EndPhase();
            }
            return;
        }

        timer -= Time.deltaTime;
        if (timer > 0f) return;

        EndPhase();
    }

    void DrawFinalBossBar(float y)
    {
        float width = Screen.width * 0.5f;
        Rect back = new Rect((Screen.width - width) * 0.5f, y, width, BossBarHeight);
        HudStyle.Frame(back, HudStyle.BG, HudStyle.LINE);

        float pad = HudStyle.Grid;
        HudStyle.Text(new Rect(back.x + pad, back.y + 9f, width * 0.5f, 13f), "FINAL BOSS", 11, HudStyle.DIM, TextAnchor.MiddleLeft, false);
        string hp = "HP " + Mathf.Max(0, finalBoss.currentHp) + "/" + finalBoss.maxHp;
        HudStyle.Text(new Rect(back.xMax - pad - width * 0.5f, back.y + 9f, width * 0.5f, 13f), hp, 11, HudStyle.TEXT, TextAnchor.MiddleRight, false);

        float ratio = Mathf.Clamp01((float)finalBoss.currentHp / finalBoss.maxHp);
        HudStyle.Bar(new Rect(back.x + pad, back.y + 24f, width - pad * 2f, 10f), ratio, HudStyle.DANGER);
    }

    void DrawStageTag(float y)
    {
        float width = 198f;
        Rect back = new Rect((Screen.width - width) * 0.5f, y, width, TagHeight);
        HudStyle.Frame(back, HudStyle.BG, HudStyle.LINE);

        float pad = HudStyle.Grid;
        HudStyle.Text(new Rect(back.x + pad, back.y, 88f, back.height), "STAGE " + phase + "/" + totalPhases, 11, HudStyle.DIM, TextAnchor.MiddleLeft, false);

        string value;
        Color color;
        if (IsBossPhase())
        {
            value = "BOSS";
            color = HudStyle.DANGER;
        }
        else
        {
            int seconds = Mathf.CeilToInt(Mathf.Max(0f, timer));
            value = (seconds / 60).ToString("00") + ":" + (seconds % 60).ToString("00");
            color = HudStyle.TEXT;
        }
        HudStyle.Text(new Rect(back.xMax - pad - 88f, back.y, 88f, back.height), value, 22, color, TextAnchor.MiddleRight, true);
    }

    void DrawBanner(float y)
    {
        float width = 198f;
        Rect back = new Rect((Screen.width - width) * 0.5f, y, width, BannerHeight);

        float left = bannerUntil - Time.time;
        float fade = Mathf.Clamp01(left / 0.5f);
        bool flash = bannerTime - left < 0.3f;

        Color fill = HudStyle.BG;
        fill.a *= fade;
        Color line = flash ? bannerColor : HudStyle.LINE;
        line.a *= fade;
        Color text = bannerColor;
        text.a *= fade;

        HudStyle.Frame(back, fill, line);
        HudStyle.Fill(new Rect(back.x + 1f, back.y + 1f, 2f, back.height - 2f), text);
        HudStyle.Text(back, banner, 11, text, TextAnchor.MiddleCenter, true);
    }

    void OnGUI()
    {
        if (!active || Time.timeScale == 0f) return;

        float y = HudStyle.Grid;
        if (FinalBossActive)
        {
            DrawFinalBossBar(y);
            y += BossBarHeight;
        }
        else if (phase != totalPhases)
        {
            DrawStageTag(y);
            y += TagHeight;
        }

        if (BannerVisible) DrawBanner(y + HudStyle.Grid);
    }
}
