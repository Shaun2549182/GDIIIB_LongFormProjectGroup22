using UnityEngine;
using TMPro;

public class WordInventoryUI : MonoBehaviour
{
    [Header("List Anchors")]
    [SerializeField] private Transform characterListAnchor;
    [SerializeField] private Transform placeListAnchor;

    [Header("UI Prefab")]
    [SerializeField] private GameObject wordTextItemPrefab;

    private void OnEnable() => WordInventory.OnWordCollected += AddWordToList;
    private void OnDisable() => WordInventory.OnWordCollected -= AddWordToList;

    private void AddWordToList(string wordText, WordCategory category)
    {
        Transform targetAnchor = null;

        if (category == WordCategory.Character)
        {
            targetAnchor = characterListAnchor;
        }
        else if (category == WordCategory.Place)
        {
            targetAnchor = placeListAnchor;
        }

        if (targetAnchor == null)
        {
            Debug.LogWarning($"[WordInventoryUI] Target anchor for category '{category}' is missing or unassigned!");
            return;
        }

        if (wordTextItemPrefab == null)
        {
            Debug.LogWarning("[WordInventoryUI] wordTextItemPrefab is missing in the Inspector!");
            return;
        }

        GameObject newItem = Instantiate(wordTextItemPrefab, targetAnchor);

        // Flexible TMP lookup (handles both UI and standard TMP text components)
        if (newItem.TryGetComponent<TMP_Text>(out var tmpText))
        {
            tmpText.text = wordText;
        }
    }
}