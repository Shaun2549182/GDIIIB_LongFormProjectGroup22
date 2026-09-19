using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(CanvasGroup))]
public class LockInButton : MonoBehaviour
{
    private Button button;
    private CanvasGroup canvasGroup;
    private bool hasCompletedArcade = false;

    private void Awake()
    {
        button = GetComponent<Button>();
        canvasGroup = GetComponent<CanvasGroup>();

        HideImmediate();
        button.onClick.AddListener(OnLockInClicked);
    }

    private void OnEnable() => GameManager.OnPhaseChanged += HandlePhaseChanged;
    private void OnDisable() => GameManager.OnPhaseChanged -= HandlePhaseChanged;

    private void HandlePhaseChanged(GamePhase newPhase)
    {
        if (newPhase == GamePhase.Arcade)
        {
            hasCompletedArcade = true;
            HideImmediate();
        }
        else if (newPhase == GamePhase.Assembly && hasCompletedArcade)
        {
            ShowButton();
        }
        else
        {
            HideImmediate();
        }
    }

    private void HideImmediate()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void ShowButton()
    {
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        LeanTween.alphaCanvas(canvasGroup, 1f, 0.4f).setEase(LeanTweenType.easeOutQuad);
    }

    private void OnLockInClicked()
    {
        string characterWord = string.Empty;
        string placeWord = string.Empty;

        // Retrieve selections from active slots
        SentenceSlotUI[] slots = FindObjectsByType<SentenceSlotUI>(FindObjectsSortMode.None);
        foreach (var slot in slots)
        {
            if (slot.GetSlotCategory() == WordCategory.Character)
            {
                characterWord = slot.GetSelectedWord();
            }
            else if (slot.GetSlotCategory() == WordCategory.Place)
            {
                placeWord = slot.GetSelectedWord();
            }
        }

        Debug.Log($"[LockInButton] Final choices locked in: Character='{characterWord}', Place='{placeWord}'");
        GameManager.Instance?.LockInChoices(characterWord, placeWord);
    }
}