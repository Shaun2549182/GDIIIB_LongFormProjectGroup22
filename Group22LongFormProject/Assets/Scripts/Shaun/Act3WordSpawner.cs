using System.Collections.Generic;
using UnityEngine;

public class Act3WordSpawner : MonoBehaviour
{
    [Header("Scene Brick Layout References")]
    [SerializeField] private List<Brick> characterBricks; // Assign 3 Secondary Character Bricks
    [SerializeField] private List<Brick> placeBricks;     // Assign 4 Weather Bricks

    private void Start()
    {
        PopulateAct3Bricks();
    }

    public void PopulateAct3Bricks()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[Act3WordSpawner] GameManager.Instance is null!");
            return;
        }

        // Fetch outcome-specific Act 3 words from GameManager (Act = 3)
        List<string> secondaryCharWords = GameManager.Instance.GetActWordsForCategory(3, WordCategory.Character);
        List<string> weatherWords = GameManager.Instance.GetActWordsForCategory(3, WordCategory.Place);

        // Populate Secondary Character Bricks (Category: Character -> 3 words)
        for (int i = 0; i < characterBricks.Count; i++)
        {
            if (characterBricks[i] == null) continue;

            if (i < secondaryCharWords.Count)
            {
                characterBricks[i].SetWord(secondaryCharWords[i], WordCategory.Character);
            }
            else
            {
                characterBricks[i].gameObject.SetActive(false);
            }
        }

        // Populate Weather Bricks (Category: Place -> 4 words)
        for (int i = 0; i < placeBricks.Count; i++)
        {
            if (placeBricks[i] == null) continue;

            if (i < weatherWords.Count)
            {
                placeBricks[i].SetWord(weatherWords[i], WordCategory.Place);
            }
            else
            {
                placeBricks[i].gameObject.SetActive(false);
            }
        }

        Debug.Log("[Act3WordSpawner] Act 3 Bricks successfully populated with Secondary Characters and Weather words!");
    }
}