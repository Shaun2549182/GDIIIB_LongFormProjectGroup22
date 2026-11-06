using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct CollectedWord
{
    public string wordText;
    public WordCategory category;

    public CollectedWord(string text, WordCategory cat)
    {
        wordText = text;
        category = cat;
    }
}

public class WordInventory : MonoBehaviour
{
    public static WordInventory Instance { get; private set; }

    public static event Action<string, WordCategory> OnWordCollected;
    public static event Action OnInventoryCleared;

    public List<CollectedWord> CollectedWords { get; private set; } = new List<CollectedWord>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddWord(string wordText, WordCategory category)
    {
        CollectedWords.Add(new CollectedWord(wordText, category));
        OnWordCollected?.Invoke(wordText, category);
    }

    public void ClearInventory()
    {
        CollectedWords.Clear();
        OnInventoryCleared?.Invoke();
        Debug.Log("[WordInventory] Inventory cleared.");
    }

    public List<string> GetWordsByCategory(WordCategory category)
    {
        List<string> matchingWords = new List<string>();

        foreach (var item in CollectedWords)
        {
            if (category == WordCategory.Any || item.category == category)
            {
                matchingWords.Add(item.wordText);
            }
        }

        return matchingWords;
    }
}