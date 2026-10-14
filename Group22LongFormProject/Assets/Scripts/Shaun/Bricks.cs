using UnityEngine;
using TMPro;

public class Brick : MonoBehaviour
{
    public enum BrickType { Standard, Word }

    [Header("Brick Configuration")]
    [SerializeField] private BrickType brickType = BrickType.Standard;
    [SerializeField] private int hitPoints = 1;

    [Header("Word Data (Word Bricks Only)")]
    [SerializeField] private string wordText;
    [SerializeField] private WordCategory wordCategory;
    [SerializeField] private TextMeshPro labelText;
    [SerializeField] private GameObject fallingWordPrefab;

    public BrickType GetBrickType() => brickType;
    public WordCategory GetCategory() => wordCategory;
    public string GetWordText() => wordText;

    private void OnValidate()
    {
        UpdateLabel();
    }

    /// <summary>
    /// Configures the brick as a word brick and updates its label at runtime.
    /// </summary>
    public void SetWord(string word, WordCategory category)
    {
        brickType = BrickType.Word;
        wordText = word;
        wordCategory = category;
        UpdateLabel();
        gameObject.SetActive(true);
    }

    private void UpdateLabel()
    {
        if (labelText != null)
        {
            labelText.text = (brickType == BrickType.Word) ? wordText : string.Empty;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            TakeDamage(1);
        }
    }

    public void TakeDamage(int damage)
    {
        hitPoints -= damage;

        if (hitPoints <= 0)
        {
            OnDestroyed();
        }
    }

    private void OnDestroyed()
    {
        if (brickType == BrickType.Word && fallingWordPrefab != null && !string.IsNullOrEmpty(wordText))
        {
            GameObject pickupObj = Instantiate(fallingWordPrefab, transform.position, Quaternion.identity);
            FallingWordItem pickupScript = pickupObj.GetComponent<FallingWordItem>();

            if (pickupScript != null)
            {
                pickupScript.Initialize(wordText, wordCategory);
            }
        }

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        ArcadeWordTracker.CheckRemainingWords();
    }
}