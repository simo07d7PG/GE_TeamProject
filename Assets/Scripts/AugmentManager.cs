using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum AugmentType
{
    Cooling,
    Vaccine,
    ReverseEngineering,
    Optimization,
    Benchmark,
}

public class AugmentManager : MonoBehaviour
{
    public static AugmentManager Instance { get; private set; }

    public int maxTier = 3;
    public float[] tierBonus = { 0.10f, 0.25f, 0.60f };

    public GameObject panel;
    public Text headerText;
    public Text ownedText;
    public Button[] cardButtons;
    public Text[] nameTexts;
    public Text[] tierTexts;
    public Text[] descTexts;
    public Text[] valueTexts;
    public Text[] codeTexts;
    public Text[] fixedTexts;

    private static readonly string[] Names =
    {
        "최고오급 팬",
        "V4 최신 백신",
        "리버스 엔지니어링",
        "극최적화 연산",
        "벤치마크 MAXX",
    };

    private static readonly string[] Codes =
    {
        "[FAN]",
        "[AV]",
        "[REV]",
        "[OPT]",
        "[BENCH]",
    };

    private readonly int[] tiers = new int[5];
    private readonly List<int> fixedTypes = new List<int>();
    private readonly int[] offer = new int[3];
    private PlayerShooter shooter;
    private string rowLabels;
    private string rowValues;

    void Awake()
    {
        Instance = this;
        if (panel != null) panel.SetActive(false);
    }

    public int TierOf(AugmentType type)
    {
        return tiers[(int)type];
    }

    public float Bonus(AugmentType type)
    {
        return BonusAt(tiers[(int)type]);
    }

    float BonusAt(int tier)
    {
        if (tier <= 0) return 0f;
        return tierBonus[Mathf.Min(tier, tierBonus.Length) - 1];
    }

    public bool Open(int phaseCleared)
    {
        List<int> candidates = new List<int>();
        for (int i = 0; i < tiers.Length; i++)
        {
            if (!fixedTypes.Contains(i)) candidates.Add(i);
        }

        bool pickable = false;
        for (int s = 0; s < offer.Length; s++)
        {
            if (s < fixedTypes.Count)
            {
                offer[s] = fixedTypes[s];
            }
            else if (candidates.Count > 0)
            {
                int c = Random.Range(0, candidates.Count);
                offer[s] = candidates[c];
                candidates.RemoveAt(c);
            }
            else
            {
                offer[s] = -1;
            }
            if (offer[s] >= 0 && tiers[offer[s]] < maxTier) pickable = true;
        }

        if (!pickable || panel == null) return false;

        if (headerText != null) headerText.text = "STAGE " + phaseCleared + " CLEAR";
        for (int s = 0; s < offer.Length; s++) FillCard(s);
        UpdateOwned();
        panel.SetActive(true);
        return true;
    }

    void FillCard(int slot)
    {
        int type = offer[slot];
        bool has = type >= 0;
        if (slot < cardButtons.Length && cardButtons[slot] != null)
        {
            cardButtons[slot].gameObject.SetActive(has);
            cardButtons[slot].interactable = has && tiers[type] < maxTier;
        }
        if (!has) return;

        int tier = tiers[type];
        bool max = tier >= maxTier;
        Color main = max ? HudStyle.DIM : HudStyle.TEXT;
        if (codeTexts != null && slot < codeTexts.Length && codeTexts[slot] != null) codeTexts[slot].text = Codes[type];
        if (fixedTexts != null && slot < fixedTexts.Length && fixedTexts[slot] != null) fixedTexts[slot].text = fixedTypes.Contains(type) ? "[고정]" : "";
        if (nameTexts[slot] != null)
        {
            nameTexts[slot].text = Names[type];
            nameTexts[slot].color = main;
        }
        if (tierTexts[slot] != null) tierTexts[slot].text = Pips(tier, max);
        Describe((AugmentType)type, tier, max);
        if (descTexts[slot] != null) descTexts[slot].text = rowLabels;
        if (valueTexts != null && slot < valueTexts.Length && valueTexts[slot] != null)
        {
            valueTexts[slot].text = rowValues;
            valueTexts[slot].color = main;
        }
    }

    string Tag(Color color, string text)
    {
        return "<color=#" + ColorUtility.ToHtmlStringRGB(color) + ">" + text + "</color>";
    }

    string Pips(int tier, bool max)
    {
        string pips = "";
        for (int i = 0; i < maxTier; i++)
        {
            if (max) pips += Tag(HudStyle.DIM, "■");
            else pips += i < tier ? Tag(HudStyle.ACCENT, "■") : Tag(HudStyle.DIM, "□");
        }
        string info = max ? "MAX" : "T" + tier + " → T" + (tier + 1);
        return pips + "  <size=11>" + Tag(HudStyle.DIM, info) + "</size>";
    }

