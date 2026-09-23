using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Dropdown))]
public class SentenceSlotUI : MonoBehaviour
{
    [Header("Slot Settings")]
    [SerializeField] private WordCategory slotCategory = WordCategory.Any;

    private TMP_Dropdown dropdown;
    private bool isInitializing = false;

    private void Awake()
    {
        dropdown = GetComponent<TMP_Dropdown>();
        dropdown.interactable = false;

        dropdown.onValueChanged.RemoveAllListeners();
        dropdown.onValueChanged.AddListener(OnSelectionChanged);
    }

    private void OnEnable() => GameManager.OnPhaseChanged += HandlePhaseChanged;
    private void OnDisable() => GameManager.OnPhaseChanged -= HandlePhaseChanged;

    public void InitializeSlot(WordCategory category)
    {
        slotCategory = category;
        dropdown.interactable = false;

        if (GameManager.Instance != null && GameManager.Instance.CurrentPhase == GamePhase.Assembly)
        {
            PopulateDropdownOptions();
            dropdown.interactable = true;
        }
    }

    private void HandlePhaseChanged(GamePhase newPhase)
    {
        if (newPhase == GamePhase.Assembly)
        {
            PopulateDropdownOptions();
            dropdown.interactable = true;
        }
        else
        {
            dropdown.interactable = false;
            WordVisualManager.Instance?.ClearCategoryVisual(slotCategory);
        }
    }

    public void PopulateDropdownOptions()
    {
        if (dropdown == null || WordInventory.Instance == null) return;

        if (slotCategory == WordCategory.Any)
        {
            dropdown.ClearOptions();
            dropdown.options.Add(new TMP_Dropdown.OptionData("Select Word"));
            dropdown.interactable = false;
            return;
        }

        isInitializing = true;
        dropdown.onValueChanged.RemoveListener(OnSelectionChanged);

        dropdown.ClearOptions();

        // Retrieve words filtered strictly by this slot's category
        List<string> collectedWords = WordInventory.Instance.GetWordsByCategory(slotCategory);

        if (collectedWords == null || collectedWords.Count == 0)
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData("No words found"));
            dropdown.interactable = false;
            WordVisualManager.Instance?.ClearCategoryVisual(slotCategory);
        }
        else
        {
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
            foreach (string word in collectedWords)
            {
                options.Add(new TMP_Dropdown.OptionData(word));
            }
            dropdown.AddOptions(options);
            dropdown.interactable = true;

            dropdown.value = 0;
            dropdown.RefreshShownValue();

            string defaultWord = GetSelectedWord();
            WordVisualManager.Instance?.UpdateCategoryVisual(slotCategory, defaultWord);
        }

        dropdown.onValueChanged.AddListener(OnSelectionChanged);
        isInitializing = false;
    }

    private void OnSelectionChanged(int index)
    {
        if (isInitializing) return;

        string selectedWord = GetSelectedWord();
        if (!string.IsNullOrEmpty(selectedWord) && selectedWord != "BLANK")
        {
            WordVisualManager.Instance?.UpdateCategoryVisual(slotCategory, selectedWord);
        }
    }

    public string GetSelectedWord()
    {
        if (dropdown == null || dropdown.options.Count == 0) return string.Empty;
        return dropdown.options[dropdown.value].text;
    }

    public WordCategory GetSlotCategory() => slotCategory;
}