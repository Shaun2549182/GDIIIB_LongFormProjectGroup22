using UnityEngine;
using TMPro;

public class WordInventoryUI : MonoBehaviour
{
    [Header("List Anchors (Vertical Layout Groups)")]
    [SerializeField] private Transform characterListAnchor;
    [SerializeField] private Transform placeListAnchor;

    [Header("UI Prefab")]
    [SerializeField] private GameObject wordTextItemPrefab;

    private void OnEnable()
    {
        WordInventory.OnWordCollected += AddWordToList;
        WordInventory.OnInventoryCleared += ClearUILists;
    }

    private void OnDisable()
    {
        WordInventory.OnWordCollected -= AddWordToList;
        WordInventory.OnInventoryCleared -= ClearUILists;
    }

    private void AddWordToList(string wordText, WordCategory category)
    {
        Transform targetAnchor = (category == WordCategory.Character) ? characterListAnchor : placeListAnchor;

        if (targetAnchor != null && wordTextItemPrefab != null)
        {
            GameObject newItem = Instantiate(wordTextItemPrefab, targetAnchor);
            if (newItem.TryGetComponent<TextMeshProUGUI>(out var tmpText))
            {
                tmpText.text = wordText;
            }
        }
    }

    /// <summary>
    /// Destroys all UI text items inside the vertical group anchors when retrying or resetting.
    /// </summary>
    public void ClearUILists()
    {
        ClearAnchorChildren(characterListAnchor);
        ClearAnchorChildren(placeListAnchor);
    }

    private void ClearAnchorChildren(Transform anchor)
    {
        if (anchor == null) return;

        for (int i = anchor.childCount - 1; i >= 0; i--)
        {
            Destroy(anchor.GetChild(i).gameObject);
        }
    }
}