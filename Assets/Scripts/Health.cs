using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    public int maxHp = 3;
    public int currentHp;
    public int score = 10;
    public Slider hpSlider;

    void Start()
    {
        currentHp = maxHp;
        UpdateBar();
    }

    public void TakeDamage(int damage)
    {
        if (currentHp <= 0) return;
        currentHp -= damage;
        UpdateBar();
        if (currentHp <= 0) Die();
    }

    public void ResetHp()
    {
        currentHp = maxHp;
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
}
