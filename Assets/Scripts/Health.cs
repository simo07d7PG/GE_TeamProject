using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    public int maxHp = 3;
    public int currentHp;
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

    void UpdateBar()
    {
        if (hpSlider != null) hpSlider.value = (float)currentHp / maxHp;
    }

    void Die()
    {
        if (CompareTag("Player")) Time.timeScale = 0f;
        else Destroy(gameObject);
    }
}
