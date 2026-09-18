using UnityEngine;

public class SceneInitializer : MonoBehaviour
{
    [TextArea]
    [SerializeField] private string startingSentence = "In the modern day, {Character} is a {Place}.";

    private void Start()
    {
        SentenceEvents.TriggerSentenceInit(startingSentence);
    }
}