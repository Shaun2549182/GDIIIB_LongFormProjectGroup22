using System.Collections.Generic;
using UnityEngine;

public class ArcadeStageController : MonoBehaviour
{
    public static ArcadeStageController Instance { get; private set; }

    [Header("Stage References")]
    [SerializeField] private Transform brickContainer; // Drag the parent object holding all bricks here (Optional)
    [SerializeField] private List<GameObject> stageBricks = new List<GameObject>();
    [SerializeField] private BallController ballController;
    [SerializeField] private ArcadeTimer arcadeTimer;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        CacheBricks();
    }

    private void OnEnable() => GameManager.OnPhaseChanged += HandlePhaseChanged;
    private void OnDisable() => GameManager.OnPhaseChanged -= HandlePhaseChanged;

    private void HandlePhaseChanged(GamePhase newPhase)
    {
        if (newPhase == GamePhase.Arcade)
        {
            ResetStage();
        }
    }

    public void CacheBricks()
    {
        stageBricks.Clear();

        if (brickContainer != null)
        {
            foreach (Transform child in brickContainer)
            {
                stageBricks.Add(child.gameObject);
            }
        }
        else
        {
            GameObject[] foundBricks = GameObject.FindGameObjectsWithTag("Brick");
            stageBricks.AddRange(foundBricks);
        }
    }

    public void ResetStage()
    {
        // 1. Reactivate all cached bricks
        foreach (GameObject brick in stageBricks)
        {
            if (brick != null)
            {
                brick.SetActive(true);
            }
        }

        ArcadeTimer.Instance?.ResetTimer();

        // 2. Destroy any active falling word items in the scene
        FallingWordItem[] fallingWords = FindObjectsByType<FallingWordItem>(FindObjectsSortMode.None);
        foreach (var fallingWord in fallingWords)
        {
            if (fallingWord != null)
            {
                Destroy(fallingWord.gameObject);
            }
        }

        // 3. Clear uncommitted word inventory & UI
        WordInventory.Instance?.ClearInventory();

        // 4. Reset local arcade hearts
        ArcadeLifeManager.Instance?.ResetLives();

        // 5. Reset Ball & Paddle positions
        if (ballController == null) ballController = FindFirstObjectByType<BallController>();
        ballController?.ResetBall();

        Debug.Log("[ArcadeStageController] Arcade stage, bricks, and falling words successfully reset.");
    }
}