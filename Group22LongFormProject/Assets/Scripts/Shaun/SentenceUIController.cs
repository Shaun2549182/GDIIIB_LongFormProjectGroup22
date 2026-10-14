using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SentenceUIController : MonoBehaviour
{
    [Header("UI Containers")]
    [Tooltip("Primary container used for all single-container scenes (e.g. GameScene), or top text for Tutorial.")]
    [SerializeField] private Transform firstContainer;

    [Tooltip("Secondary container used ONLY in Tutorial scene for dropdown slots and bottom text.")]
    [SerializeField] private Transform secondContainer;

    [Header("Prefabs")]
    [SerializeField] private GameObject textSegmentPrefab;
    [SerializeField] private GameObject slotPrefab;

    [Header("Typewriter Settings")]
    [SerializeField] private float typeSpeed = 0.05f;

    private void OnEnable() => SentenceEvents.OnSentenceInit += BuildSentence;
    private void OnDisable() => SentenceEvents.OnSentenceInit -= BuildSentence;

    public void BuildSentence(string template)
    {
<<<<<<< Updated upstream
        // Clear existing elements in the container
        foreach (Transform child in sentenceContainer)
        {
            Destroy(child.gameObject);
        }

        // Split template text into segments using placeholder tags (e.g., {Character}, {Place})
=======
        // Cancel active LeanTween animations on this object
        LeanTween.cancel(gameObject);

        // Clear existing children from First Container
        if (firstContainer != null)
        {
            foreach (Transform child in firstContainer)
            {
                LeanTween.cancel(child.gameObject);
                Destroy(child.gameObject);
            }
        }

        // Clear existing children from Second Container if assigned
        if (secondContainer != null)
        {
            foreach (Transform child in secondContainer)
            {
                LeanTween.cancel(child.gameObject);
                Destroy(child.gameObject);
            }
        }

        // Split template text into segments using placeholder tags ({Character}, {Place}, etc.)
>>>>>>> Stashed changes
        string[] parts = Regex.Split(template, @"(\{.*?\})");

        bool reachedFirstSlot = false;
        ProcessNextPart(parts, 0, reachedFirstSlot);
    }

    private void ProcessNextPart(string[] parts, int index, bool reachedFirstSlot)
    {
        if (index >= parts.Length)
        {
            SentenceEvents.TriggerSentenceConstructionComplete();
            return;
        }

        string part = parts[index];

        if (string.IsNullOrEmpty(part))
        {
            ProcessNextPart(parts, index + 1, reachedFirstSlot);
            return;
        }

<<<<<<< Updated upstream
        // Handle Slot Dropdowns ({Character}, {Place}, etc.)
        if (part.StartsWith("{") && part.EndsWith("}"))
=======
        // Check if this part is a slot tag (e.g., {Character}, {Place})
        bool isSlotTag = part.StartsWith("{") && part.EndsWith("}");

        if (isSlotTag)
>>>>>>> Stashed changes
        {
            reachedFirstSlot = true;
        }

        // Only split across containers if active scene name is/contains "Tutorial" AND secondContainer is assigned
        string currentScene = SceneManager.GetActiveScene().name;
        bool isTutorialScene = currentScene.IndexOf("Tutorial", StringComparison.OrdinalIgnoreCase) >= 0;
        bool useTwoContainers = isTutorialScene && (secondContainer != null);

        // Determine target container
        Transform targetContainer;
        if (useTwoContainers && reachedFirstSlot)
        {
            targetContainer = secondContainer;
        }
        else
        {
            targetContainer = (firstContainer != null) ? firstContainer : transform;
        }

        // Handle Slot Dropdowns ({Character}, {Place}, etc.)
        if (isSlotTag)
        {
            GameObject slotObj = Instantiate(slotPrefab, targetContainer);
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
<<<<<<< Updated upstream
                Debug.LogError($"[SentenceUIController] SentenceSlotUI component missing on {slotPrefab.name} or its children!");
=======
                Debug.LogError($"[SentenceUIController] SentenceSlotUI missing on {slotPrefab.name}!");
>>>>>>> Stashed changes
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(targetContainer as RectTransform);
            ProcessNextPart(parts, index + 1, reachedFirstSlot);
        }
        // Handle Standard Typewritten Text
        else
        {
            GameObject textObj = Instantiate(textSegmentPrefab, targetContainer);
            TextMeshProUGUI tmpText = textObj.GetComponent<TextMeshProUGUI>();
            tmpText.text = string.Empty;

            float totalDuration = part.Length * typeSpeed;

            LeanTween.value(gameObject, 0f, part.Length, totalDuration)
                .setOnUpdate((float val) =>
                {
                    int charCount = Mathf.Clamp(Mathf.FloorToInt(val), 0, part.Length);
                    tmpText.text = part.Substring(0, charCount);
                    LayoutRebuilder.ForceRebuildLayoutImmediate(targetContainer as RectTransform);
                })
                .setOnComplete(() =>
                {
                    tmpText.text = part;
                    LayoutRebuilder.ForceRebuildLayoutImmediate(targetContainer as RectTransform);
                    ProcessNextPart(parts, index + 1, reachedFirstSlot);
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