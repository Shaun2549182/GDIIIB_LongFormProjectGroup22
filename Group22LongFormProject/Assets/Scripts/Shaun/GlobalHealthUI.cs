using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class GlobalHealthUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private string textPrefix = "Lives: ";
    [SerializeField] private string tutorialText = "Unlimited";

    private void Awake()
    {
        if (livesText == null) livesText = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        GameManager.OnHealthChanged += UpdateHealthDisplay;

        if (GameManager.Instance != null)
        {
            UpdateHealthDisplay(GameManager.Instance.CurrentHealth);
        }
        else
        {
            CheckTutorialOverride();
        }
    }

    private void OnDisable()
    {
        GameManager.OnHealthChanged -= UpdateHealthDisplay;
    }

    private void UpdateHealthDisplay(int currentHealth)
    {
        if (livesText == null) return;

        bool isTutorial = SceneManager.GetActiveScene().name.IndexOf("Tutorial", System.StringComparison.OrdinalIgnoreCase) >= 0;

        if (isTutorial)
        {
            livesText.text = $"{textPrefix}{tutorialText}";
        }
        else
        {
            livesText.text = $"{textPrefix}{currentHealth}";
        }
    }

    private void CheckTutorialOverride()
    {
        bool isTutorial = SceneManager.GetActiveScene().name.IndexOf("Tutorial", System.StringComparison.OrdinalIgnoreCase) >= 0;
        if (isTutorial && livesText != null)
        {
            livesText.text = $"{textPrefix}{tutorialText}";
        }
    }
}