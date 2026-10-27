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
        // Track when player transitions through Arcade
        if (newPhase == GamePhase.Arcade)
        {
            hasCompletedArcade = true;
            HideImmediate();
        }
        // Reveal only when returning to Assembly after Arcade is finished
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
        string currentScene = SceneManager.GetActiveScene().name;

        // 1. If clicked inside the Tutorial scene, transition to GameScene
        if (currentScene.IndexOf("Tutorial", System.StringComparison.OrdinalIgnoreCase) >= 0)
        {
            Debug.Log("[LockInButton] Tutorial complete! Transitioning to GameScene.");
            SceneManager.LoadScene("GameScene");
            return;
        }

        // 2. Extract selected choices using SentenceSlotUI methods
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

        Debug.Log($"[LockInButton] Choices locked in — Character: '{chosenCharacter}', Place: '{chosenPlace}'");

        // 3. Pass selections to GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LockInChoices(chosenCharacter, chosenPlace);
        }
        else
        {
            Debug.LogError("[LockInButton] GameManager instance missing! Loading default display scene fallback.");
            SceneManager.LoadScene("DisplayScene");
        }
    }
}