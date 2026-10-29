using UnityEngine;

public class Crosshair : MonoBehaviour
{
    public static Crosshair Instance { get; private set; }

    public float thickness = 2f;

    public float hitMarkerTime = 0.1f;
    public float killMarkerTime = 0.2f;
    public float markerGap = 6f;
    public float markerLength = 9f;

    [Header("Gun Heat Gauge")]
    public float gaugeOffset = 22f;
    public float gaugeWidth = 6f;
    public float gaugeHeight = 40f;
    public float hitSoundVolume = 0.5f;

    private float markerTimer;
    private bool markerKill;
    private PlayerShooter shooter;
    private GUIStyle labelStyle;

    public bool MarkerVisible => markerTimer > 0f;
    public bool MarkerKill => markerKill;

    public static void Create()
    {
        if (Instance != null) return;
        new GameObject("Crosshair HUD").AddComponent<Crosshair>();
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

    void Start()
    {
        shooter = FindFirstObjectByType<PlayerShooter>();
    }

    void Update()
    {
        if (markerTimer > 0f) markerTimer -= Time.unscaledDeltaTime;
    }

    public void OnPlayerHit(bool killed)
    {
        Fx.HitTick(killed, hitSoundVolume);

        if (!killed && markerKill && markerTimer > 0f) return;

        markerKill = killed;
        markerTimer = killed ? killMarkerTime : hitMarkerTime;
    }

    void OnGUI()
    {
        if (Time.timeScale == 0f) return;

        Vector2 center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

        if (markerTimer > 0f)
        {
            float size = markerKill ? 1.4f : 1f;
            GUI.color = markerKill ? Color.yellow : Color.white;
            for (int i = 0; i < 4; i++) DrawLine(center, 45f + i * 90f, markerGap * size, markerLength * size, thickness * size);
            GUI.color = Color.white;
        }

        DrawHeatGauge(center);
    }

    void DrawHeatGauge(Vector2 center)
    {
        if (shooter == null) return;

        Rect back = new Rect(center.x + gaugeOffset, center.y - gaugeHeight * 0.5f, gaugeWidth, gaugeHeight);
        GUI.color = new Color(0f, 0f, 0f, 0.6f);
        GUI.DrawTexture(back, Texture2D.whiteTexture);

        float ratio = shooter.GunHeatRatio;
        bool overheated = shooter.Overheated;
        bool blinkOn = Mathf.Repeat(Time.unscaledTime * 8f, 1f) < 0.5f;

        if (overheated) GUI.color = blinkOn ? Color.red : new Color(1f, 0.6f, 0.6f);
        else GUI.color = Color.Lerp(Color.cyan, new Color(1f, 0.5f, 0f), ratio);

        float fill = (gaugeHeight - 2f) * (overheated ? 1f : ratio);
        GUI.DrawTexture(new Rect(back.x + 1f, back.yMax - 1f - fill, gaugeWidth - 2f, fill), Texture2D.whiteTexture);

        if (overheated && blinkOn)
        {
            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(GUI.skin.label);
                labelStyle.fontSize = 11;
                labelStyle.fontStyle = FontStyle.Bold;
            }
            GUI.color = Color.red;
            GUI.Label(new Rect(back.xMax + 4f, center.y - 10f, 80f, 20f), "OVERHEAT", labelStyle);
        }
        GUI.color = Color.white;
    }

    void DrawLine(Vector2 center, float angle, float from, float length, float width)
    {
        Matrix4x4 saved = GUI.matrix;
        GUIUtility.RotateAroundPivot(angle, center);
        GUI.DrawTexture(new Rect(center.x + from, center.y - width * 0.5f, length, width), Texture2D.whiteTexture);
        GUI.matrix = saved;
    }
}
