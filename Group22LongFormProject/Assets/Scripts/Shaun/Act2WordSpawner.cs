using System.Collections.Generic;
using UnityEngine;

public class Act2WordSpawner : MonoBehaviour
{
    [Header("Scene Brick Layout References")]
    [SerializeField] private List<Brick> characterBricks; // Assign your 3 Object Bricks here
    [SerializeField] private List<Brick> placeBricks;     // Assign your 4 Adverb Bricks here

    private void Start()
    {
        PopulateAct2Bricks();
    }

    public void PopulateAct2Bricks()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[Act2WordSpawner] GameManager.Instance is null!");
            return;
        }

        // Exact method call on GameManager
        List<string> objectWords = GameManager.Instance.GetAct2WordsForCategory(WordCategory.Character);
        List<string> adverbWords = GameManager.Instance.GetAct2WordsForCategory(WordCategory.Place);

        // Populate Object Bricks (Category: Character -> 3 words)
        for (int i = 0; i < characterBricks.Count; i++)
        {
            if (characterBricks[i] == null) continue;

            if (i < objectWords.Count)
            {
                characterBricks[i].SetWord(objectWords[i], WordCategory.Character);
            }
            else
            {
                characterBricks[i].gameObject.SetActive(false); // Hide brick if fewer words exist
            }
        }

        // Populate Adverb Bricks (Category: Place -> 4 words)
        for (int i = 0; i < placeBricks.Count; i++)
        {
            if (placeBricks[i] == null) continue;

            if (i < adverbWords.Count)
            {
                placeBricks[i].SetWord(adverbWords[i], WordCategory.Place);
            }
            else
            {
                placeBricks[i].gameObject.SetActive(false); // Hide brick if fewer words exist
            }
        }

        Debug.Log("[Act2WordSpawner] Act 2 Bricks successfully populated with unique outcome words!");
    }
}