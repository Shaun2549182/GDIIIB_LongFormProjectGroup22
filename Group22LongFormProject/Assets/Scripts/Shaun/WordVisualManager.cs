using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct WordVisualMapping
{
    public string wordText;
    public Sprite visualSprite;
}

public class WordVisualManager : MonoBehaviour
{
    public static WordVisualManager Instance { get; private set; }

    [Header("Main Scene Display Anchors")]
    [SerializeField] private Transform characterSceneAnchor;
    [SerializeField] private Transform placeSceneAnchor;

    [Header("Assembly Phase Scales")]
    [SerializeField] private Vector3 assemblyCharacterScale = new Vector3(250f, 250f, 1f);
    [SerializeField] private Vector3 assemblyPlaceScale = new Vector3(0.6f, 0.6f, 1f);

    [Header("Display Phase Scales")]
    [SerializeField] private Vector3 displayCharacterScale = new Vector3(15f, 15f, 1f);
    [SerializeField] private Vector3 displayPlaceScale = new Vector3(1f, 1f, 1f);

    [Header("Word to Sprite Mappings")]
    [SerializeField] private List<WordVisualMapping> mappings;

    private Dictionary<string, Sprite> mappingDict;
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

    public void RegisterSceneAnchors(Transform charAnchor, Transform placeAnchor)
    {
        characterSceneAnchor = charAnchor;
        placeSceneAnchor = placeAnchor;
    }

    private void InitializeDictionary()
    {
        mappingDict = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
        foreach (var mapping in mappings)
        {
            if (!string.IsNullOrEmpty(mapping.wordText) && mapping.visualSprite != null)
            {
                mappingDict[mapping.wordText] = mapping.visualSprite;
            }
        }
    }

    public Sprite GetSpriteForWord(string word)
    {
        if (string.IsNullOrEmpty(word)) return null;
        if (mappingDict == null) InitializeDictionary();

        mappingDict.TryGetValue(word, out Sprite sprite);
        return sprite;
    }

    public Vector3 GetScaleForCategory(WordCategory category, bool isDisplayPhase = false)
    {
        if (isDisplayPhase)
        {
            switch (category)
            {
                case WordCategory.Character: return displayCharacterScale;
                case WordCategory.Place: return displayPlaceScale;
                default: return Vector3.one;
            }
        }
        else
        {
            switch (category)
            {
                case WordCategory.Character: return assemblyCharacterScale;
                case WordCategory.Place: return assemblyPlaceScale;
                default: return Vector3.one;
            }
        }
    }

    public GameObject SpawnVisualAtAnchor(string word, WordCategory category, Transform anchor, bool isDisplayPhase = false)
    {
        if (anchor == null || string.IsNullOrEmpty(word) || word == "No words found") return null;

        Sprite sprite = GetSpriteForWord(word);
        if (sprite != null)
        {
            GameObject instance = new GameObject($"Visual_{word}");
            instance.transform.SetParent(anchor, false);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = GetScaleForCategory(category, isDisplayPhase);

            // Handle UI Canvas rendering
            if (anchor is RectTransform || anchor.GetComponentInParent<Canvas>() != null)
            {
                Image img = instance.AddComponent<Image>();
                img.sprite = sprite;
                img.preserveAspect = true;

                // Moves this object to the top of its parent's hierarchy so it draws BEHIND sibling UI elements
                instance.transform.SetAsFirstSibling();
            }
            // Handle 2D World Space rendering
            else
            {
                SpriteRenderer sr = instance.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;

                // Negative sorting order forces rendering BEHIND standard world objects (default order = 0)
                sr.sortingOrder = -100;
            }

            return instance;
        }

        return null;
    }

    public void UpdateCategoryVisual(WordCategory category, string word)
    {
        ClearCategoryVisual(category);

        Transform targetAnchor = GetAnchorForCategory(category);
        if (targetAnchor == null) return;

        GameObject instance = SpawnVisualAtAnchor(word, category, targetAnchor, isDisplayPhase: false);

        if (instance != null)
        {
            activeInstances[category] = instance;
        }
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
        switch (category)
        {
            case WordCategory.Character: return characterSceneAnchor;
            case WordCategory.Place: return placeSceneAnchor;
            default: return null;
        }
    }
}