using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SentenceUIController : MonoBehaviour
{
    [SerializeField] private GameObject textSegmentPrefab;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Transform sentenceContainer;
    [SerializeField] private float typeSpeed = 0.05f;

    private void OnEnable() => SentenceEvents.OnSentenceInit += BuildSentence;
    private void OnDisable() => SentenceEvents.OnSentenceInit -= BuildSentence;

    public void BuildSentence(string template)
    {
        // Clear existing elements in the container
        foreach (Transform child in sentenceContainer)
        {
            Destroy(child.gameObject);
        }

        // Split template text into segments using placeholder tags (e.g., {Character}, {Place})
        string[] parts = Regex.Split(template, @"(\{.*?\})");
        ProcessNextPart(parts, 0);
    }

    private void ProcessNextPart(string[] parts, int index)
    {
        if (index >= parts.Length)
        {
            SentenceEvents.TriggerSentenceConstructionComplete();
            return;
        }

        string part = parts[index];

        if (string.IsNullOrEmpty(part))
        {
            ProcessNextPart(parts, index + 1);
            return;
        }

        // Handle Slot Dropdowns ({Character}, {Place}, etc.)
        if (part.StartsWith("{") && part.EndsWith("}"))
        {
            GameObject slotObj = Instantiate(slotPrefab, sentenceContainer);
            WordCategory category = ParseCategoryFromTag(part);

            // Check root and child objects for SentenceSlotUI component
            SentenceSlotUI slotUI = slotObj.GetComponent<SentenceSlotUI>();
            if (slotUI == null)
            {
                slotUI = slotObj.GetComponentInChildren<SentenceSlotUI>();
            }

            if (slotUI != null)
            {
                slotUI.InitializeSlot(category);
            }
            else
            {
                Debug.LogError($"[SentenceUIController] SentenceSlotUI component missing on {slotPrefab.name} or its children!");
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(sentenceContainer as RectTransform);
            ProcessNextPart(parts, index + 1);
        }
        // Handle Standard Typewritten Text
        else
        {
            GameObject textObj = Instantiate(textSegmentPrefab, sentenceContainer);
            TextMeshProUGUI tmpText = textObj.GetComponent<TextMeshProUGUI>();
            tmpText.text = string.Empty;

            float totalDuration = part.Length * typeSpeed;

            LeanTween.value(gameObject, 0f, part.Length, totalDuration)
                .setOnUpdate((float val) =>
                {
                    int charCount = Mathf.Clamp(Mathf.FloorToInt(val), 0, part.Length);
                    tmpText.text = part.Substring(0, charCount);
                    LayoutRebuilder.ForceRebuildLayoutImmediate(sentenceContainer as RectTransform);
                })
                .setOnComplete(() =>
                {
                    tmpText.text = part;
                    LayoutRebuilder.ForceRebuildLayoutImmediate(sentenceContainer as RectTransform);
                    ProcessNextPart(parts, index + 1);
                });
        }
    }

    private WordCategory ParseCategoryFromTag(string tag)
    {
        // Strip curly braces: "{Character}" -> "Character"
        string cleanTag = tag.Trim('{', '}').Trim();

        if (Enum.TryParse<WordCategory>(cleanTag, true, out var category))
        {
            return category;
        }

        return WordCategory.Any;
    }
}