using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Act2DisplaySceneController : MonoBehaviour
{
    [SerializeField] private TMP_Text act2OutcomeText;
    [SerializeField] private Button continueToAct3Button;

    private void Start()
    {
        if (GameManager.Instance != null && act2OutcomeText != null)
        {
            // Display formatted Act 2 unique outcome sentence
            act2OutcomeText.text = GameManager.Instance.GetAct2DisplayStory();
        }

        if (continueToAct3Button != null)
        {
            continueToAct3Button.onClick.AddListener(OnContinueClicked);
        }
    }

    private void OnContinueClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ProceedToAct3();
        }
    }
}