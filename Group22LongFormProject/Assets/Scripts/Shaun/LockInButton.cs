using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(CanvasGroup))]
public class LockInButton : MonoBehaviour
{
    private Button button;
    private CanvasGroup canvasGroup;

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
        // Reveal ONLY when in Assembly phase AND the arcade phase was successfully passed
        if (newPhase == GamePhase.Assembly && GameManager.Instance != null && GameManager.Instance.IsArcadeCompleted)
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
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene.IndexOf("Tutorial", System.StringComparison.OrdinalIgnoreCase) >= 0)
        {
            Debug.Log("[LockInButton] Tutorial complete! Transitioning to GameScene.");
            SceneManager.LoadScene("GameScene");
            return;
        }

        string chosenCharacter = "";
        string chosenPlace = "";

        SentenceSlotUI[] slots = FindObjectsByType<SentenceSlotUI>(FindObjectsSortMode.None);
        foreach (var slot in slots)
        {
            WordCategory category = slot.GetSlotCategory();
            string selectedWord = slot.GetSelectedWord();

            if (category == WordCategory.Character)
            {
                chosenCharacter = selectedWord;
            }
            else if (category == WordCategory.Place)
            {
                chosenPlace = selectedWord;
            }

            if (slot.TryGetComponent<TMP_Dropdown>(out var dropdown))
            {
                dropdown.interactable = false;
            }
        }

        canvasGroup.interactable = false;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.LockInChoices(chosenCharacter, chosenPlace);
        }
        else
        {
            SceneManager.LoadScene("DisplayScene");
        }
    }
}