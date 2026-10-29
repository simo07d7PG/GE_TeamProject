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

    private bool active;
    private float timer;
    private int phase;
    private string banner = "";
    private float bannerUntil;
    private GUIStyle bannerStyle;
    private GUIStyle infoStyle;
    private GameObject bossPrefab;
    private GameObject boss;
    private Health finalBoss;
    private readonly Dictionary<EnemySpawner, int> baseMaxAlive = new Dictionary<EnemySpawner, int>();

    public int CurrentPhase => phase;
    public bool FinalBossActive => active && phase == totalPhases && finalBoss != null;
    public Health FinalBoss => finalBoss != null ? finalBoss : null;

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
            ShowBanner("PHASE " + phase + "  FINAL BOSS!", 2.5f);
            SpawnBoss(finalBossHpMultiplier);
            if (boss != null) finalBoss = boss.GetComponent<Health>();
        }
        else if (phase == midBossPhase)
        {
            ShowBanner("PHASE " + phase + "  BOSS!", 2.5f);
            SpawnBoss(1);
        }
        else
        {
            ShowBanner("PHASE " + phase, 2.5f);
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

    void ShowBanner(string text, float seconds)
    {
        banner = text;
        bannerUntil = Time.time + seconds;
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

    void OnGUI()
    {
        if (!active || Time.timeScale == 0f) return;

        if (bannerStyle == null)
        {
            bannerStyle = new GUIStyle(GUI.skin.label);
            bannerStyle.alignment = TextAnchor.MiddleCenter;
            bannerStyle.fontSize = 48;
            bannerStyle.fontStyle = FontStyle.Bold;

            infoStyle = new GUIStyle(GUI.skin.label);
            infoStyle.alignment = TextAnchor.MiddleRight;
            infoStyle.fontSize = 22;
            infoStyle.fontStyle = FontStyle.Bold;
        }

        string info = IsBossPhase() ? "PHASE " + phase + "  BOSS" : "PHASE " + phase + "  " + Mathf.CeilToInt(Mathf.Max(0f, timer)) + "s";
        GUI.color = Color.white;
        GUI.Label(new Rect(Screen.width - 310f, 10f, 300f, 32f), info, infoStyle);

        if (Time.time < bannerUntil)
        {
            GUI.color = Color.red;
            GUI.Label(new Rect(0f, Screen.height * 0.25f, Screen.width, 70f), banner, bannerStyle);
            GUI.color = Color.white;
        }
    }
}
