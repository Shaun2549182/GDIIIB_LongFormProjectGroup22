using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GamePhase
{
    Assembly,
    Arcade,
    GameOver,
    Display
}

[System.Serializable]
public struct WordAdjectiveMapping
{
    public string word;
    public string adjective;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public static event Action<GamePhase> OnPhaseChanged;

    [Header("Scene Names")]
    [SerializeField] private string mainGameSceneName = "GameScene";
    [SerializeField] private string gameOverSceneName = "GameOverScene";
    [SerializeField] private string displaySceneName = "DisplayScene";

    [Header("Sentence Templates")]
    [SerializeField] private string assemblySentenceTemplate = "In the modern day, {Character} is a {Place}.";
    [SerializeField] private string displaySentenceTemplate = "In the modern day, {Character} is a {Place}. How {CharAdjective} and {PlaceAdjective}!";

    [Header("Word Adjective Mappings")]
    [SerializeField]
    private List<WordAdjectiveMapping> wordAdjectiveMappings = new List<WordAdjectiveMapping>()
    {
        new WordAdjectiveMapping { word = "Emma", adjective = "Cute" },
        new WordAdjectiveMapping { word = "Ben", adjective = "Cool" },
        new WordAdjectiveMapping { word = "Liam", adjective = "Smart" },
        new WordAdjectiveMapping { word = "Officer", adjective = "Strong" },
        new WordAdjectiveMapping { word = "Baker", adjective = "Fluffy" }
    };

    private Dictionary<string, string> adjectiveDict;
    private bool isInitializing = false;

    public GamePhase CurrentPhase { get; private set; } = GamePhase.Assembly;
    public string SelectedCharacterWord { get; private set; }
    public string SelectedPlaceWord { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeAdjectiveDictionary();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // First-time Editor boot check (sceneLoaded doesn't always fire on initial Editor Play)
        if (SceneManager.GetActiveScene().name == mainGameSceneName && CurrentPhase == GamePhase.Assembly)
        {
            InitializeAssemblyPhase();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == mainGameSceneName)
        {
            InitializeAssemblyPhase();
        }
    }

    private void InitializeAssemblyPhase()
    {
        if (isInitializing) return;
        isInitializing = true;

        SelectedCharacterWord = string.Empty;
        SelectedPlaceWord = string.Empty;

        WordInventory.Instance?.ClearInventory();
        WordVisualManager.Instance?.ClearAllVisuals();

        ChangePhase(GamePhase.Assembly);

        // Uses the clean assembly template WITHOUT the adjective tags
        SentenceEvents.TriggerSentenceInit(assemblySentenceTemplate);

        isInitializing = false;
    }

    private void InitializeAdjectiveDictionary()
    {
        adjectiveDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var mapping in wordAdjectiveMappings)
        {
            if (!string.IsNullOrEmpty(mapping.word))
            {
                adjectiveDict[mapping.word] = mapping.adjective;
            }
        }
    }

    public void ChangePhase(GamePhase newPhase)
    {
        CurrentPhase = newPhase;
        OnPhaseChanged?.Invoke(newPhase);
        Debug.Log($"[GameManager] Game Phase switched to: {newPhase}");

        if (newPhase == GamePhase.GameOver)
        {
            SceneManager.LoadScene(gameOverSceneName);
        }
        else if (newPhase == GamePhase.Display)
        {
            SceneManager.LoadScene(displaySceneName);
        }
    }

    public void CompleteArcadePhase()
    {
        int characterCount = WordInventory.Instance != null ? WordInventory.Instance.GetWordsByCategory(WordCategory.Character).Count : 0;
        int placeCount = WordInventory.Instance != null ? WordInventory.Instance.GetWordsByCategory(WordCategory.Place).Count : 0;

        if (characterCount >= 1 && placeCount >= 1)
        {
            Debug.Log("[GameManager] Requirements met! Transitioning to Assembly phase.");
            ChangePhase(GamePhase.Assembly);
        }
        else
        {
            Debug.Log($"[GameManager] Requirements failed (Characters: {characterCount}, Places: {placeCount}). Game Over!");
            ChangePhase(GamePhase.GameOver);
        }
    }

    public void LockInChoices(string characterWord, string placeWord)
    {
        SelectedCharacterWord = characterWord;
        SelectedPlaceWord = placeWord;
        ChangePhase(GamePhase.Display);
    }

    public string GetFormattedSentence()
    {
        string charWord = string.IsNullOrEmpty(SelectedCharacterWord) ? "___" : SelectedCharacterWord;
        string placeWord = string.IsNullOrEmpty(SelectedPlaceWord) ? "___" : SelectedPlaceWord;

        string charAdj = GetAdjectiveForWord(charWord, "unique");
        string placeAdj = GetAdjectiveForWord(placeWord, "special");

        string formatted = displaySentenceTemplate;
        formatted = formatted.Replace("{Character}", charWord);
        formatted = formatted.Replace("{Place}", placeWord);
        formatted = formatted.Replace("{CharAdjective}", charAdj);
        formatted = formatted.Replace("{PlaceAdjective}", placeAdj);

        return formatted;
    }

    private string GetAdjectiveForWord(string word, string fallback)
    {
        if (adjectiveDict == null) InitializeAdjectiveDictionary();

        if (adjectiveDict.TryGetValue(word, out string adj))
        {
            return adj;
        }
        return fallback;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(mainGameSceneName);
    }
}