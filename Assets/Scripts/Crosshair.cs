using UnityEngine;

public class Crosshair : MonoBehaviour
{
    public static Crosshair Instance { get; private set; }

    public float gap = 7f;
    public float lineLength = 8f;
    public float thickness = 2f;
    public float aimRange = 100f;

    public float hitMarkerTime = 0.1f;
    public float killMarkerTime = 0.2f;
    public float markerGap = 6f;
    public float markerLength = 9f;

    private Camera mainCam;
    private bool aimingEnemy;
    private float markerTimer;
    private bool markerKill;

    public bool AimingEnemy => aimingEnemy;
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
        if (Time.timeScale == 0f) return;

        if (mainCam == null) mainCam = Camera.main;
        if (mainCam == null) return;

        Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        aimingEnemy = Physics.Raycast(ray, out hit, aimRange, ~0, QueryTriggerInteraction.Ignore)
            && hit.collider.CompareTag("Enemy");
    }

    public void OnPlayerHit(bool killed)
    {
        if (!killed && markerKill && markerTimer > 0f) return;

        markerKill = killed;
        markerTimer = killed ? killMarkerTime : hitMarkerTime;
    }

    void OnGUI()
    {
        if (Time.timeScale == 0f) return;

        Vector2 center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

        GUI.color = aimingEnemy ? Color.red : new Color(1f, 1f, 1f, 0.85f);
        for (int i = 0; i < 4; i++) DrawLine(center, i * 90f, gap, lineLength, thickness);

        if (markerTimer > 0f)
        {
            float size = markerKill ? 1.4f : 1f;
            GUI.color = markerKill ? Color.yellow : Color.white;
            for (int i = 0; i < 4; i++) DrawLine(center, 45f + i * 90f, markerGap * size, markerLength * size, thickness * size);
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
