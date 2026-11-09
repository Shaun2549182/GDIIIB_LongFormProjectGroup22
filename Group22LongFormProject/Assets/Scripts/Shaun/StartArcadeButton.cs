using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class StartArcadeButton : MonoBehaviour
{
    private Button startButton;

    private void Awake()
    {
        startButton = GetComponent<Button>();
        startButton.onClick.AddListener(OnStartButtonClicked);
    }

    private void OnEnable()
    {
        GameManager.OnPhaseChanged += HandlePhaseChanged;

        if (GameManager.Instance != null)
        {
            HandlePhaseChanged(GameManager.Instance.CurrentPhase);
        }
    }

    private void OnDisable()
    {
        GameManager.OnPhaseChanged -= HandlePhaseChanged;
    }

    private void HandlePhaseChanged(GamePhase newPhase)
    {
        // Added () to call !GameManager.Instance.IsArcadeCompleted()
        bool isAssembly = (newPhase == GamePhase.Assembly);
        bool arcadeNotDone = GameManager.Instance != null && !GameManager.Instance.IsArcadeCompleted();

        gameObject.SetActive(isAssembly && arcadeNotDone);
    }

    private void OnStartButtonClicked()
    {
        GameManager.Instance?.ChangePhase(GamePhase.Arcade);
    }
}