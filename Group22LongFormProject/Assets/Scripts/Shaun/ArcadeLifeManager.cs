using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ArcadeLifeManager : MonoBehaviour
{
    public static ArcadeLifeManager Instance { get; private set; }

    [Header("Local Arcade Settings")]
    [SerializeField] private int startingLives = 3;
    [SerializeField] private List<GameObject> healthPointObjects;
    [SerializeField] private BallController ballController;

    private int currentLives;
    private bool isProcessingDeath = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void OnEnable() => GameManager.OnPhaseChanged += HandlePhaseChanged;
    private void OnDisable() => GameManager.OnPhaseChanged -= HandlePhaseChanged;

    private void HandlePhaseChanged(GamePhase newPhase)
    {
        if (newPhase == GamePhase.Arcade)
        {
            ResetLives();
        }
    }

    /// <summary>
    /// Restores local attempt lives and re-enables UI heart icons.
    /// </summary>
    public void ResetLives()
    {
        bool isTutorial = SceneManager.GetActiveScene().name.IndexOf("Tutorial", System.StringComparison.OrdinalIgnoreCase) >= 0;
        currentLives = isTutorial ? 2 : startingLives;
        isProcessingDeath = false;

        if (healthPointObjects != null)
        {
            for (int i = 0; i < healthPointObjects.Count; i++)
            {
                if (healthPointObjects[i] != null)
                {
                    healthPointObjects[i].SetActive(i < currentLives);
                }
            }
        }

        if (ballController == null)
        {
            ballController = FindFirstObjectByType<BallController>();
        }
    }

    public void OnBallFell()
    {
        // Guard against multiple trigger collisions in the same frame
        if (isProcessingDeath) return;

        currentLives--;

        // Hide UI heart icon for lost hit
        if (currentLives >= 0 && healthPointObjects != null && currentLives < healthPointObjects.Count)
        {
            if (healthPointObjects[currentLives] != null)
            {
                healthPointObjects[currentLives].SetActive(false);
            }
        }

        // Reset ball position back to paddle
        ballController?.ResetBall();

        if (currentLives <= 0)
        {
            isProcessingDeath = true;
            GameManager.Instance?.CompleteArcadePhase();
        }
    }
}