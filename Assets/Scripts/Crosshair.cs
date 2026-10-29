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
    public float gaugeHeight = 44f;
    public float hitSoundVolume = 0.5f;

    private float markerTimer;
    private bool markerKill;
    private PlayerShooter shooter;

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
            GUI.color = markerKill ? HudStyle.ACCENT : HudStyle.TEXT;
            for (int i = 0; i < 4; i++) DrawLine(center, 45f + i * 90f, markerGap * size, markerLength * size, thickness);
            GUI.color = Color.white;
        }

        DrawHeatGauge(center);
    }

    void DrawHeatGauge(Vector2 center)
    {
        if (shooter == null) return;

        Rect back = new Rect(center.x + gaugeOffset, center.y - gaugeHeight * 0.5f, gaugeWidth, gaugeHeight);
        HudStyle.Frame(back, HudStyle.BG, HudStyle.LINE);

        float ratio = shooter.GunHeatRatio;
        bool overheated = shooter.Overheated;
        bool blinkOn = Mathf.Repeat(Time.unscaledTime * 8f, 1f) < 0.5f;

        Color fill = Color.Lerp(HudStyle.TEXT, HudStyle.ACCENT, ratio);
        if (overheated) fill = blinkOn ? HudStyle.DANGER : Color.Lerp(HudStyle.DANGER, HudStyle.LINE, 0.6f);

        HudStyle.BarUp(new Rect(back.x + 1f, back.y + 1f, back.width - 2f, back.height - 2f), overheated ? 1f : ratio, fill);

        if (overheated && blinkOn)
        {
            HudStyle.Text(new Rect(back.xMax + 6f, center.y - 7f, 80f, 13f), "OVERHEAT", 11, HudStyle.DANGER, TextAnchor.MiddleLeft, false);
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
