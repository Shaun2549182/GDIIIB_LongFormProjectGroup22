using UnityEngine;
using TMPro;

public class Act2SceneInitializer : MonoBehaviour
{
    [Header("Optional UI Font Adjustment")]
    [SerializeField] private TextMeshProUGUI sentenceText;
    [SerializeField] private bool overrideFontSize = false;
    [SerializeField] private float act2FontSize = 24f;

    private void Start()
    {
        // Clear word inventory for Act 2 collection
        WordInventory.Instance?.ClearInventory();

        // Optional runtime font adjustment for longer Act 2 sentence templates
        if (overrideFontSize && sentenceText != null)
        {
            sentenceText.fontSize = act2FontSize;
        }

        string act2Template = "Later that day, {Character} needed to {Place} find an item.";

        // Query GameManager for the outcome matching Act 1 choices
        if (GameManager.Instance != null)
        {
            act2Template = GameManager.Instance.GetAct2SentenceTemplate();
        }

        // Initialize UI with template
        SentenceEvents.TriggerSentenceInit(act2Template);
        Debug.Log($"[Act2SceneInitializer] Initialized Act 2 with template: '{act2Template}'");
    }
}