using System.Collections.Generic;
using UnityEngine;

public class AdPopups : MonoBehaviour
{
    private class Popup
    {
        public Object owner;
        public Rect rect;
        public float expireTime;
        public string text;
    }

    public static AdPopups Instance { get; private set; }

    private const int MaxPopups = 6;
    private static readonly string[] Texts =
    {
        "YOU WON\n$1,000,000!!",
        "FREE VIRUS\nSCAN NOW",
        "HOT SINGLES\nNEAR YOU",
        "CLICK HERE\nTO CLAIM",
    };

    private readonly List<Popup> popups = new List<Popup>();
    private GUIStyle bodyStyle;

    public static void Create()
    {
        if (Instance != null) return;
        new GameObject("AdPopups").AddComponent<AdPopups>();
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

    public void Spawn(Object owner, float duration)
    {
        if (popups.Count >= MaxPopups) return;

        Vector2 size = new Vector2(220f, 110f);
        Rect rect;
        int tries = 0;
        do
        {
            rect = new Rect(
                Random.Range(20f, Mathf.Max(21f, Screen.width - size.x - 20f)),
                Random.Range(20f, Mathf.Max(21f, Screen.height - size.y - 20f)),
                size.x, size.y);
            tries++;
        }
        while (tries < 10 && rect.Contains(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f)));

        popups.Add(new Popup
        {
            owner = owner,
            rect = rect,
            expireTime = Time.time + duration,
            text = Texts[Random.Range(0, Texts.Length)],
        });
        Fx.Blip(Camera.main != null ? Camera.main.transform.position : Vector3.zero, 880f, 0.1f, 0.4f);
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;

        popups.RemoveAll(p => p.owner == null || Time.time >= p.expireTime);
    }

    void OnGUI()
    {
        if (Time.timeScale == 0f || popups.Count == 0) return;

        if (bodyStyle == null)
        {
            bodyStyle = new GUIStyle(GUI.skin.label);
            bodyStyle.alignment = TextAnchor.MiddleCenter;
            bodyStyle.fontSize = 20;
            bodyStyle.fontStyle = FontStyle.Bold;
            bodyStyle.normal.textColor = Color.black;
        }

        foreach (Popup p in popups)
        {
            GUI.color = new Color(0.9f, 0.9f, 0.9f, 0.95f);
            GUI.DrawTexture(p.rect, Texture2D.whiteTexture);

            Rect title = new Rect(p.rect.x, p.rect.y, p.rect.width, 22f);
            GUI.color = new Color(0.1f, 0.2f, 0.8f, 1f);
            GUI.DrawTexture(title, Texture2D.whiteTexture);

            GUI.color = Color.white;
            GUI.Label(new Rect(title.x + 6f, title.y, 120f, 22f), "AD");
            GUI.color = Color.red;
            GUI.DrawTexture(new Rect(title.xMax - 22f, title.y + 2f, 18f, 18f), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(new Rect(title.xMax - 20f, title.y, 18f, 22f), "X");

            GUI.color = Color.white;
            GUI.Label(new Rect(p.rect.x, p.rect.y + 22f, p.rect.width, p.rect.height - 22f), p.text, bodyStyle);
        }
        GUI.color = Color.white;
    }
}
