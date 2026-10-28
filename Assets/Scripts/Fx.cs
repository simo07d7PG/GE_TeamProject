using System.Collections.Generic;
using UnityEngine;

public static class Fx
{
    private static Material flashMaterial;
    private static Material particleMaterial;
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
