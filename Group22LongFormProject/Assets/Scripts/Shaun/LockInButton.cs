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

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        lockInButton.onClick.AddListener(OnLockInClicked);
        SetVisible(false);
    }

    private void OnEnable()
    {
        GameManager.OnPhaseChanged += HandlePhaseChanged;
        SentenceEvents.OnSlotUpdated += HandleSlotUpdated;
        SentenceEvents.OnSentenceInit += HandleSentenceInit;

        RefreshVisibility();
    }

    private void OnDisable()
    {
        GameManager.OnPhaseChanged -= HandlePhaseChanged;
        SentenceEvents.OnSlotUpdated -= HandleSlotUpdated;
        SentenceEvents.OnSentenceInit -= HandleSentenceInit;
    }

    private void HandlePhaseChanged(GamePhase newPhase) => RefreshVisibility();
    private void HandleSlotUpdated(WordCategory category, string word) => RefreshVisibility();
    private void HandleSentenceInit(string template) => RefreshVisibility();

    private void RefreshVisibility()
    {
        bool isAssembly = GameManager.Instance != null && GameManager.Instance.CurrentPhase == GamePhase.Assembly;
        bool arcadeDone = GameManager.Instance != null && GameManager.Instance.IsArcadeCompleted();
        bool slotsFilled = AreBothSlotsFilled();

        SetVisible(isAssembly && arcadeDone && slotsFilled);
    }

    private bool AreBothSlotsFilled()
    {
        SentenceSlotUI[] slots = FindObjectsByType<SentenceSlotUI>(FindObjectsSortMode.None);
        if (slots == null || slots.Length < 2)
        {
            return false;
        }

        int filledCount = 0;
        foreach (var slot in slots)
        {
            if (slot != null && slot.CurrentSnappedBox != null && !string.IsNullOrEmpty(slot.GetSelectedWord()))
            {
                filledCount++;
            }
        }

        return filledCount >= 2;
    }

    private void SetVisible(bool visible)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
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
            string selectedChar = GameManager.Instance != null ? GameManager.Instance.GetCurrentActCharacterWord() : "";
            string selectedPlace = GameManager.Instance != null ? GameManager.Instance.GetCurrentActPlaceWord() : "";

            GameManager.Instance?.LockInChoices(selectedChar, selectedPlace);
        }
    }
}