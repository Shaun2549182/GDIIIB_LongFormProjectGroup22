using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(CanvasGroup))]
public class StartArcadeButton : MonoBehaviour
{
    private Button button;
    private CanvasGroup canvasGroup;
    private bool isSentenceComplete = false;

    private void Awake()
    {
        button = GetComponent<Button>();
        canvasGroup = GetComponent<CanvasGroup>();

        HideImmediate();
        button.onClick.AddListener(OnButtonClicked);
    }

    private void OnEnable()
    {
        SentenceEvents.OnSentenceInit += HandleSentenceInit;
        SentenceEvents.OnSentenceConstructionComplete += HandleSentenceComplete;
        GameManager.OnPhaseChanged += HandlePhaseChanged;
    }

    private void OnDisable()
    {
        SentenceEvents.OnSentenceInit -= HandleSentenceInit;
        SentenceEvents.OnSentenceConstructionComplete -= HandleSentenceComplete;
        GameManager.OnPhaseChanged -= HandlePhaseChanged;
    }

    private void HandleSentenceInit(string template)
    {
        isSentenceComplete = false;
        HideImmediate();
    }

    private void HandleSentenceComplete()
    {
        isSentenceComplete = true;

        if (GameManager.Instance != null && GameManager.Instance.CurrentPhase == GamePhase.Assembly && !GameManager.Instance.IsArcadeCompleted)
        {
            ShowButton();
        }
    }

    private void HandlePhaseChanged(GamePhase newPhase)
    {
        if (newPhase == GamePhase.Assembly)
        {
            // Reveal only if sentence typing is already complete and arcade phase is not finished
            if (isSentenceComplete && (GameManager.Instance == null || !GameManager.Instance.IsArcadeCompleted))
            {
                ShowButton();
            }
            else
            {
                HideImmediate();
            }
        }
        else
        {
            HideImmediate();
        }
    }

    private void HideImmediate()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void ShowButton()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        LeanTween.alphaCanvas(canvasGroup, 1f, 0.4f).setEase(LeanTweenType.easeOutQuad);
    }

    private void OnButtonClicked()
    {
        HideImmediate();
        GameManager.Instance?.ChangePhase(GamePhase.Arcade);
    }
}