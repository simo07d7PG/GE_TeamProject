using System.Collections.Generic;
using UnityEngine;

public static class HudStyle
{
    public static readonly Color BG = new Color(13f / 255f, 16f / 255f, 20f / 255f, 0.88f);
    public static readonly Color LINE = new Color32(0x2C, 0x33, 0x3B, 255);
    public static readonly Color TEXT = new Color32(0xD5, 0xDC, 0xE3, 255);
    public static readonly Color DIM = new Color32(0x6E, 0x7A, 0x86, 255);
    public static readonly Color ACCENT = new Color32(0xFF, 0xB5, 0x47, 255);
    public static readonly Color DANGER = new Color32(0xFF, 0x4B, 0x3E, 255);
    public static readonly Color COOL = new Color32(0xB9, 0xC0, 0xC7, 255);
    public static readonly Color HP = new Color32(0x9A, 0xD7, 0xB5, 255);

    public const float Grid = 11f;

    private static Font fallback;
    private static readonly Dictionary<int, GUIStyle> styles = new Dictionary<int, GUIStyle>();

    public static Font Font
    {
        get
        {
            if (GameManager.Instance != null && GameManager.Instance.hudFont != null) return GameManager.Instance.hudFont;
            return Fallback();
        }
    }

    public static Font BoldFont
    {
        get
        {
            if (GameManager.Instance != null && GameManager.Instance.hudFontBold != null) return GameManager.Instance.hudFontBold;
            return Font;
        }
    }

    static Font Fallback()
    {
        if (fallback == null) fallback = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return fallback;
    }

    public static GUIStyle Label(int size, Color color, TextAnchor alignment, bool bold)
    {
        int key = size * 100 + (int)alignment * 2 + (bold ? 1 : 0);
        GUIStyle style;
        if (!styles.TryGetValue(key, out style) || style == null)
        {
            style = new GUIStyle(GUI.skin.label);
            style.alignment = alignment;
            style.fontSize = size;
            style.fontStyle = FontStyle.Normal;
            style.wordWrap = false;
            style.clipping = TextClipping.Overflow;
            style.padding = new RectOffset(0, 0, 0, 0);
            styles[key] = style;
        }
        style.font = bold ? BoldFont : Font;
        style.normal.textColor = color;
        return style;
    }

    public static void Text(Rect rect, string text, int size, Color color, TextAnchor alignment, bool bold)
    {
        GUI.color = Color.white;
        GUI.Label(Snap(rect), text, Label(size, color, alignment, bold));
    }

    public static void Fill(Rect rect, Color color)
    {
        GUI.color = color;
        GUI.DrawTexture(Snap(rect), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    public static void Frame(Rect rect, Color fill, Color line)
    {
        rect = Snap(rect);
        Fill(rect, fill);
        Fill(new Rect(rect.x, rect.y, rect.width, 1f), line);
        Fill(new Rect(rect.x, rect.yMax - 1f, rect.width, 1f), line);
        Fill(new Rect(rect.x, rect.y + 1f, 1f, rect.height - 2f), line);
        Fill(new Rect(rect.xMax - 1f, rect.y + 1f, 1f, rect.height - 2f), line);
    }

    public static void Bar(Rect rect, float ratio, Color color)
    {
        rect = Snap(rect);
        Fill(rect, LINE);
        Fill(new Rect(rect.x, rect.y, Mathf.Round(rect.width * Mathf.Clamp01(ratio)), rect.height), color);
    }

    public static void BarUp(Rect rect, float ratio, Color color)
    {
        rect = Snap(rect);
        Fill(rect, LINE);
        float height = Mathf.Round(rect.height * Mathf.Clamp01(ratio));
        Fill(new Rect(rect.x, rect.yMax - height, rect.width, height), color);
    }

    public static Rect Snap(Rect rect)
    {
        return new Rect(Mathf.Round(rect.x), Mathf.Round(rect.y), Mathf.Round(rect.width), Mathf.Round(rect.height));
    }
}
