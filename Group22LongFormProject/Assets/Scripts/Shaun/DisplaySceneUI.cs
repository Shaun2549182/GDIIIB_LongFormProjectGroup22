using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DisplaySceneUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI storyText;
    [SerializeField] private Button continueButton;

    private void Start()
    {
        UpdateStoryText();

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueClicked);
        }
    }

    public void UpdateStoryText()
    {
        if (GameManager.Instance != null && storyText != null)
        {
            string narrativeStory = GameManager.Instance.GetDisplayStory();
            storyText.text = narrativeStory;
            Debug.Log($"[DisplaySceneUI] Story updated: '{narrativeStory}'");
        }
        else
        {
            if (storyText == null)
            {
                Debug.LogError("[DisplaySceneUI] 'Story Text' reference is missing in the Inspector!");
            }
            if (GameManager.Instance == null)
            {
                Debug.LogError("[DisplaySceneUI] GameManager.Instance is null!");
            }
        }
    }

    private void OnContinueClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ProceedToAct2();
        }
    }
}