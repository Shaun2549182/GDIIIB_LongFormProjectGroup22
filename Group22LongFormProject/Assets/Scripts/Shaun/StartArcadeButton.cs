using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(CanvasGroup))]
public class StartArcadeButton : MonoBehaviour 
{
    private Button button;
    private CanvasGroup canvasGroup;

    private void Awake() 
    {
        button = GetComponent<Button>();
        canvasGroup = GetComponent<CanvasGroup>();
        
        // Make invisible and non-interactive immediately on start
        HideButtonImmediate();
        
        button.onClick.AddListener(OnButtonClicked);
    }

    private void OnEnable() => SentenceEvents.OnSentenceConstructionComplete += ShowButton;
    private void OnDisable() => SentenceEvents.OnSentenceConstructionComplete -= ShowButton;

    private void HideButtonImmediate() 
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void ShowButton() 
    {
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        // Smooth LeanTween fade-in
        LeanTween.alphaCanvas(canvasGroup, 1f, 0.4f).setEase(LeanTweenType.easeOutQuad);
    }

    private void OnButtonClicked() 
    {
        HideButtonImmediate();
        GameManager.Instance.ChangePhase(GamePhase.Arcade);
    }
}