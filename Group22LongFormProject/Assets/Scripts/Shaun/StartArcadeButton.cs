using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(CanvasGroup))]
public class StartArcadeButton : MonoBehaviour
{
    private Button startButton;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        startButton = GetComponent<Button>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        startButton.onClick.AddListener(OnStartButtonClicked);
        SetVisible(false);
    }

    private void OnEnable()
    {
        GameManager.OnPhaseChanged += HandlePhaseChanged;
        SentenceEvents.OnSentenceInit += HandleSentenceInit;
        SentenceEvents.OnSentenceConstructionComplete += HandleSentenceComplete;

        RefreshVisibility();
    }

    private void OnDisable()
    {
        GameManager.OnPhaseChanged -= HandlePhaseChanged;
        SentenceEvents.OnSentenceInit -= HandleSentenceInit;
        SentenceEvents.OnSentenceConstructionComplete -= HandleSentenceComplete;
    }

    private void HandlePhaseChanged(GamePhase newPhase) => RefreshVisibility();
    private void HandleSentenceInit(string template) => RefreshVisibility();
    private void HandleSentenceComplete() => RefreshVisibility();

    private void RefreshVisibility()
    {
        bool isAssembly = GameManager.Instance != null && GameManager.Instance.CurrentPhase == GamePhase.Assembly;
        bool arcadeNotDone = GameManager.Instance != null && !GameManager.Instance.IsArcadeCompleted();
        bool sentenceDone = SentenceEvents.IsSentenceComplete;

        SetVisible(isAssembly && arcadeNotDone && sentenceDone);
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

    private void OnStartButtonClicked()
    {
        GameManager.Instance?.ChangePhase(GamePhase.Arcade);
    }
}