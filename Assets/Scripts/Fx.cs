using System.Collections.Generic;
using UnityEngine;

public static class Fx
{
    private static Material flashMaterial;
    private static Material particleMaterial;
    private static AudioClip fanClipLow;
    private static AudioClip fanClipHigh;
    private static AudioClip tickHitClip;
    private static AudioClip tickKillClip;
    private static readonly Dictionary<int, AudioClip> clips = new Dictionary<int, AudioClip>();

    public static Material FlashMaterial
    {
        get
        {
            if (flashMaterial == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
                if (shader == null) shader = Shader.Find("Unlit/Color");
                if (shader == null) return null;

                flashMaterial = new Material(shader);
                flashMaterial.color = Color.white;
                if (flashMaterial.HasProperty("_BaseColor")) flashMaterial.SetColor("_BaseColor", Color.white);
            }
            return flashMaterial;
        }
    }

    private static Material ParticleMaterial
    {
        get
        {
            if (particleMaterial == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
                if (shader == null) shader = Shader.Find("Sprites/Default");
                if (shader == null) return null;

                particleMaterial = new Material(shader);
            }
            return particleMaterial;
        }
    }

    public static Material MakeMaterial(Color color)
    {
        Material baseMat = FlashMaterial;
        Material mat = baseMat != null ? new Material(baseMat) : new Material(Shader.Find("Sprites/Default"));
        mat.color = color;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        return mat;
    }

    public static void Burst(Vector3 position, Color color, int count = 14)
    {
        GameObject go = new GameObject("FxBurst");
        go.transform.position = position;

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystem.MainModule main = ps.main;
        main.duration = 0.5f;
        main.loop = false;
        main.playOnAwake = false;
        main.startLifetime = 0.5f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 7f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.25f);
        main.startColor = color;
        main.gravityModifier = 0.5f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.stopAction = ParticleSystemStopAction.Destroy;

        ParticleSystem.EmissionModule emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });

        ParticleSystem.ShapeModule shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.2f;

        ParticleSystem.SizeOverLifetimeModule size = ps.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

        ParticleSystemRenderer psr = go.GetComponent<ParticleSystemRenderer>();
        Material mat = ParticleMaterial;
        if (mat != null) psr.sharedMaterial = mat;

        ps.Play();
    }

    public static void Blip(Vector3 position, float frequency, float duration, float volume = 0.5f)
    {
        int key = Mathf.RoundToInt(frequency) * 1000 + Mathf.RoundToInt(duration * 1000f);
        if (!clips.TryGetValue(key, out AudioClip clip) || clip == null)
        {
            clip = CreateClip(frequency, duration);
            clips[key] = clip;
        }
        AudioSource.PlayClipAtPoint(clip, position, volume);
    }

    public static void HitTick(bool kill, float volume = 0.5f)
    {
        AudioClip clip;
        if (kill)
        {
            if (tickKillClip == null) tickKillClip = CreateTickClip("tick_kill", 2600f, 0.14f);
            clip = tickKillClip;
        }
        else
        {
            if (tickHitClip == null) tickHitClip = CreateTickClip("tick_hit", 1700f, 0.06f);
            clip = tickHitClip;
        }

        GameObject go = new GameObject("FxTick");
        AudioSource source = go.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;
        source.spatialBlend = 0f;
        source.Play();
        Object.Destroy(go, clip.length + 0.1f);
    }

    private static AudioClip CreateTickClip(string clipName, float frequency, float duration)
    {
        const int sampleRate = 44100;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] data = new float[sampleCount];

        float phase = 0f;
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            phase += 2f * Mathf.PI * frequency / sampleRate;
            float envelope = Mathf.Exp(-t * 7f) * Mathf.Min(1f, i / 24f);
            data[i] = (Mathf.Sin(phase) * 0.7f + Mathf.Sin(phase * 2f) * 0.3f) * envelope;
        }

        AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    public static AudioClip FanClipLow()
    {
        if (fanClipLow == null) fanClipLow = CreateFanClip("fan_low", 1800f, 213f, 0.14f, 0f, 0f, 1);
        return fanClipLow;
    }

    public static AudioClip FanClipHigh()
    {
        if (fanClipHigh == null) fanClipHigh = CreateFanClip("fan_high", 5500f, 247f, 0.2f, 0.12f, 3100f, 2);
        return fanClipHigh;
    }

    private static AudioClip CreateFanClip(string clipName, float lowPassHz, float bladeHz, float noiseRms, float air, float whineHz, int seed)
    {
        const int sampleRate = 44100;
        const float duration = 3f;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        int fadeCount = sampleRate / 2;
        float[] raw = new float[sampleCount + fadeCount];

        System.Random random = new System.Random(seed);
        float highK = 1f / (1f + 2f * Mathf.PI * 200f / sampleRate);
        float lowK = 1f - Mathf.Exp(-2f * Mathf.PI * lowPassHz / sampleRate);
        float prevWhite = 0f;
        float prevIn1 = 0f;
        float high1 = 0f;
        float prevIn2 = 0f;
        float high2 = 0f;
        float low = 0f;
        float sumSquares = 0f;
        for (int i = 0; i < raw.Length; i++)
        {
            float white = (float)(random.NextDouble() * 2.0 - 1.0);
            float colored = white + (white - prevWhite) * air;
            prevWhite = white;

            high1 = highK * (high1 + colored - prevIn1);
            prevIn1 = colored;
            high2 = highK * (high2 + high1 - prevIn2);
            prevIn2 = high1;

            low += lowK * (high2 - low);
            raw[i] = low;
            sumSquares += low * low;
        }

        float gain = noiseRms / Mathf.Sqrt(sumSquares / raw.Length);
        float driftA = (float)random.NextDouble() * Mathf.PI * 2f;
        float driftB = (float)random.NextDouble() * Mathf.PI * 2f;

        float[] data = new float[sampleCount];
        float peak = 0f;
        for (int i = 0; i < sampleCount; i++)
        {
            float noise = raw[i];
            if (i < fadeCount)
            {
                float w = (float)i / fadeCount * Mathf.PI * 0.5f;
                noise = raw[i] * Mathf.Sin(w) + raw[sampleCount + i] * Mathf.Cos(w);
            }

            float cycle = 2f * Mathf.PI * i / sampleCount;
            float drift = 1f + 0.02f * (Mathf.Sin(cycle + driftA) + 0.6f * Mathf.Sin(cycle * 2f + driftB));

            double t = (double)i / sampleRate;
            float blade = (float)System.Math.Sin(2.0 * System.Math.PI * bladeHz * t) * 0.03f
                + (float)System.Math.Sin(2.0 * System.Math.PI * bladeHz * 2.0 * t) * 0.012f;
            float whine = whineHz > 0f ? (float)System.Math.Sin(2.0 * System.Math.PI * whineHz * t) * 0.004f : 0f;

            data[i] = noise * gain * drift + blade + whine;
            peak = Mathf.Max(peak, Mathf.Abs(data[i]));
        }

        if (peak > 0.75f)
        {
            float scale = 0.75f / peak;
            for (int i = 0; i < sampleCount; i++) data[i] *= scale;
        }

        AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private static AudioClip CreateClip(float frequency, float duration)
    {
        const int sampleRate = 44100;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] data = new float[sampleCount];

        float phase = 0f;
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            float freq = Mathf.Lerp(frequency, frequency * 0.5f, t);
            phase += 2f * Mathf.PI * freq / sampleRate;
            data[i] = Mathf.Sin(phase) * (1f - t);
        }

        AudioClip clip = AudioClip.Create("blip_" + frequency, sampleCount, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
