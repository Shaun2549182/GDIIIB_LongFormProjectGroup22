using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SentenceUIController : MonoBehaviour
{
    [Header("UI Containers & Prefabs")]
    [SerializeField] private Transform sentenceContainer;
    [SerializeField] private GameObject textSegmentPrefab;
    [SerializeField] private GameObject slotPrefab;

    [Header("Typewriter Settings")]
    [SerializeField] private float typeSpeed = 0.05f;

    [Header("Scene-Specific Font Settings")]
    [SerializeField] private bool overrideFontSize = false;
    [SerializeField] private float customFontSize = 24f;

    private void OnEnable()
    {
        SentenceEvents.OnSentenceInit -= BuildSentence;
        SentenceEvents.OnSentenceInit += BuildSentence;
    }

    private void OnDisable()
    {
        SentenceEvents.OnSentenceInit -= BuildSentence;
    }

    public void BuildSentence(string template)
    {
        // Stop any active typewriter tweens on this object to prevent ghost completion callbacks
        LeanTween.cancel(gameObject);

        // Destroy existing container children and cancel their individual tweens
        foreach (Transform child in sentenceContainer)
        {
            LeanTween.cancel(child.gameObject);
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
                Debug.LogError($"[SentenceUIController] SentenceSlotUI missing on {slotPrefab.name}!");
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(sentenceContainer as RectTransform);
            ProcessNextPart(parts, index + 1);
        }
        // Handle Standard Typewritten Text
        else
        {
            GameObject textObj = Instantiate(textSegmentPrefab, sentenceContainer);
            TextMeshProUGUI tmpText = textObj.GetComponent<TextMeshProUGUI>();

            // Apply scene-specific font size override if enabled
            if (overrideFontSize && tmpText != null)
            {
                tmpText.fontSize = customFontSize;
            }

            tmpText.text = string.Empty;

            float totalDuration = part.Length * typeSpeed;

            LeanTween.value(gameObject, 0f, part.Length, totalDuration)
                .setOnUpdate((float val) =>
                {
                    if (tmpText == null) return;
                    int charCount = Mathf.Clamp(Mathf.FloorToInt(val), 0, part.Length);
                    tmpText.text = part.Substring(0, charCount);
                    LayoutRebuilder.ForceRebuildLayoutImmediate(sentenceContainer as RectTransform);
                })
                .setOnComplete(() =>
                {
                    if (tmpText == null) return;
                    tmpText.text = part;
                    LayoutRebuilder.ForceRebuildLayoutImmediate(sentenceContainer as RectTransform);
                    ProcessNextPart(parts, index + 1);
                });
        }
    }

    private WordCategory ParseCategoryFromTag(string tag)
    {
        string cleanTag = tag.Trim('{', '}').Trim();

        if (Enum.TryParse<WordCategory>(cleanTag, true, out var category))
        {
            return category;
        }

        return WordCategory.Any;
    }
}