using UnityEngine;

public class WordCollectionBox : MonoBehaviour
{
    public static WordCollectionBox Instance { get; private set; }

    [Header("Spawning Settings")]
    [SerializeField] private GameObject draggableWordBoxPrefab;
    [SerializeField] private Transform spawnAnchor;

    [Header("Spawn Position Variance")]
    [SerializeField] private float horizontalVariance = 30f; // Width scatter range in local pixels
    [SerializeField] private float verticalDropOffset = 25f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        WordInventory.OnWordCollected += SpawnCollectedWordBox;
        WordInventory.OnInventoryCleared += ClearCollectionBox;
    }

    private void OnDisable()
    {
        WordInventory.OnWordCollected -= SpawnCollectedWordBox;
        WordInventory.OnInventoryCleared -= ClearCollectionBox;
    }

    private void SpawnCollectedWordBox(string wordText, WordCategory category)
    {
        if (draggableWordBoxPrefab == null) return;

        Transform targetContainer = (spawnAnchor != null) ? spawnAnchor : transform;
        GameObject newBox = Instantiate(draggableWordBoxPrefab, targetContainer, false);

        newBox.transform.localScale = Vector3.one;

        // Horizontal scatter only; Y is set to 0 to align with the container origin
        float randomX = Random.Range(-horizontalVariance, horizontalVariance);
        newBox.transform.localPosition = new Vector3(randomX, 0f, 0f);

        if (newBox.TryGetComponent<DraggableWordBox>(out var wordBoxScript))
        {
            wordBoxScript.Initialize(wordText, category);
            wordBoxScript.SetPhysicsEnabled(true);
        }
    }

    public void ClearCollectionBox()
    {
        DraggableWordBox[] spawnedBoxes = GetComponentsInChildren<DraggableWordBox>();
        foreach (var box in spawnedBoxes)
        {
            if (box != null)
            {
                Destroy(box.gameObject);
            }
        }
    }
}