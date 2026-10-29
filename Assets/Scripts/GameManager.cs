using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject titlePanel;
    public GameObject resultPanel;
    public GameObject clearPanel;
    public Text scoreText;
    public Text resultScoreText;
    public Text clearScoreText;
    public HeatGauge heatGauge;
    public EnemySpawner[] spawners;
    public Font hudFont;
    public Font hudFontBold;

    private GameObject player;
    private PlayerShooter playerShooter;
    private Vector3 playerStartPosition;
    private Quaternion playerStartRotation;
    private int score;
    private bool isPlaying;

    public bool IsPlaying => isPlaying;

    void Awake()
    {
        Instance = this;
        ComboSystem.Create();
        WaveManager.Create();
        PlayerUpgrades.Create();
        AdPopups.Create();
        Crosshair.Create();
        DamageIndicator.Create();

        titlePanel.SetActive(false);
        resultPanel.SetActive(false);
        if (clearPanel != null) clearPanel.SetActive(false);
        UpdateScoreText();
    }

    void Start()
    {
        player = GameObject.FindWithTag("Player");
        playerShooter = player.GetComponentInChildren<PlayerShooter>();
        playerStartPosition = player.transform.position;
        playerStartRotation = player.transform.rotation;
        StartGame();
    }

    public void StartGame()
    {
        titlePanel.SetActive(false);
        resultPanel.SetActive(false);
        if (clearPanel != null) clearPanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        isPlaying = true;
        if (WaveManager.Instance != null) WaveManager.Instance.Begin();
    }

    public void AddScore(int amount)
    {
        if (!isPlaying) return;
        score += Mathf.RoundToInt(amount * ComboSystem.TotalMultiplier);
        UpdateScoreText();
    }

    public void OnEnemyKilled(Health enemy)
    {
        AddScore(enemy.score);
        if (ComboSystem.Instance != null) ComboSystem.Instance.AddKill();
        if (PlayerUpgrades.Instance != null) PlayerUpgrades.Instance.TryDrop(enemy);
        if (playerShooter != null) playerShooter.CoolOnKill();
    }

    public void GameOver()
    {
        if (!isPlaying) return;
        isPlaying = false;
        if (WaveManager.Instance != null) WaveManager.Instance.End();
        resultScoreText.text = "SCORE " + score.ToString("D6");
        resultPanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
    }

    public void GameClear()
    {
        if (!isPlaying) return;
        isPlaying = false;
        if (WaveManager.Instance != null) WaveManager.Instance.End();
        if (clearScoreText != null) clearScoreText.text = "SCORE " + score.ToString("D6");
        if (clearPanel != null) clearPanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
    }

    public void RestartScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToTitle()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Title");
    }

    public void Restart()
    {
        foreach (GameObject e in GameObject.FindGameObjectsWithTag("Enemy")) Destroy(e);
        foreach (Bullet b in FindObjectsByType<Bullet>(FindObjectsSortMode.None)) Destroy(b.gameObject);

        CharacterController controller = player.GetComponent<CharacterController>();
        controller.enabled = false;
        player.transform.SetPositionAndRotation(playerStartPosition, playerStartRotation);
        controller.enabled = true;
        player.GetComponent<Health>().ResetHp();
        if (playerShooter != null) playerShooter.ResetWeaponHeat();
        if (ComboSystem.Instance != null) ComboSystem.Instance.ResetCombo();
        if (PlayerUpgrades.Instance != null) PlayerUpgrades.Instance.ResetUpgrades();
        if (AugmentManager.Instance != null) AugmentManager.Instance.ResetAll();
        foreach (Pickup p in FindObjectsByType<Pickup>(FindObjectsSortMode.None)) Destroy(p.gameObject);

        heatGauge.ResetHeat();
        foreach (EnemySpawner s in spawners) s.ResetSpawner();

        score = 0;
        UpdateScoreText();
        StartGame();
    }

    void UpdateScoreText()
    {
        scoreText.text = score.ToString("D6");
    }
}
