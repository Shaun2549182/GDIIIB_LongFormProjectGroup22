using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(TMP_Dropdown))]
public class SentenceSlot : MonoBehaviour, IPointerClickHandler
{
    private TMP_Dropdown dropdown;
    public WordCategory RequiredCategory { get; private set; } = WordCategory.Any;
    public string SelectedWord { get; private set; } = string.Empty;

    private void Awake()
    {
        dropdown = GetComponent<TMP_Dropdown>();
        dropdown.onValueChanged.AddListener(OnOptionSelected);
    }

    public void SetCategory(WordCategory category)
    {
        RequiredCategory = category;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        RefreshDropdownOptions();
    }

    private void RefreshDropdownOptions()
    {
        dropdown.ClearOptions();

        List<string> options = new List<string> { $"[ Select {RequiredCategory} ]" };

        if (WordInventory.Instance != null)
        {
            List<string> filteredWords = WordInventory.Instance.GetWordsByCategory(RequiredCategory);
            options.AddRange(filteredWords);
        }

        dropdown.AddOptions(options);
    }

    private void OnOptionSelected(int index)
    {
        if (index == 0)
        {
            SelectedWord = string.Empty;
        }
        else
        {
            SelectedWord = dropdown.options[index].text;
        }

        SentenceEvents.TriggerSlotUpdated();
    }
}