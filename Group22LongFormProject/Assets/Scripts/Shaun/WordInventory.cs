using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct CollectedWord
{
    public string wordText;
    public WordCategory category;

    public CollectedWord(string wordText, WordCategory category)
    {
        this.wordText = wordText;
        this.category = category;
    }
}

public class WordInventory : MonoBehaviour
{
    public static WordInventory Instance { get; private set; }
    public static event Action<string, WordCategory> OnWordCollected;

    public List<CollectedWord> CollectedWords { get; private set; } = new List<CollectedWord>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddWord(string wordText, WordCategory category)
    {
        CollectedWords.Add(new CollectedWord(wordText, category));
        OnWordCollected?.Invoke(wordText, category);
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

    /// <summary>
    /// Clears all collected words from the inventory list when transitioning acts or resetting the game state.
    /// </summary>
    public void ClearInventory()
    {
        if (CollectedWords != null)
        {
            CollectedWords.Clear();
        }

        Debug.Log("[WordInventory] Inventory successfully cleared.");
    }
}