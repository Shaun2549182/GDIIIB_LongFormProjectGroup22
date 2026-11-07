using UnityEngine;

public class Act3SceneInitializer : MonoBehaviour
{
    private void Start()
    {
        WordInventory.Instance?.ClearInventory();

        string act3Template = "Braving the {Place}, the hero sought out a {Character}.";

        if (GameManager.Instance != null)
        {
            act3Template = GameManager.Instance.GetCurrentOutcome().act3Template;
        }

        SentenceEvents.TriggerSentenceInit(act3Template);
        Debug.Log($"[Act3SceneInitializer] Initialized Act 3 Assembly with template: '{act3Template}'");
    }
}