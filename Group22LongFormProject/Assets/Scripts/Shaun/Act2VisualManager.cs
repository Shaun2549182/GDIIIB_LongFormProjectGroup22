using System;
using System.Collections.Generic;
using UnityEngine;

public class Act2VisualManager : MonoBehaviour
{
    [System.Serializable]
    public struct CharacterVisualMapping
    {
        public string characterName; // "Liam", "Ben", "Emma"
        public GameObject characterPrefab;
    }

    [Header("Spawn Position in Act 2")]
    [SerializeField] private Transform characterSpawnAnchor;

    [Header("Character Prefab Database")]
    [SerializeField] private List<CharacterVisualMapping> characterMappings = new List<CharacterVisualMapping>();

    private Dictionary<string, GameObject> mappingDict;

    private void Awake()
    {
        InitializeDictionary();
    }

    private void Start()
    {
        SpawnAct1Character();
    }

    private void InitializeDictionary()
    {
        mappingDict = new Dictionary<string, GameObject>(StringComparer.OrdinalIgnoreCase);
        foreach (var mapping in characterMappings)
        {
            if (!string.IsNullOrEmpty(mapping.characterName) && mapping.characterPrefab != null)
            {
                mappingDict[mapping.characterName.Trim()] = mapping.characterPrefab;
            }
        }
    }

    public void SpawnAct1Character()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[Act2VisualManager] GameManager.Instance is null!");
            return;
        }

        string selectedCharacter = GameManager.Instance.SelectedCharacterWord;

        if (string.IsNullOrEmpty(selectedCharacter))
        {
            Debug.LogWarning("[Act2VisualManager] No SelectedCharacterWord found in GameManager.");
            return;
        }

        if (characterSpawnAnchor == null)
        {
            Debug.LogError("[Act2VisualManager] Character Spawn Anchor is not assigned in the Inspector!");
            return;
        }

        // Clear any placeholder objects currently under the spawn anchor
        foreach (Transform child in characterSpawnAnchor)
        {
            Destroy(child.gameObject);
        }

        // Look up and instantiate the matching character prefab
        if (mappingDict != null && mappingDict.TryGetValue(selectedCharacter, out GameObject prefab))
        {
            GameObject characterInstance = Instantiate(prefab, characterSpawnAnchor.position, characterSpawnAnchor.rotation, characterSpawnAnchor);
            Debug.Log($"[Act2VisualManager] Successfully spawned Act 1 character: '{selectedCharacter}' in Act 2.");
        }
        else
        {
            Debug.LogWarning($"[Act2VisualManager] No prefab found for character: '{selectedCharacter}'");
        }
    }
}