    string Pc(string sign, float cur, float next, bool arrow)
    {
        string a = cur <= 0f ? "0%" : sign + Mathf.RoundToInt(cur * 100f) + "%";
        if (!arrow) return a;
        return Tag(HudStyle.DIM, a + " → ") + sign + Mathf.RoundToInt(next * 100f) + "%";
    }

    void Row(string label, string value)
    {
        if (rowLabels.Length > 0)
        {
            rowLabels += "\n";
            rowValues += "\n";
        }
        rowLabels += label;
        rowValues += value;
    }

    void Describe(AugmentType type, int tier, bool max)
    {
        float cur = BonusAt(tier);
        float next = BonusAt(tier + 1);
        bool arrow = !max;
        rowLabels = "";
        rowValues = "";
        switch (type)
        {
            case AugmentType.Cooling:
                Row("열 상승", Pc("-", cur, next, arrow));
                Row("냉각", Pc("+", cur, next, arrow));
                Row("총기 발열", Pc("-", cur, next, arrow));
                Row("총기 냉각", Pc("+", cur, next, arrow));
                break;
            case AugmentType.Vaccine:
                Row("공격력", Pc("+", cur, next, arrow));
                Row("광고 팝업 차단", Pc("", cur, next, arrow));
                break;
            case AugmentType.ReverseEngineering:
                Row("아이템 드롭률", Pc("+", cur, next, arrow));
                break;
            case AugmentType.Optimization:
                if (tier == 0) Row("연사 기능", "추가");
                Row("연사 속도", Pc("+", cur, next, arrow));
                Row("탄속", Pc("+", cur, next, arrow));
                break;
            default:
                Row("모든 열 상승", Pc("+", cur, next, arrow));
                Row("콤보 점수 배율", Pc("+", cur, next, arrow));
                break;
        }
    }

    void UpdateOwned()
    {
        if (ownedText == null) return;

        string list = Tag(HudStyle.DIM, "설치됨") + "  ";
        if (fixedTypes.Count == 0) list += Tag(HudStyle.DIM, "없음");
        for (int i = 0; i < fixedTypes.Count; i++)
        {
            int t = fixedTypes[i];
            if (i > 0) list += Tag(HudStyle.DIM, " · ");
            list += Tag(HudStyle.TEXT, Names[t]) + " " + Tag(HudStyle.DIM, "T" + tiers[t]);
        }
        ownedText.text = list;
    }

    public void Choose(int slot)
    {
        if (slot < 0 || slot >= offer.Length) return;

        int type = offer[slot];
        if (type < 0 || tiers[type] >= maxTier) return;

        tiers[type]++;
        if (!fixedTypes.Contains(type)) fixedTypes.Add(type);
        if (panel != null) panel.SetActive(false);
        ApplyAll();
        if (WaveManager.Instance != null) WaveManager.Instance.ContinueAfterAugment();
    }

    public void ResetAll()
    {
        for (int i = 0; i < tiers.Length; i++) tiers[i] = 0;
        fixedTypes.Clear();
        if (panel != null) panel.SetActive(false);
        ApplyAll();
    }

    public void ApplyAll()
    {
        float cooling = Bonus(AugmentType.Cooling);
        float vaccine = Bonus(AugmentType.Vaccine);
        float reverse = Bonus(AugmentType.ReverseEngineering);
        float optimize = Bonus(AugmentType.Optimization);
        float bench = Bonus(AugmentType.Benchmark);
        float rise = (1f - cooling) * (1f + bench);

        HeatGauge heat = GameManager.Instance != null ? GameManager.Instance.heatGauge : null;
        if (heat != null)
        {
            heat.riseMultiplier = rise;
            heat.coolMultiplier = 1f + cooling;
        }

        if (shooter == null) shooter = FindFirstObjectByType<PlayerShooter>();
        if (shooter != null)
        {
            shooter.gunHeatMultiplier = rise;
            shooter.gunCoolMultiplier = 1f + cooling;
            shooter.damageMultiplier = 1f + vaccine;
            shooter.autoFire = tiers[(int)AugmentType.Optimization] >= 1;
            shooter.fireRateMultiplier = 1f + optimize;
            shooter.bulletSpeedMultiplier = 1f + optimize;
        }

        if (AdPopups.Instance != null) AdPopups.Instance.blockChance = vaccine;
        if (PlayerUpgrades.Instance != null) PlayerUpgrades.Instance.dropMultiplier = 1f + reverse;
        if (ComboSystem.Instance != null) ComboSystem.Instance.scoreBonus = bench;
    }
}
