using UnityEngine;
using UnityEngine.UI;

public class HeatGauge : MonoBehaviour
{
    public Slider heatSlider;
    public GameManager gameManager;
    public float heatPerEnemy = 0.3f;
    public float coolPerSecond = 2f;

    private float heat;

    void Update()
    {
        int alive = GameObject.FindGameObjectsWithTag("Enemy").Length;
        heat += (alive * heatPerEnemy - coolPerSecond) * Time.deltaTime;
        heat = Mathf.Clamp(heat, 0f, 100f);

        if (heatSlider != null) heatSlider.value = heat / 100f;
        if (heat >= 100f) gameManager.GameOver();
    }

    public void ResetHeat()
    {
        heat = 0f;
    }
}
