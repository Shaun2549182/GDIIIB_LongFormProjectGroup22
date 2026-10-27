using UnityEngine;
using TMPro;

public class Act2DisplaySceneController : MonoBehaviour
{
    [SerializeField] private TMP_Text storyText;

    private void Start()
    {
        if (GameManager.Instance != null && storyText != null)
        {
            // Fetches formatted Act 2 template sentence
            storyText.text = GameManager.Instance.GetAct2DisplayStory();
        }
    }
}