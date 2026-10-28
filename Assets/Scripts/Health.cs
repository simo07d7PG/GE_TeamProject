using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    public int maxHp = 3;
    public int currentHp;
    public int score = 10;
    public Slider hpSlider;

    public static event System.Action<Health> OnEnemyKilled;
    public static event System.Action<Health, Vector3> OnPlayerDamaged;

    public float invincibilityDuration = 1.5f; 
    public float blinkInterval = 0.1f;        
    private bool isInvincible = false;       
    private Renderer[] renderers;

    public float knockbackForce = 4f;
    public float knockbackDecay = 12f;
    public float flashDuration = 0.06f;
    private Vector3 knockback;
    private bool isFlashing;
    private Vector3 flashBaseScale;
    private Material[][] flashOriginals;
    private float blinkElapsed;
    private float extraInvincibleUntil;

    void Start()
    {
        currentHp = maxHp;
        UpdateBar();

        renderers = GetComponentsInChildren<Renderer>();
    }

    void Update()
    {
        if (knockback.sqrMagnitude < 0.01f) return;

        transform.position += knockback * Time.deltaTime;
        knockback = Vector3.MoveTowards(knockback, Vector3.zero, knockbackDecay * Time.deltaTime);
    }

    public void TakeDamage(int damage, Vector3 hitDirection = default)
    {
        if (currentHp <= 0 || isInvincible || Time.time < extraInvincibleUntil) return;

        currentHp -= damage;
        UpdateBar();

        bool isPlayer = CompareTag("Player");
        if (isPlayer) OnPlayerDamaged?.Invoke(this, hitDirection);
        else PlayHitFeedback(hitDirection);

        if (currentHp <= 0)
        {
            Die();
        }
        else if (isPlayer)
        {
            StartInvincibility();
        }
    }

    void PlayHitFeedback(Vector3 hitDirection)
    {
        hitDirection.y = 0f;
        if (hitDirection.sqrMagnitude > 0.001f) knockback = hitDirection.normalized * knockbackForce;

        Fx.Blip(transform.position, 520f, 0.06f, 0.4f);
        if (!isFlashing && currentHp > 0) StartHitFlash();
    }

    void StartHitFlash()
    {
        Material flash = Fx.FlashMaterial;
        if (flash == null || renderers == null) return;

        isFlashing = true;
        flashBaseScale = transform.localScale;
        transform.localScale = flashBaseScale * 1.15f;

        flashOriginals = new Material[renderers.Length][];
        for (int i = 0; i < renderers.Length; i++)
        {
            flashOriginals[i] = renderers[i].sharedMaterials;
            Material[] flashes = new Material[flashOriginals[i].Length];
            for (int m = 0; m < flashes.Length; m++) flashes[m] = flash;
            renderers[i].sharedMaterials = flashes;
        }

        Invoke(nameof(EndHitFlash), flashDuration);
    }

    void EndHitFlash()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null) renderers[i].sharedMaterials = flashOriginals[i];
        }
        transform.localScale = flashBaseScale;
        isFlashing = false;
    }

    public void GrantInvincibility(float duration)
    {
        extraInvincibleUntil = Mathf.Max(extraInvincibleUntil, Time.time + duration);
    }

    public void Heal(int amount)
    {
        if (currentHp <= 0) return;

        currentHp = Mathf.Min(maxHp, currentHp + amount);
        UpdateBar();
    }

    public void ResetHp()
    {
        currentHp = maxHp;
        isInvincible = false;
        CancelInvoke(nameof(Blink));

        if (renderers != null)
        {
            foreach (Renderer r in renderers) r.enabled = true;
        }

        UpdateBar();
    }

    void UpdateBar()
    {
        if (hpSlider != null) hpSlider.value = (float)currentHp / maxHp;
    }

    void Die()
    {
        GameManager gm = GameManager.Instance;

        if (CompareTag("Player"))
        {
            if (gm != null) gm.GameOver();
        }
        else
        {
            if (gm != null) gm.AddScore(score);
            Fx.Burst(transform.position + Vector3.up * 0.5f, new Color(0.4f, 1f, 0.9f));
            Fx.Blip(transform.position, 260f, 0.18f, 0.6f);
            OnEnemyKilled?.Invoke(this);
            Destroy(gameObject);
        }
    }

    void StartInvincibility()
    {
        isInvincible = true;
        blinkElapsed = 0f;
        Blink();
    }

    void Blink()
    {
        if (blinkElapsed >= invincibilityDuration)
        {
            foreach (Renderer r in renderers)
            {
                r.enabled = true;
            }
            isInvincible = false;
            return;
        }

        foreach (Renderer r in renderers)
        {
            r.enabled = !r.enabled;
        }
        blinkElapsed += blinkInterval;
        Invoke(nameof(Blink), blinkInterval);
    }
}