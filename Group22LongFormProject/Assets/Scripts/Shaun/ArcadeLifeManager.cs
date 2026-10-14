using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class ArcadeLifeManager : MonoBehaviour
{
    [SerializeField] private int startingLives = 3;
    [SerializeField] private List<GameObject> healthPointObjects;
    [SerializeField] private BallController ballController;

    private int currentLives;

    private void Start()
    {
        // Check if current scene is Tutorial
        bool isTutorial = SceneManager.GetActiveScene().name.IndexOf("Tutorial", System.StringComparison.OrdinalIgnoreCase) >= 0;

        if (isTutorial)
        {
            currentLives = 999;
        }
        else
        {
            currentLives = startingLives;
        }
    }

    public void OnBallFell()
    {
        currentLives--;

        // Hide UI heart/life icons if within range
        if (currentLives >= 0 && healthPointObjects != null && currentLives < healthPointObjects.Count)
        {
            if (healthPointObjects[currentLives] != null)
            {
                healthPointObjects[currentLives].SetActive(false);
            }
        }

        if (currentLives <= 0)
        {
            // Out of lives: End Arcade phase
            GameManager.Instance?.CompleteArcadePhase();
        }
        else
        {
            // Reset ball for next attempt
            ballController?.ResetBall();
        }
    }
}