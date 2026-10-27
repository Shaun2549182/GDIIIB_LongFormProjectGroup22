using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public struct WordVisualMapping
{
    public string wordText;
    public GameObject visualPrefab; // Square or Triangle prefab
}

public class WordVisualManager : MonoBehaviour
{
    public static WordVisualManager Instance { get; private set; }

    [Header("Main Scene Display Anchors (Fallback Inspector References)")]
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

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Clear destroyed visual instance references from previous scene
        activeInstances.Clear();

        // Relocate visual anchors in newly loaded scene
        RebindSceneAnchors();
    }

    /// <summary>
    /// Searches active scene for anchor objects named "CharacterAnchor" and "PlaceAnchor"
    /// </summary>
    public void RebindSceneAnchors()
    {
        GameObject charObj = GameObject.Find("CharacterAnchor");
        if (charObj != null)
        {
            characterSceneAnchor = charObj.transform;
            Debug.Log("[WordVisualManager] Successfully bound 'CharacterAnchor'.");
        }
        else
        {
            Debug.LogWarning("[WordVisualManager] Failed to find 'CharacterAnchor' in current scene!");
        }

        GameObject placeObj = GameObject.Find("PlaceAnchor");
        if (placeObj != null)
        {
            placeSceneAnchor = placeObj.transform;
            Debug.Log("[WordVisualManager] Successfully bound 'PlaceAnchor'.");
        }
        else
        {
            Debug.LogWarning("[WordVisualManager] Failed to find 'PlaceAnchor' in current scene!");
        }
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

    public GameObject GetPrefabForWord(string word)
    {
        if (string.IsNullOrEmpty(word)) return null;
        if (mappingDict == null) InitializeDictionary();

        mappingDict.TryGetValue(word, out GameObject prefab);
        return prefab;
    }

    public void UpdateCategoryVisual(WordCategory category, string word)
    {
        Transform targetAnchor = GetAnchorForCategory(category);
        SpawnVisualAtAnchor(word, category, targetAnchor);
    }

    /// <summary>
    /// Spawns a word visual prefab under a specific anchor transform.
    /// </summary>
    public GameObject SpawnVisualAtAnchor(string word, WordCategory category, Transform targetAnchor, bool isDisplayPhase = false)
    {
        ClearCategoryVisual(category);

        if (targetAnchor == null)
        {
            Debug.LogError($"[WordVisualManager] Aborted: Anchor for '{category}' is NULL or destroyed!");
            return null;
        }

        if (string.IsNullOrEmpty(word) || word == "No words found")
        {
            Debug.LogWarning($"[WordVisualManager] Aborted: Word string is invalid ('{word}').");
            return null;
        }

        GameObject prefab = GetPrefabForWord(word);
        if (prefab == null)
        {
            Debug.LogError($"[WordVisualManager] Aborted: No prefab mapping found for word '{word}' in mappingDict!");
            return null;
        }

        GameObject instance = Instantiate(prefab, targetAnchor);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;

        activeInstances[category] = instance;
        Debug.Log($"[WordVisualManager] Successfully instantiated '{prefab.name}' under anchor '{targetAnchor.name}'.");
        return instance;
    }

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
        if ((category == WordCategory.Character && characterSceneAnchor == null) ||
            (category == WordCategory.Place && placeSceneAnchor == null))
        {
            RebindSceneAnchors();
        }

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