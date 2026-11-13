using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SentenceSlotUI : MonoBehaviour, IDropHandler
{
    [Header("Slot Category Configuration")]
    [SerializeField] private WordCategory slotCategory = WordCategory.Any;

    [Header("Color Coding Settings")]
    [SerializeField] private Image slotBackgroundImage;
    [SerializeField] private Color characterColor = new Color(0.2f, 0.4f, 0.8f, 0.6f);
    [SerializeField] private Color placeColor = new Color(0.2f, 0.8f, 0.4f, 0.6f);
    [SerializeField] private Color defaultColor = new Color(0.5f, 0.5f, 0.5f, 0.4f);

    [Header("Snap Position")]
    [SerializeField] private Transform snapAnchor;

    public DraggableWordBox CurrentSnappedBox { get; private set; }
    public WordCategory GetSlotCategory() => slotCategory;

    private void Awake()
    {
        if (slotBackgroundImage == null) slotBackgroundImage = GetComponent<Image>();
        if (snapAnchor == null) snapAnchor = transform;
    }

    public void InitializeSlot(WordCategory category)
    {
        slotCategory = category;
        ApplyCategoryColor();
    }

    private void ApplyCategoryColor()
    {
        if (slotBackgroundImage == null) return;

        switch (slotCategory)
        {
            case WordCategory.Character:
                slotBackgroundImage.color = characterColor;
                break;
            case WordCategory.Place:
                slotBackgroundImage.color = placeColor;
                break;
            default:
                slotBackgroundImage.color = defaultColor;
                break;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject draggedObject = eventData.pointerDrag;
        if (draggedObject == null) return;

        if (draggedObject.TryGetComponent<DraggableWordBox>(out var wordBox))
        {
            if (wordBox.Category == slotCategory || slotCategory == WordCategory.Any)
            {
                if (CurrentSnappedBox != null && CurrentSnappedBox != wordBox)
                {
                    CurrentSnappedBox.ReturnToCollectionBox();
                }

                wordBox.SnapToSlot(this, snapAnchor.position);
                CurrentSnappedBox = wordBox;

                // Update scene visual anchor
                WordVisualManager.Instance?.UpdateCategoryVisual(slotCategory, wordBox.WordText);

                // Update GameManager & Ball state
                if (slotCategory == WordCategory.Character)
                {
                    GameManager.Instance?.SetSelectedCharacter(wordBox.WordText);

                    // Trigger ball color change & particle feedback burst
                    BallController ball = FindFirstObjectByType<BallController>();
                    ball?.TriggerCharacterChangeFeedback();
                }
                else if (slotCategory == WordCategory.Place)
                {
                    GameManager.Instance?.SetSelectedPlace(wordBox.WordText);
                }

                SentenceEvents.TriggerSlotUpdated(slotCategory, wordBox.WordText);
            }
            else
            {
                wordBox.ReturnToCollectionBox();
            }
        }
    }

    public void ClearSlot()
    {
        if (slotCategory == WordCategory.Character)
        {
            GameManager.Instance?.SetSelectedCharacter(string.Empty);

            // Reset ball back to default color
            BallController ball = FindFirstObjectByType<BallController>();
            ball?.ResetBall();
        }
        else if (slotCategory == WordCategory.Place)
        {
            GameManager.Instance?.SetSelectedPlace(string.Empty);
        }

        CurrentSnappedBox = null;
        WordVisualManager.Instance?.ClearCategoryVisual(slotCategory);

        SentenceEvents.TriggerSlotUpdated(slotCategory, string.Empty);
    }

    public string GetSelectedWord()
    {
        return CurrentSnappedBox != null ? CurrentSnappedBox.WordText : string.Empty;
    }
}