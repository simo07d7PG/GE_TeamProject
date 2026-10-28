using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    public int maxHp = 3;
    public int currentHp;
    public int score = 10;
    public Slider hpSlider;

    public static event System.Action<Health> OnEnemyKilled;

    public float invincibilityDuration = 1.5f; 
    public float blinkInterval = 0.1f;        
    private bool isInvincible = false;       
    private Renderer[] renderers;

    public float knockbackForce = 4f;
    public float knockbackDecay = 12f;
    public float flashDuration = 0.06f;
    private Vector3 knockback;
    private bool isFlashing;

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
        if (currentHp <= 0 || isInvincible) return;

        currentHp -= damage;
        UpdateBar();

        bool isPlayer = CompareTag("Player");
        if (!isPlayer) PlayHitFeedback(hitDirection);

        if (currentHp <= 0)
        {
            Die();
        }
        else if (isPlayer)
        {
            StartCoroutine(InvincibilityCoroutine());
        }
    }

    void PlayHitFeedback(Vector3 hitDirection)
    {
        hitDirection.y = 0f;
        if (hitDirection.sqrMagnitude > 0.001f) knockback = hitDirection.normalized * knockbackForce;

        Fx.Blip(transform.position, 520f, 0.06f, 0.4f);
        if (!isFlashing && currentHp > 0) StartCoroutine(HitFlashCoroutine());
    }

    IEnumerator HitFlashCoroutine()
    {
        Material flash = Fx.FlashMaterial;
        if (flash == null || renderers == null) yield break;

        isFlashing = true;
        Vector3 baseScale = transform.localScale;
        transform.localScale = baseScale * 1.15f;

        Material[][] originals = new Material[renderers.Length][];
        for (int i = 0; i < renderers.Length; i++)
        {
            originals[i] = renderers[i].sharedMaterials;
            Material[] flashes = new Material[originals[i].Length];
            for (int m = 0; m < flashes.Length; m++) flashes[m] = flash;
            renderers[i].sharedMaterials = flashes;
        }

        yield return new WaitForSeconds(flashDuration);

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null) renderers[i].sharedMaterials = originals[i];
        }
        transform.localScale = baseScale;
        isFlashing = false;
    }

    public void ResetHp()
    {
        currentHp = maxHp;
        isInvincible = false; 

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
        GameManager gm = null;
        GameObject g = GameObject.FindWithTag("GameController");
        if (g != null) gm = g.GetComponent<GameManager>();

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
    IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        float elapsedTime = 0f;

        while (elapsedTime < invincibilityDuration)
        {
            foreach (Renderer r in renderers)
            {
                r.enabled = !r.enabled;
            }

            yield return new WaitForSeconds(blinkInterval);
            elapsedTime += blinkInterval;
        }

        foreach (Renderer r in renderers)
        {
            r.enabled = true;
        }
        isInvincible = false;
    }
}