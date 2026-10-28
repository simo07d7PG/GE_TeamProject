using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject titlePanel;
    public GameObject resultPanel;
    public Text scoreText;
    public Text resultScoreText;
    public HeatGauge heatGauge;
    public EnemySpawner[] spawners;

    private GameObject player;
    private Vector3 playerStartPosition;
    private Quaternion playerStartRotation;
    private int score;
    private bool isPlaying;

    void Awake()
    {
        titlePanel.SetActive(true);
        resultPanel.SetActive(false);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        UpdateScoreText();
    }

    void Start()
    {
        player = GameObject.FindWithTag("Player");
        playerStartPosition = player.transform.position;
        playerStartRotation = player.transform.rotation;
    }

    public void StartGame()
    {
        titlePanel.SetActive(false);
        resultPanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        isPlaying = true;
    }

    public void AddScore(int amount)
    {
        if (!isPlaying) return;
        score += amount;
        UpdateScoreText();
    }

    public void GameOver()
    {
        if (!isPlaying) return;
        isPlaying = false;
        resultScoreText.text = "SCORE " + score;
        resultPanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
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
        PlayerShooter shooter = player.GetComponentInChildren<PlayerShooter>();
        if (shooter != null) shooter.ResetWeaponHeat();

        heatGauge.ResetHeat();
        foreach (EnemySpawner s in spawners) s.ResetSpawner();

        score = 0;
        UpdateScoreText();
        StartGame();
    }

    void UpdateScoreText()
    {
        scoreText.text = "SCORE " + score;
    }
}
