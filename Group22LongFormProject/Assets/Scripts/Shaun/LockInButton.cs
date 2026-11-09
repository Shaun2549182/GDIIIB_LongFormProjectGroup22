using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(CanvasGroup))]
public class LockInButton : MonoBehaviour
{
    private Button lockInButton;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        lockInButton = GetComponent<Button>();

        // Fetch or add CanvasGroup for clean show/hide without deactivating the GameObject
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        lockInButton.onClick.AddListener(OnLockInClicked);
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
        bool isAssembly = (newPhase == GamePhase.Assembly);
        bool arcadeDone = GameManager.Instance != null && GameManager.Instance.IsArcadeCompleted();

        bool showButton = isAssembly && arcadeDone;

        // Set visibility and interactivity via CanvasGroup instead of SetActive()
        if (canvasGroup != null)
        {
            canvasGroup.alpha = showButton ? 1f : 0f;
            canvasGroup.interactable = showButton;
            canvasGroup.blocksRaycasts = showButton;
        }
    }

    private void OnLockInClicked()
    {
        bool isTutorial = SceneManager.GetActiveScene().name.IndexOf("Tutorial", System.StringComparison.OrdinalIgnoreCase) >= 0;

        if (isTutorial)
        {
            GameManager.Instance?.StartMainGame();
        }
        else
        {
            // Fetch current act selections instead of defaulting to Act 1
            string selectedChar = GameManager.Instance != null ? GameManager.Instance.GetCurrentActCharacterWord() : "";
            string selectedPlace = GameManager.Instance != null ? GameManager.Instance.GetCurrentActPlaceWord() : "";

            GameManager.Instance?.LockInChoices(selectedChar, selectedPlace);
        }
    }
}