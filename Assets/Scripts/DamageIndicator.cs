using System.Collections.Generic;
using UnityEngine;

public class DamageIndicator : MonoBehaviour
{
    public static DamageIndicator Instance { get; private set; }

    public float indicatorTime = 1f;
    public float arcAngle = 40f;
    public float arcThickness = 10f;
    public float flashTime = 0.4f;
    public float flashAlpha = 0.55f;
    public float heartbeatRate = 1.2f;

    private readonly List<Vector3> hitDirections = new List<Vector3>();
    private readonly List<float> hitTimes = new List<float>();
    private float flashTimer;
    private Health player;
    private Camera mainCam;
    private Texture2D vignette;

    public int IndicatorCount => hitDirections.Count;
    public bool LowHp => player != null && player.currentHp == 1;
    public float VignetteAlpha
    {
        get
        {
            float alpha = flashAlpha * Mathf.Clamp01(flashTimer / flashTime);
            if (LowHp) alpha = Mathf.Max(alpha, 0.2f + 0.35f * Heartbeat());
            return alpha;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Create()
    {
        if (Instance != null) return;
        new GameObject("DamageIndicator").AddComponent<DamageIndicator>();
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

    void OnEnable()
    {
        Health.OnPlayerDamaged += OnPlayerDamaged;
    }

    void OnDisable()
    {
        Health.OnPlayerDamaged -= OnPlayerDamaged;
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;

        if (flashTimer > 0f) flashTimer -= Time.deltaTime;
        for (int i = hitTimes.Count - 1; i >= 0; i--)
        {
            hitTimes[i] -= Time.deltaTime;
            if (hitTimes[i] > 0f) continue;
            hitTimes.RemoveAt(i);
            hitDirections.RemoveAt(i);
        }
    }

    void OnPlayerDamaged(Health hp, Vector3 hitDirection)
    {
        player = hp;
        flashTimer = flashTime;

        hitDirection.y = 0f;
        if (hitDirection.sqrMagnitude < 0.001f) return;
        hitDirections.Add(-hitDirection.normalized);
        hitTimes.Add(indicatorTime);
    }

    float Heartbeat()
    {
        float t = Mathf.Repeat(Time.time * heartbeatRate, 1f);
        float beat = Mathf.Exp(-Mathf.Pow((t - 0.1f) * 18f, 2f));
        beat += 0.6f * Mathf.Exp(-Mathf.Pow((t - 0.3f) * 18f, 2f));
        return Mathf.Clamp01(beat);
    }

    void OnGUI()
    {
        if (Time.timeScale == 0f) return;

        if (player == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) player = p.GetComponent<Health>();
        }
        if (player != null && player.currentHp <= 0) return;

        float alpha = VignetteAlpha;
        if (alpha > 0.01f)
        {
            if (vignette == null) vignette = CreateVignette();
            GUI.color = new Color(1f, 0f, 0f, alpha);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), vignette);
        }

        if (hitDirections.Count > 0)
        {
            if (mainCam == null) mainCam = Camera.main;
            if (mainCam != null) DrawArcs();
        }
        GUI.color = Color.white;
    }

    void DrawArcs()
    {
        Vector3 forward = mainCam.transform.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.001f) return;

        Vector2 center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        float radius = Mathf.Min(Screen.width, Screen.height) * 0.42f;
        int segments = 10;
        float segmentLength = radius * arcAngle * Mathf.Deg2Rad / segments + 1f;

        for (int i = 0; i < hitDirections.Count; i++)
        {
            float angle = Vector3.SignedAngle(forward, hitDirections[i], Vector3.up);
            GUI.color = new Color(1f, 0.15f, 0.1f, 0.9f * Mathf.Clamp01(hitTimes[i] / indicatorTime));

            for (int s = 0; s < segments; s++)
            {
                float a = angle - arcAngle * 0.5f + arcAngle * (s + 0.5f) / segments;
                Matrix4x4 saved = GUI.matrix;
                GUIUtility.RotateAroundPivot(a, center);
                GUI.DrawTexture(new Rect(center.x - segmentLength * 0.5f, center.y - radius - arcThickness * 0.5f, segmentLength, arcThickness), Texture2D.whiteTexture);
                GUI.matrix = saved;
            }
        }
    }

    Texture2D CreateVignette()
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x + 0.5f) / size * 2f - 1f;
                float dy = (y + 0.5f) / size * 2f - 1f;
                float d = Mathf.Sqrt(dx * dx + dy * dy) / Mathf.Sqrt(2f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.45f, 1f, d))));
            }
        }
        tex.Apply();
        return tex;
    }
}
