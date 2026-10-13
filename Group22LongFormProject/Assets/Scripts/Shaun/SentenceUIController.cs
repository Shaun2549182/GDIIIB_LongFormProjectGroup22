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

    [Header("Scene Visual Anchors")]
    [SerializeField] private Transform characterAnchor;
    [SerializeField] private Transform placeAnchor;

    private void Awake()
    {
        // Re-register local scene anchors to persistent WordVisualManager
        if (WordVisualManager.Instance != null && characterAnchor != null && placeAnchor != null)
        {
            WordVisualManager.Instance.RegisterSceneAnchors(characterAnchor, placeAnchor);
        }
    }

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
        if (WordVisualManager.Instance != null && characterAnchor != null && placeAnchor != null)
        {
            WordVisualManager.Instance.RegisterSceneAnchors(characterAnchor, placeAnchor);
        }

        LeanTween.cancel(gameObject);

        foreach (Transform child in sentenceContainer)
        {
            LeanTween.cancel(child.gameObject);
            Destroy(child.gameObject);
        }

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

            LayoutRebuilder.ForceRebuildLayoutImmediate(sentenceContainer as RectTransform);
            ProcessNextPart(parts, index + 1);
        }
        else
        {
            GameObject textObj = Instantiate(textSegmentPrefab, sentenceContainer);
            TextMeshProUGUI tmpText = textObj.GetComponent<TextMeshProUGUI>();
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