using UnityEngine;
using TMPro;

public class FallingWordItem : MonoBehaviour
{
    [SerializeField] private float fallSpeed = 3f;
    [SerializeField] private float destroyYThreshold = -6f; // Off-screen bottom limit
    [SerializeField] private TextMeshPro labelText3D;
    [SerializeField] private TextMeshProUGUI labelTextUI;

    private string wordText;
    private WordCategory wordCategory;

    public void Initialize(string text, WordCategory category)
    {
        wordText = text;
        wordCategory = category;

        // Auto-find text component if left unassigned in the Inspector
        if (labelText3D == null) labelText3D = GetComponentInChildren<TextMeshPro>();
        if (labelTextUI == null) labelTextUI = GetComponentInChildren<TextMeshProUGUI>();

        if (labelText3D != null) labelText3D.text = wordText;
        if (labelTextUI != null) labelTextUI.text = wordText;
    }

    private void Update()
    {
        // Move slowly downward
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);

        // Destroy if missed by paddle and falls below screen
        if (transform.position.y < destroyYThreshold)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Paddle"))
        {
            WordInventory.Instance?.AddWord(wordText, wordCategory);
            Debug.Log($"[Caught Word]: {wordText} ({wordCategory})");
            Destroy(gameObject);
        }
    }
}