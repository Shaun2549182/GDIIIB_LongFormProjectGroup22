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

        // 2. Spawn Character visual prefab
        string charWord = GameManager.Instance.SelectedCharacterWord;
        if (!string.IsNullOrEmpty(charWord) && characterDisplayAnchor != null)
        {
            GameObject charPrefab = WordVisualManager.Instance?.GetPrefabForWord(charWord);
            if (charPrefab != null)
            {
                InstantiatePrefabAtAnchor(charPrefab, characterDisplayAnchor);
            }
        }

        // 3. Spawn Place visual prefab
        string placeWord = GameManager.Instance.SelectedPlaceWord;
        if (!string.IsNullOrEmpty(placeWord) && placeDisplayAnchor != null)
        {
            GameObject placePrefab = WordVisualManager.Instance?.GetPrefabForWord(placeWord);
            if (placePrefab != null)
            {
                InstantiatePrefabAtAnchor(placePrefab, placeDisplayAnchor);
            }
        }
    }

    private void InstantiatePrefabAtAnchor(GameObject prefab, Transform anchor)
    {
        GameObject instance = Instantiate(prefab, anchor);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;
    }

    private void OnPlayAgainClicked()
    {
        GameManager.Instance?.RestartGame();
    }
}