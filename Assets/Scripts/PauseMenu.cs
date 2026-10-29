using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance { get; private set; }

    public GameObject panel;

    private bool isOpen;

    void Awake()
    {
        Instance = this;
        if (panel != null) panel.SetActive(false);
    }

    public void Toggle()
    {
        if (isOpen) Resume();
        else Open();
    }

    void Open()
    {
        if (panel == null || isOpen) return;
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;
        if (Time.timeScale == 0f) return;
        if (AugmentManager.Instance != null && AugmentManager.Instance.panel != null && AugmentManager.Instance.panel.activeSelf) return;

        isOpen = true;
        panel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Resume()
    {
        if (!isOpen) return;

        isOpen = false;
        if (panel != null) panel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void Restart()
    {
        isOpen = false;
        GameManager.Instance.RestartScene();
    }

    public void GoToTitle()
    {
        isOpen = false;
        GameManager.Instance.GoToTitle();
    }

    public void QuitToDesktop()
    {
        Application.Quit();
    }
}
