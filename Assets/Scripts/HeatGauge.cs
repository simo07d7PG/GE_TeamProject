using UnityEngine;
using UnityEngine.UI;

public class HeatGauge : MonoBehaviour
{
    public Slider heatSlider;
    public Image heatFill;
    public Text heatText;
    public Color hotColor = new Color(0.9f, 0.1f, 0.1f);
    public Color coolingColor = new Color(0.78f, 0.78f, 0.78f);
    public GameManager gameManager;

    [Header("Heat")]
    public float maxHeat = 100f;
    public int enemyThreshold = 5;
    public AnimationCurve heatRiseCurve = new AnimationCurve(
        new Keyframe(0f, 0f, 0f, 0.3f),
        new Keyframe(1f, 0.3f, 0.3f, 0.225f),
        new Keyframe(5f, 1.2f, 0.225f, 0.36f),
        new Keyframe(10f, 3f, 0.36f, 0.2f),
        new Keyframe(15f, 4f, 0.2f, 0f));
    public float maxRisePerSecond = 4f;
    public float coolPerSecond = 1f;
    public float riseMultiplier = 1f;
    public float coolMultiplier = 1f;

    [Header("Fan Sound")]
    public float fanMinVolume = 0.05f;
    public float fanMaxVolume = 0.6f;
    public float fanMinPitch = 0.97f;
    public float fanMaxPitch = 1.08f;

    private float heat;
    private bool isCooling;
    private AudioSource fanLow;
    private AudioSource fanHigh;

    public float HeatRatio => Mathf.Clamp01(heat / maxHeat);
    public bool IsCooling => isCooling;

    void Start()
    {
        fanLow = CreateFan(Fx.FanClipLow());
        fanHigh = CreateFan(Fx.FanClipHigh());
        fanLow.volume = fanMinVolume;
        fanHigh.volume = 0f;
        fanLow.Play();
        fanHigh.Play();
    }

    AudioSource CreateFan(AudioClip clip)
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.loop = true;
        source.spatialBlend = 0f;
        source.playOnAwake = false;
        source.pitch = fanMinPitch;
        return source;
    }

    void Update()
    {
        if (Time.timeScale == 0f)
        {
            if (fanLow != null && fanLow.isPlaying) fanLow.Pause();
            if (fanHigh != null && fanHigh.isPlaying) fanHigh.Pause();
            return;
        }
        if (fanLow != null) fanLow.UnPause();
        if (fanHigh != null) fanHigh.UnPause();

        CheckGameOver();

        int alive = GameObject.FindGameObjectsWithTag("Enemy").Length;
        isCooling = false;
        if (alive > enemyThreshold)
        {
            float rate = Mathf.Min(heatRiseCurve.Evaluate(alive - enemyThreshold), maxRisePerSecond);
            AddHeat(rate * Time.deltaTime);
        }
        else if (alive < enemyThreshold && heat > 0f && heat < maxHeat)
        {
            isCooling = true;
            heat = Mathf.Max(0f, heat - coolPerSecond * coolMultiplier * Time.deltaTime);
        }

        float ratio = HeatRatio;
        if (heatSlider != null) heatSlider.value = ratio;
        if (heatFill != null) heatFill.color = isCooling ? coolingColor : hotColor;
        if (heatText != null) heatText.text = Mathf.RoundToInt(ratio * 100f) + "%";
        if (fanLow != null && fanHigh != null)
        {
            float volume = Mathf.Lerp(fanMinVolume, fanMaxVolume, ratio);
            float pitch = Mathf.Lerp(fanMinPitch, fanMaxPitch, ratio);
            fanLow.volume = volume * Mathf.Cos(ratio * Mathf.PI * 0.5f);
            fanHigh.volume = volume * Mathf.Sin(ratio * Mathf.PI * 0.5f);
            fanLow.pitch = pitch;
            fanHigh.pitch = pitch;
        }

        CheckGameOver();
    }

    void CheckGameOver()
    {
        if (heat < maxHeat) return;
        GameManager manager = gameManager != null ? gameManager : GameManager.Instance;
        if (manager != null) manager.GameOver();
    }

    public void AddHeat(float amount)
    {
        heat = Mathf.Clamp(heat + amount * riseMultiplier, 0f, maxHeat);
    }

    public void ResetHeat()
    {
        heat = 0f;
        isCooling = false;
    }
}
