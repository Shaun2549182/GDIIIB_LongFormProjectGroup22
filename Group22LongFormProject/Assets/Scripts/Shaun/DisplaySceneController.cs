using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DisplaySceneController : MonoBehaviour
{
    [Header("UI & Display Elements")]
    [SerializeField] private TextMeshProUGUI sentenceDisplayText;
    [SerializeField] private Transform characterDisplayAnchor;
    [SerializeField] private Transform placeDisplayAnchor;
    [SerializeField] private Button playAgainButton;

    private void Start()
    {
        if (playAgainButton != null)
        {
            playAgainButton.onClick.AddListener(OnPlayAgainClicked);
        }

        DisplayFinalResult();
    }

    private void DisplayFinalResult()
    {
        if (GameManager.Instance == null) return;

        // 1. Render final formatted sentence
        if (sentenceDisplayText != null)
        {
            sentenceDisplayText.text = GameManager.Instance.GetFormattedSentence();
        }

        // 2. Spawn Character visual prefab with Display Phase scale
        string charWord = GameManager.Instance.SelectedCharacterWord;
        if (!string.IsNullOrEmpty(charWord) && characterDisplayAnchor != null)
        {
            WordVisualManager.Instance?.SpawnVisualAtAnchor(charWord, WordCategory.Character, characterDisplayAnchor, isDisplayPhase: true);
        }

        // 3. Spawn Place visual prefab with Display Phase scale
        string placeWord = GameManager.Instance.SelectedPlaceWord;
        if (!string.IsNullOrEmpty(placeWord) && placeDisplayAnchor != null)
        {
            WordVisualManager.Instance?.SpawnVisualAtAnchor(placeWord, WordCategory.Place, placeDisplayAnchor, isDisplayPhase: true);
        }
    }

    private void OnPlayAgainClicked()
    {
        GameManager.Instance?.RestartGame();
    }
}