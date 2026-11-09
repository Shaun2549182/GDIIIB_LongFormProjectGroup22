using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Act3DisplaySceneController : MonoBehaviour
{
    [SerializeField] private TMP_Text finalStoryText;
    [SerializeField] private Button replayButton;
    [SerializeField] private Button quitButton;

    private void Start()
    {
        if (GameManager.Instance != null && finalStoryText != null)
        {
            finalStoryText.text = GameManager.Instance.GetAct3DisplayStory();
        }

        if (replayButton != null)
        {
            replayButton.onClick.AddListener(() => GameManager.Instance?.RestartGame());
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(() => GameManager.Instance?.QuitGame());
        }
    }
}