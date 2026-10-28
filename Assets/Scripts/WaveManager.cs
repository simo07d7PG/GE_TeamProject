using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }
    public static bool SpawnPaused { get; private set; }

    public float waveDuration = 40f;
    public float restDuration = 5f;
    public int bossEvery = 3;
    public int maxAliveStep = 2;

    private bool active;
    private bool resting;
    private float timer;
    private int wave;
    private string banner = "";
    private float bannerUntil;
    private GUIStyle bannerStyle;
    private GUIStyle infoStyle;
    private GameObject bossPrefab;
    private readonly Dictionary<EnemySpawner, int> baseMaxAlive = new Dictionary<EnemySpawner, int>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Create()
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

        wave = 0;
        active = true;
        StartWave();
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

    void StartWave()
    {
        wave++;
        resting = false;
        SpawnPaused = false;
        timer = waveDuration;

        foreach (KeyValuePair<EnemySpawner, int> pair in baseMaxAlive)
        {
            if (pair.Key != null) pair.Key.maxAlive = pair.Value + (wave - 1) * maxAliveStep;
        }

        bool bossWave = bossEvery > 0 && wave % bossEvery == 0;
        ShowBanner(bossWave ? "WAVE " + wave + "  BOSS!" : "WAVE " + wave, 2.5f);
        if (bossWave) SpawnBoss();
    }

    void StartRest()
    {
        resting = true;
        SpawnPaused = true;
        timer = restDuration;
        ShowBanner("WAVE CLEAR", 2f);
    }

    void SpawnBoss()
    {
        if (bossPrefab == null) return;

        foreach (EnemySpawner sp in FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None))
        {
            if (sp.spawnPoints == null || sp.spawnPoints.Length == 0) continue;
            Transform point = sp.spawnPoints[Random.Range(0, sp.spawnPoints.Length)];
            Instantiate(bossPrefab, point.position, Quaternion.identity);
            return;
        }
    }

    void ShowBanner(string text, float seconds)
    {
        banner = text;
        bannerUntil = Time.time + seconds;
    }

    void Update()
    {
        if (!active || Time.timeScale == 0f) return;

        timer -= Time.deltaTime;
        if (timer > 0f) return;

        if (resting) StartWave();
        else StartRest();
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

        string info = resting ? "REST " : "WAVE " + wave + "  ";
        GUI.color = Color.white;
        GUI.Label(new Rect(Screen.width - 260f, 10f, 250f, 32f), info + Mathf.CeilToInt(Mathf.Max(0f, timer)) + "s", infoStyle);

        if (Time.time < bannerUntil)
        {
            GUI.color = resting ? Color.green : Color.red;
            GUI.Label(new Rect(0f, Screen.height * 0.25f, Screen.width, 70f), banner, bannerStyle);
            GUI.color = Color.white;
        }
    }
}
