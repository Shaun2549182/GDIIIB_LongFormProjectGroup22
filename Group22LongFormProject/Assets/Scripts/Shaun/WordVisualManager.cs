using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public struct WordVisualMapping
{
    public string wordText;
    public GameObject visualPrefab;
}

public class WordVisualManager : MonoBehaviour
{
    public static WordVisualManager Instance { get; private set; }

    [Header("Act 1 & Shared Location Anchors")]
    [SerializeField] private Transform act1CharacterAnchor;
    [SerializeField] private Transform sharedPlaceAnchor; // Shared by Act 1 Place & Act 2 Place

    [Header("Act 2 Character Anchor (Discovered Item)")]
    [SerializeField] private Transform act2CharacterAnchor;

    [Header("Act 3 Anchors")]
    [SerializeField] private Transform act3CharacterAnchor;
    [SerializeField] private Transform act3PlaceAnchor;

    [Header("Word to Sprite Prefab Mappings")]
    [SerializeField] private List<WordVisualMapping> mappings = new List<WordVisualMapping>();

    private Dictionary<string, GameObject> mappingDict;
    private readonly Dictionary<(int act, WordCategory category), GameObject> activeInstances =
        new Dictionary<(int act, WordCategory category), GameObject>();

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
        activeInstances.Clear();
        RebindSceneAnchors();
        RestorePersistedVisuals();
    }

    /// <summary>
    /// Locates scene anchor GameObjects in the active hierarchy.
    /// </summary>
    public void RebindSceneAnchors()
    {
        // Act 1 Character Anchor
        GameObject a1Char = GameObject.Find("Act1CharacterAnchor") ?? GameObject.Find("CharacterAnchor");
        if (a1Char != null) act1CharacterAnchor = a1Char.transform;

        // Shared Place Anchor (used by both Act 1 Place and Act 2 Location)
        GameObject placeObj = GameObject.Find("PlaceAnchor")
            ?? GameObject.Find("Act1PlaceAnchor")
            ?? GameObject.Find("SharedPlaceAnchor");
        if (placeObj != null) sharedPlaceAnchor = placeObj.transform;

        // Act 2 Character Anchor (Discovered Item)
        GameObject a2Char = GameObject.Find("Act2CharacterAnchor");
        if (a2Char != null) act2CharacterAnchor = a2Char.transform;

        // Act 3 Anchors
        GameObject a3Char = GameObject.Find("Act3CharacterAnchor");
        if (a3Char != null) act3CharacterAnchor = a3Char.transform;

        GameObject a3Place = GameObject.Find("Act3PlaceAnchor");
        if (a3Place != null) act3PlaceAnchor = a3Place.transform;
    }

    /// <summary>
    /// Restores sprites across acts, using the Act 2 location on the shared Place anchor when chosen,
    /// or falling back to the Act 1 location if Act 2's location slot is empty.
    /// </summary>
    public void RestorePersistedVisuals()
    {
        if (GameManager.Instance == null) return;

        int currentAct = GameManager.Instance.CurrentAct;

        // 1. Act 1 Character
        SpawnIfPresent(GameManager.Instance.SelectedCharacterWord, WordCategory.Character, 1);

        // 2. Shared Place Anchor (Act 2 Location overrides Act 1 Location once selected)
        string activePlaceWord = (currentAct >= 2 && !string.IsNullOrEmpty(GameManager.Instance.Act2SelectedPlaceWord))
            ? GameManager.Instance.Act2SelectedPlaceWord
            : GameManager.Instance.SelectedPlaceWord;

        SpawnIfPresent(activePlaceWord, WordCategory.Place, 1);

        // 3. Act 2 Character (Discovered Item)
        if (currentAct >= 2)
        {
            SpawnIfPresent(GameManager.Instance.Act2SelectedCharacterWord, WordCategory.Character, 2);
        }

        // 4. Act 3 Character & Outcome
        if (currentAct >= 3)
        {
            SpawnIfPresent(GameManager.Instance.Act3SelectedCharacterWord, WordCategory.Character, 3);
            SpawnIfPresent(GameManager.Instance.Act3SelectedPlaceWord, WordCategory.Place, 3);
        }
    }

    private void SpawnIfPresent(string word, WordCategory category, int visualSlotAct)
    {
        if (string.IsNullOrEmpty(word)) return;

        Transform anchor = GetAnchorForSlot(visualSlotAct, category);
        if (anchor != null)
        {
            SpawnVisualAtAnchor(word, category, anchor, isDisplayPhase: false, act: visualSlotAct);
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

    public GameObject SpawnVisualAtAnchor(string word, WordCategory category, Transform targetAnchor, bool isDisplayPhase = false, int act = 1)
    {
        DestroyVisualInstance(category, act);

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

        activeInstances[(act, category)] = instance;
        return instance;
    }

    public void UpdateCategoryVisual(WordCategory category, string word)
    {
        int currentAct = GameManager.Instance != null ? GameManager.Instance.CurrentAct : 1;

        // Route Act 2 Place words to the same shared Place anchor (slot key 1)
        int visualSlotAct = (category == WordCategory.Place && currentAct == 2) ? 1 : currentAct;

        Transform targetAnchor = GetAnchorForSlot(visualSlotAct, category);
        SpawnVisualAtAnchor(word, category, targetAnchor, isDisplayPhase: false, act: visualSlotAct);
    }

    public void ClearCategoryVisual(WordCategory category, int act = -1)
    {
        int currentAct = GameManager.Instance != null ? GameManager.Instance.CurrentAct : 1;
        int targetAct = act < 0 ? currentAct : act;
        int visualSlotAct = (category == WordCategory.Place && targetAct == 2) ? 1 : targetAct;

        DestroyVisualInstance(category, visualSlotAct);

        // If the Act 2 Place word was removed from its slot, revert the shared anchor to Act 1's Place word
        if (act < 0 && currentAct == 2 && category == WordCategory.Place && GameManager.Instance != null)
        {
            string act1Place = GameManager.Instance.SelectedPlaceWord;
            if (!string.IsNullOrEmpty(act1Place))
            {
                SpawnIfPresent(act1Place, WordCategory.Place, 1);
            }
        }
    }

    private void DestroyVisualInstance(WordCategory category, int visualSlotAct)
    {
        var key = (visualSlotAct, category);
        if (activeInstances.TryGetValue(key, out GameObject existingObj))
        {
            if (existingObj != null)
            {
                Destroy(existingObj);
            }
            activeInstances.Remove(key);
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

    private Transform GetAnchorForSlot(int visualSlotAct, WordCategory category)
    {
        RebindIfMissing(visualSlotAct, category);

        return (visualSlotAct, category) switch
        {
            (1, WordCategory.Character) => act1CharacterAnchor,
            (1, WordCategory.Place) => sharedPlaceAnchor,
            (2, WordCategory.Character) => act2CharacterAnchor,
            (3, WordCategory.Character) => act3CharacterAnchor,
            (3, WordCategory.Place) => act3PlaceAnchor,
            _ => null
        };
    }

    private void RebindIfMissing(int visualSlotAct, WordCategory category)
    {
        bool missing = (visualSlotAct, category) switch
        {
            (1, WordCategory.Character) => act1CharacterAnchor == null,
            (1, WordCategory.Place) => sharedPlaceAnchor == null,
            (2, WordCategory.Character) => act2CharacterAnchor == null,
            (3, WordCategory.Character) => act3CharacterAnchor == null,
            (3, WordCategory.Place) => act3PlaceAnchor == null,
            _ => false
        };

        if (missing) RebindSceneAnchors();
    }
}