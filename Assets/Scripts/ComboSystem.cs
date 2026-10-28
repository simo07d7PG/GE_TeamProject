using UnityEngine;

public class ComboSystem : MonoBehaviour
{
    public static ComboSystem Instance { get; private set; }

    public float comboWindow = 3f;
    public int killsPerStep = 3;
    public int maxMultiplier = 5;

    private int combo;
    private float timer;
    private GUIStyle style;

    public static int Multiplier
    {
        get
        {
            if (Instance == null) return 1;
            return Mathf.Min(1 + Instance.combo / Instance.killsPerStep, Instance.maxMultiplier);
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Create()
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
        combo++;
        timer = comboWindow;
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

        GUI.color = Color.Lerp(Color.white, Color.yellow, (Multiplier - 1) / (float)Mathf.Max(1, maxMultiplier - 1));
        GUI.Label(area, "COMBO " + combo + "  x" + Multiplier, style);

        GUI.color = new Color(0f, 0f, 0f, 0.6f);
        GUI.DrawTexture(new Rect(area.x, area.yMax, width, 6f), Texture2D.whiteTexture);
        GUI.color = Color.yellow;
        GUI.DrawTexture(new Rect(area.x, area.yMax, width * Mathf.Clamp01(timer / comboWindow), 6f), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }
}
