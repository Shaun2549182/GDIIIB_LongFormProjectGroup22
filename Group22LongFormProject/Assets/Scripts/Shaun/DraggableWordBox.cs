using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(RectTransform))]
public class DraggableWordBox : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI & Display")]
    [SerializeField] private TextMeshProUGUI wordLabel;
    [SerializeField] private Image boxImage;

    [Header("Category Colors")]
    [SerializeField] private Color characterBoxColor = new Color(0.3f, 0.5f, 0.9f, 1f);
    [SerializeField] private Color placeBoxColor = new Color(0.3f, 0.9f, 0.5f, 1f);

    [Header("Physics Settings")]
    [SerializeField] private Rigidbody2D rb2d;

    [Header("Off-Screen Recovery Guard")]
    [SerializeField] private float offScreenYThreshold = -600f; // Local UI Y threshold

    public string WordText { get; private set; }
    public WordCategory Category { get; private set; }
    public SentenceSlotUI CurrentSlot { get; private set; }

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Transform originalParent;
    private Canvas parentCanvas;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        if (rb2d == null) rb2d = GetComponent<Rigidbody2D>();
        if (boxImage == null) boxImage = GetComponent<Image>();
        parentCanvas = GetComponentInParent<Canvas>();
    }

    private void Update()
    {
        // Safety Guard: If physics accidentally clips the box through a collider or it falls off-screen
        if (CurrentSlot == null && transform.localPosition.y < offScreenYThreshold)
        {
            ReturnToCollectionBox();
        }
    }

    public void Initialize(string text, WordCategory category)
    {
        WordText = text;
        Category = category;

        if (wordLabel != null) wordLabel.text = text;

        if (boxImage != null)
        {
            boxImage.color = (category == WordCategory.Character) ? characterBoxColor : placeBoxColor;
        }

        // Cache initial container parent
        if (originalParent == null) originalParent = transform.parent;
    }

    public void SetPhysicsEnabled(bool enabled)
    {
        if (rb2d != null)
        {
            rb2d.simulated = enabled;
            rb2d.bodyType = enabled ? RigidbodyType2D.Dynamic : RigidbodyType2D.Kinematic;
            if (!enabled) rb2d.linearVelocity = Vector2.zero;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (originalParent == null) originalParent = transform.parent;

        // Unsnap from current slot if dragging away
        if (CurrentSlot != null)
        {
            CurrentSlot.ClearSlot();
            CurrentSlot = null;
        }

        SetPhysicsEnabled(false);

        // Re-parent to root Canvas overlay during drag
        transform.SetParent(parentCanvas.transform, true);

        canvasGroup.alpha = 0.75f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (parentCanvas == null) return;
        rectTransform.anchoredPosition += eventData.delta / parentCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // If not dropped onto a valid slot, return to collection area
        if (CurrentSlot == null)
        {
            ReturnToCollectionBox();
        }
    }

    public void SnapToSlot(SentenceSlotUI targetSlot, Vector3 snapPosition)
    {
        CurrentSlot = targetSlot;
        transform.SetParent(targetSlot.transform, true);
        transform.position = snapPosition;

        SetPhysicsEnabled(false);
    }

    public void ReturnToCollectionBox()
    {
        CurrentSlot = null;

        if (originalParent != null)
        {
            // Re-parent back to collection box container
            transform.SetParent(originalParent, false);

            // Reset scale and place directly at local Y = 0
            transform.localScale = Vector3.one;
            Vector3 randomOffset = new Vector3(Random.Range(-30f, 30f), 0f, 0f);
            transform.localPosition = randomOffset;
        }

        SetPhysicsEnabled(true);
    }
}