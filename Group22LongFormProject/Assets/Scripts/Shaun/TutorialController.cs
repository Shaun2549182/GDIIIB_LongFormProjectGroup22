using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TutorialController : MonoBehaviour
{
    [Header("Tutorial Text Boxes")]
    [Tooltip("Drag your text box GameObjects here in sequential order.")]
    [SerializeField] private List<GameObject> textBoxes = new List<GameObject>();

    [Header("Navigation Buttons")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button backButton;

    [Header("Scene Transition")]
    [SerializeField] private string gameSceneName = "GameScene";

    private int currentIndex = 0;

    private void Awake()
    {
        if (nextButton != null) nextButton.onClick.AddListener(OnNextClicked);
        if (backButton != null) backButton.onClick.AddListener(OnBackClicked);
    }

    private void Start()
    {
        ShowStep(0);
    }

    private void ShowStep(int index)
    {
        if (textBoxes == null || textBoxes.Count == 0) return;

        currentIndex = Mathf.Clamp(index, 0, textBoxes.Count - 1);

        // Enable only the active text box object
        for (int i = 0; i < textBoxes.Count; i++)
        {
            if (textBoxes[i] != null)
            {
                textBoxes[i].SetActive(i == currentIndex);
            }
        }

        // Hide back button on first box, show on step 2 (index 1) and beyond
        if (backButton != null)
        {
            backButton.gameObject.SetActive(currentIndex > 0);
        }
    }

    private void OnNextClicked()
    {
        if (currentIndex < textBoxes.Count - 1)
        {
            ShowStep(currentIndex + 1);
        }
        else
        {
            // Final text box reached: transition to GameScene
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartGame();
            }
            else
            {
                SceneManager.LoadScene(gameSceneName);
            }
        }
    }

    private void OnBackClicked()
    {
        if (currentIndex > 0)
        {
            ShowStep(currentIndex - 1);
        }
    }
}