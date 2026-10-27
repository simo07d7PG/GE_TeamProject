using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    public int maxHp = 3;
    public int currentHp;
    public int score = 10;
    public Slider hpSlider;

    public float invincibilityDuration = 1.5f; 
    public float blinkInterval = 0.1f;        
    private bool isInvincible = false;       
    private Renderer[] renderers;            

    void Start()
    {
        currentHp = maxHp;
        UpdateBar();

        renderers = GetComponentsInChildren<Renderer>();
    }

    public void TakeDamage(int damage)
    {
        if (currentHp <= 0 || isInvincible) return;

        currentHp -= damage;
        UpdateBar();

        if (currentHp <= 0)
        {
            Die();
        }
        else if (CompareTag("Player"))
        {
            StartCoroutine(InvincibilityCoroutine());
        }
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