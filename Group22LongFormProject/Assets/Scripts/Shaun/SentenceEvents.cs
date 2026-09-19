using System;

public enum WordCategory
{
    Any,
    Character,
    Place
}

public static class SentenceEvents
{
    // Fired when a new sentence template is sent to the UI
    public static event Action<string> OnSentenceInit;

    // Fired when LeanTween finishes typing out the sentence and spawning slots
    public static event Action OnSentenceConstructionComplete;

    // Fired whenever the player selects a word inside a slot dropdown
    public static event Action OnSlotUpdated;

    public static void TriggerSentenceInit(string template) => OnSentenceInit?.Invoke(template);
    public static void TriggerSentenceConstructionComplete() => OnSentenceConstructionComplete?.Invoke();
    public static void TriggerSlotUpdated() => OnSlotUpdated?.Invoke();
}