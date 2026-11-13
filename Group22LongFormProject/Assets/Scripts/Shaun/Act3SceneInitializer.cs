using UnityEngine;

public class Act3SceneInitializer : MonoBehaviour
{
    private void Start()
    {
        WordInventory.Instance?.ClearInventory();

        string act3Template = "At the end of the path, the hero faced a {Character} and was {Place}.";

        if (GameManager.Instance != null)
        {
            act3Template = GameManager.Instance.GetAct3SentenceTemplate();
        }

        SentenceEvents.TriggerSentenceInit(act3Template);
        Debug.Log($"[Act3SceneInitializer] Initialized Act 3 Assembly with template: '{act3Template}'");
    }
}