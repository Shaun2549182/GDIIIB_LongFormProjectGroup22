using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class pauseMenu : MonoBehaviour
{
    [Header("Pause Menu UI")]
    public GameObject pauseMenuUI;

    [Header("Pause Settings")]
    public bool isPaused = false;

    void Start()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        isPaused = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;

        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}