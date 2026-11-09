using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public struct WordVisualMapping
{
    public string wordText;
    public GameObject visualPrefab; // Visual prefab / model / sprite container
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

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 1. Clear references to destroyed instances from previous scene
        activeInstances.Clear();

        // 2. Locate anchors in the newly loaded scene
        RebindSceneAnchors();

        // 3. Automatically restore Act 1 selections in Act 2 scenes or display scenes
        RestorePersistedVisuals();
    }

    /// <summary>
    /// Searches the active scene hierarchy for GameObjects named "CharacterAnchor" and "PlaceAnchor"
    /// </summary>
    public void RebindSceneAnchors()
    {
        GameObject charObj = GameObject.Find("CharacterAnchor");
        if (charObj != null)
        {
            characterSceneAnchor = charObj.transform;
        }

        GameObject placeObj = GameObject.Find("PlaceAnchor");
        if (placeObj != null)
        {
            placeSceneAnchor = placeObj.transform;
        }
    }

    /// <summary>
    /// Re-instantiates saved character and place visual prefabs when transitioning across scenes or acts.
    /// </summary>
    public void RestorePersistedVisuals()
    {
        if (GameManager.Instance == null) return;

        string savedCharacter = GameManager.Instance.SelectedCharacterWord;
        string savedPlace = GameManager.Instance.SelectedPlaceWord;

        if (!string.IsNullOrEmpty(savedCharacter) && characterSceneAnchor != null)
        {
            SpawnVisualAtAnchor(savedCharacter, WordCategory.Character, characterSceneAnchor);
        }

        if (!string.IsNullOrEmpty(savedPlace) && placeSceneAnchor != null)
        {
            SpawnVisualAtAnchor(savedPlace, WordCategory.Place, placeSceneAnchor);
        }
    }

    private void InitializeDictionary()
    {
        mappingDict = new Dictionary<string, GameObject>(StringComparer.OrdinalIgnoreCase);
        foreach (var mapping in mappings)
        {
            if (!string.IsNullOrEmpty(mapping.wordText) && mapping.visualPrefab != null)
            {
                mappingDict[mapping.wordText.Trim()] = mapping.visualPrefab;
            }
        }
    }

    public GameObject GetPrefabForWord(string word)
    {
        if (string.IsNullOrEmpty(word)) return null;
        if (mappingDict == null) InitializeDictionary();

        mappingDict.TryGetValue(word.Trim(), out GameObject prefab);
        return prefab;
    }


    

    public GameObject SpawnVisualAtAnchor(string word, WordCategory category, Transform targetAnchor, bool isDisplayPhase = false)
    {
        ClearCategoryVisual(category);

        if (targetAnchor == null || string.IsNullOrEmpty(word) || word == "No words found")
        {
            return null;
        }

        GameObject prefab = GetPrefabForWord(word);
        if (prefab == null)
        {
            Debug.LogWarning($"[WordVisualManager] No visual prefab mapping found for word '{word}'!");
            return null;
        }

        GameObject instance = Instantiate(prefab, targetAnchor);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;

        activeInstances[category] = instance;
        return instance;
    }

    public void UpdateCategoryVisual(WordCategory category, string word)
    {
        Transform targetAnchor = GetAnchorForCategory(category);
        SpawnVisualAtAnchor(word, category, targetAnchor, isDisplayPhase: false);
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