using System;

public enum WordCategory
{
    Any,
    Character,
    Place
}

public static class SentenceEvents
{
    // Events
    public static event Action<string> OnSentenceInit;
    public static event Action OnSentenceConstructionComplete;
    public static event Action<WordCategory, string> OnSlotUpdated;

    // Triggers
    public static void TriggerSentenceInit(string template)
    {
        OnSentenceInit?.Invoke(template);
    }

    public static void TriggerSentenceConstructionComplete()
    {
        OnSentenceConstructionComplete?.Invoke();
    }

    public static void TriggerSlotUpdated(WordCategory category, string word)
    {
        OnSlotUpdated?.Invoke(category, word);
    }

    public static void TriggerSlotUpdated()
    {
        OnSlotUpdated?.Invoke(WordCategory.Any, string.Empty);
    }
}