using UnityEngine;
using TMPro;

public class FallingWordItem : MonoBehaviour
{
    [SerializeField] private float fallSpeed = 3f;
    [SerializeField] private float destroyYThreshold = -6f;
    [SerializeField] private TextMeshPro labelText3D;
    [SerializeField] private TextMeshProUGUI labelTextUI;

    private string wordText;
    private WordCategory wordCategory;

    public bool IsPendingDestruction { get; private set; } = false;

    public void Initialize(string text, WordCategory category)
    {
        wordText = text;
        wordCategory = category;

        if (labelText3D == null) labelText3D = GetComponentInChildren<TextMeshPro>();
        if (labelTextUI == null) labelTextUI = GetComponentInChildren<TextMeshProUGUI>();

        if (labelText3D != null) labelText3D.text = wordText;
        if (labelTextUI != null) labelTextUI.text = wordText;
    }

    private void Update()
    {
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);

        if (transform.position.y < destroyYThreshold)
        {
            CollectOrDestroy(collected: false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Paddle"))
        {
            CollectOrDestroy(collected: true);
        }
    }

    public void CollectOrDestroy(bool collected)
    {
        if (IsPendingDestruction) return;
        IsPendingDestruction = true;

        if (collected)
        {
            WordInventory.Instance?.AddWord(wordText, wordCategory);
            Debug.Log($"[Caught Word]: {wordText} ({wordCategory})");
        }

        Destroy(gameObject);

        // Check if this was the last remaining word item
        ArcadeWordTracker.CheckRemainingWords();
    }
}