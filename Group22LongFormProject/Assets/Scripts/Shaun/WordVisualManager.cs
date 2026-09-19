using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct WordVisualMapping
{
    public string wordText;
    public GameObject visualPrefab; // Square or Triangle prefab
}

public class WordVisualManager : MonoBehaviour
{
    public static WordVisualManager Instance { get; private set; }

    [Header("Main Scene Display Anchors")]
    [SerializeField] private Transform characterSceneAnchor;
    [SerializeField] private Transform placeSceneAnchor;

    [Header("Word to Sprite Prefab Mappings")]
    [SerializeField] private List<WordVisualMapping> mappings;

    private Dictionary<string, GameObject> mappingDict;
    private Dictionary<WordCategory, GameObject> activeInstances = new Dictionary<WordCategory, GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeDictionary();
    }

    private void InitializeDictionary()
    {
        mappingDict = new Dictionary<string, GameObject>(StringComparer.OrdinalIgnoreCase);
        foreach (var mapping in mappings)
        {
            if (!string.IsNullOrEmpty(mapping.wordText) && mapping.visualPrefab != null)
            {
                mappingDict[mapping.wordText] = mapping.visualPrefab;
            }
        }
    }

    /// <summary>
    /// Fetches the prefab associated with a word string (used by DisplaySceneController).
    /// </summary>
    public GameObject GetPrefabForWord(string word)
    {
        if (string.IsNullOrEmpty(word)) return null;
        if (mappingDict == null) InitializeDictionary();

        mappingDict.TryGetValue(word, out GameObject prefab);
        return prefab;
    }

    /// <summary>
    /// Instantiates and displays a sprite preview at the main scene anchor (used by SentenceSlotUI).
    /// </summary>
    public void UpdateCategoryVisual(WordCategory category, string word)
    {
        ClearCategoryVisual(category);

        Transform targetAnchor = GetAnchorForCategory(category);
        if (targetAnchor == null || string.IsNullOrEmpty(word) || word == "No words found") return;

        GameObject prefab = GetPrefabForWord(word);
        if (prefab != null)
        {
            GameObject instance = Instantiate(prefab, targetAnchor);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            activeInstances[category] = instance;
        }
    }

    /// <summary>
    /// Clears any active visual object assigned to a specific category.
    /// </summary>
    public void ClearCategoryVisual(WordCategory category)
    {
        if (activeInstances.TryGetValue(category, out GameObject existingObj))
        {
            if (existingObj != null)
            {
                Destroy(existingObj);
            }
            activeInstances.Remove(category);
        }
    }

    /// <summary>
    /// Clears all active preview instances across all categories.
    /// </summary>
    public void ClearAllVisuals()
    {
        foreach (var kvp in activeInstances)
        {
            if (kvp.Value != null) Destroy(kvp.Value);
        }
        activeInstances.Clear();
    }

    private Transform GetAnchorForCategory(WordCategory category)
    {
        switch (category)
        {
            case WordCategory.Character:
                return characterSceneAnchor;
            case WordCategory.Place:
                return placeSceneAnchor;
            default:
                return null;
        }
    }
}