using UnityEngine;

public class Crosshair : MonoBehaviour
{
    public static Crosshair Instance { get; private set; }

    public float thickness = 2f;

    public float hitMarkerTime = 0.1f;
    public float killMarkerTime = 0.2f;
    public float markerGap = 6f;
    public float markerLength = 9f;

    private float markerTimer;
    private bool markerKill;

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

    void Update()
    {
        if (markerTimer > 0f) markerTimer -= Time.unscaledDeltaTime;
    }

    public void OnPlayerHit(bool killed)
    {
        if (!killed && markerKill && markerTimer > 0f) return;

        markerKill = killed;
        markerTimer = killed ? killMarkerTime : hitMarkerTime;
    }

    void OnGUI()
    {
        if (Time.timeScale == 0f || markerTimer <= 0f) return;

        Vector2 center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        float size = markerKill ? 1.4f : 1f;
        GUI.color = markerKill ? Color.yellow : Color.white;
        for (int i = 0; i < 4; i++) DrawLine(center, 45f + i * 90f, markerGap * size, markerLength * size, thickness * size);
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
