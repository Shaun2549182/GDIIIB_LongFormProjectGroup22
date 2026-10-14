using UnityEngine;

public static class ArcadeWordTracker
{
    public static void CheckRemainingWords()
    {
        // Only evaluate during the active Arcade phase
        if (GameManager.Instance == null || GameManager.Instance.CurrentPhase != GamePhase.Arcade)
        {
            return;
        }

        // Count remaining active Word Bricks
        int wordBrickCount = 0;
        Brick[] bricks = Object.FindObjectsByType<Brick>(FindObjectsSortMode.None);
        foreach (var b in bricks)
        {
            if (b != null && b.gameObject.activeInHierarchy && b.GetBrickType() == Brick.BrickType.Word)
            {
                wordBrickCount++;
            }
        }

        // Count remaining active Falling Word Items in play
        int fallingWordCount = 0;
        FallingWordItem[] items = Object.FindObjectsByType<FallingWordItem>(FindObjectsSortMode.None);
        foreach (var item in items)
        {
            if (item != null && item.gameObject.activeInHierarchy && !item.IsPendingDestruction)
            {
                fallingWordCount++;
            }
        }

        // If all word bricks are destroyed and all falling word pickups are cleared
        if (wordBrickCount == 0 && fallingWordCount == 0)
        {
            Debug.Log("[ArcadeWordTracker] All words cleared! Transitioning out of Arcade Phase.");
            GameManager.Instance.CompleteArcadePhase();
        }
    }
}