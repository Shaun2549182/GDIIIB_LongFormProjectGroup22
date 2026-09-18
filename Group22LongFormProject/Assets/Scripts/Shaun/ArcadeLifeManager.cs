using System.Collections.Generic;
using UnityEngine;

public class ArcadeLifeManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BallController ballController;
    [SerializeField] private List<GameObject> healthPointObjects; // Drag your 2 health GameObjects here

    private int currentLives;

    private void OnEnable() => GameManager.OnPhaseChanged += HandlePhaseChanged;
    private void OnDisable() => GameManager.OnPhaseChanged -= HandlePhaseChanged;

    private void HandlePhaseChanged(GamePhase newPhase)
    {
        if (newPhase == GamePhase.Arcade)
        {
            ResetLives();
        }
    }

    public void ResetLives()
    {
        currentLives = healthPointObjects.Count;
        foreach (var hpObj in healthPointObjects)
        {
            if (hpObj != null) hpObj.SetActive(true);
        }
    }

    public void OnBallFell()
    {
        currentLives--;

        if (currentLives >= 0 && currentLives < healthPointObjects.Count)
        {
            if (healthPointObjects[currentLives] != null)
            {
                healthPointObjects[currentLives].SetActive(false);
            }
        }

        if (currentLives <= 0)
        {
            // Out of lives: Validate collected words before changing phase
            GameManager.Instance.CompleteArcadePhase();
        }
        else
        {
            ballController.ResetBall();
        }
    }
}