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

    private static readonly string[] Names =
    {
        "최고오급 팬",
        "V4 최신 백신",
        "리버스 엔지니어링",
        "극최적화 연산",
        "벤치마크 MAXX",
    };

    private readonly int[] tiers = new int[5];
    private readonly List<int> fixedTypes = new List<int>();
    private readonly int[] offer = new int[3];
    private PlayerShooter shooter;

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

        if (headerText != null) headerText.text = "PHASE " + phaseCleared + " CLEAR";
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
        if (nameTexts[slot] != null) nameTexts[slot].text = Names[type];
        if (tierTexts[slot] != null) tierTexts[slot].text = max ? "MAX" : "Tier " + tier + " → " + (tier + 1);
        if (descTexts[slot] != null) descTexts[slot].text = Describe((AugmentType)type, tier, max);
    }

    string Pc(string sign, float cur, float next, bool arrow)
    {
        string a = sign + Mathf.RoundToInt(cur * 100f) + "%";
        if (!arrow) return a;
        return a + " → " + sign + Mathf.RoundToInt(next * 100f) + "%";
    }

    string Describe(AugmentType type, int tier, bool max)
    {
        float cur = BonusAt(tier);
        float next = BonusAt(tier + 1);
        bool arrow = !max;
        switch (type)
        {
            case AugmentType.Cooling:
                return "열 상승 " + Pc("-", cur, next, arrow) + "\n냉각 " + Pc("+", cur, next, arrow)
                    + "\n총기 발열 " + Pc("-", cur, next, arrow) + "\n총기 냉각 " + Pc("+", cur, next, arrow);
            case AugmentType.Vaccine:
                return "공격력 " + Pc("+", cur, next, arrow) + "\n광고 팝업 차단 " + Pc("", cur, next, arrow);
            case AugmentType.ReverseEngineering:
                return "아이템 드롭률 " + Pc("+", cur, next, arrow);
            case AugmentType.Optimization:
                return (tier == 0 ? "연사 기능 추가\n" : "") + "연사 속도 " + Pc("+", cur, next, arrow) + "\n탄속 " + Pc("+", cur, next, arrow);
            default:
                return "모든 열 상승 " + Pc("+", cur, next, arrow) + "\n콤보 점수 배율 " + Pc("+", cur, next, arrow);
        }
    }

    void UpdateOwned()
    {
        if (ownedText == null) return;

        string list = "보유 증강";
        if (fixedTypes.Count == 0) list += "\n없음";
        foreach (int t in fixedTypes) list += "\n" + Names[t] + "  Tier " + tiers[t];
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
