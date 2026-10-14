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
public struct NarrativeOutcome
{
    public string characterWord;
    public string placeWord; // Act 1 Occupation/Place category word (e.g., Officer, Baker)

    [TextArea(2, 4)]
    public string displayStory; // Story text shown in DisplayScene

    [TextArea(2, 4)]
    public string act2Template; // Act 2 template sentence

    [Header("Act 2 Word Pools")]
    public List<string> act2CharacterWords; // 3 Object words for Act 2 (Single words, <= 7 letters)
    public List<string> act2PlaceWords;     // 4 Adverb words for Act 2 (Single words, <= 7 letters)
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public static event Action<GamePhase> OnPhaseChanged;

    [Header("Scene Names")]
    [SerializeField] private string mainGameSceneName = "GameScene";
    [SerializeField] private string displaySceneName = "DisplayScene";
    [SerializeField] private string act2GameSceneName = "Act2GameScene";
    [SerializeField] private string gameOverSceneName = "GameOverScene";

<<<<<<< Updated upstream
    [Header("Sentence Settings")]
    [SerializeField] private string sentenceTemplate = "In the modern day, {Character} is a {Place}. How {CharAdjective} and {PlaceAdjective}!";
=======
    [Header("Act 1 Sentence Template")]
    [SerializeField] private string act1AssemblySentenceTemplate = "In the modern day, {Character} is a {Place}.";
>>>>>>> Stashed changes

    [Header("Narrative Outcomes Database")]
    [SerializeField]
    private NarrativeOutcome defaultOutcome = new NarrativeOutcome
    {
<<<<<<< Updated upstream
        new WordAdjectiveMapping { word = "Emma", adjective = "Cute" },
        new WordAdjectiveMapping { word = "Ben", adjective = "Cool" },
        new WordAdjectiveMapping { word = "Liam", adjective = "Smart" },
        new WordAdjectiveMapping { word = "Princess", adjective = "Beautiful" },
        new WordAdjectiveMapping { word = "Officer", adjective = "Strong" },
        new WordAdjectiveMapping { word = "Baker", adjective = "Fluffy" }
    };

    private Dictionary<string, string> adjectiveDict;
=======
        characterWord = "Default",
        placeWord = "Default",
        displayStory = "The hero set out on an unexpected journey.",
        act2Template = "Later that day, {Character} needed to {Place} find an item.",
        act2CharacterWords = new List<string> { "Key", "Map", "Compass" },
        act2PlaceWords = new List<string> { "Quickly", "Quietly", "Swiftly", "Softly" }
    };

    [SerializeField]
    private List<NarrativeOutcome> narrativeOutcomes = new List<NarrativeOutcome>()
    {
        // --- LIAM (Clumsy) ---
        new NarrativeOutcome
        {
            characterWord = "Liam",
            placeWord = "Officer",
            displayStory = "Liam works as a police officer, though his clumsy nature leads to him constantly spilling coffee on his uniform while on patrol.",
            act2Template = "Officer Liam must {Place} search the police station to recover his lost {Character}.",
            act2CharacterWords = new List<string> { "Badge", "Radio", "Whistle" },
            act2PlaceWords = new List<string> { "Hastily", "Badly", "Wildly", "Rashly" }
        },
        new NarrativeOutcome
        {
            characterWord = "Liam",
            placeWord = "Baker",
            displayStory = "Liam works as a baker, though his clumsiness often leaves him tripping over flour sacks and covered head-to-toe in dough.",
            act2Template = "Baker Liam needs to {Place} gather ingredients to bake a huge {Character}.",
            act2CharacterWords = new List<string> { "Loaf", "Cake", "Tart" },
            act2PlaceWords = new List<string> { "Oddly", "Madly", "Roughly", "Crudely" }
        },

        // --- BEN (Curious) ---
        new NarrativeOutcome
        {
            characterWord = "Ben",
            placeWord = "Officer",
            displayStory = "Ben serves as a curious town officer, always asking extra questions and poking into every strange detail on his beat.",
            act2Template = "Officer Ben decides to {Place} inspect the alley using his trusty {Character}.",
            act2CharacterWords = new List<string> { "Lens", "Folder", "Camera" },
            act2PlaceWords = new List<string> { "Quietly", "Eagerly", "Keenly", "Slyly" }
        },
        new NarrativeOutcome
        {
            characterWord = "Ben",
            placeWord = "Baker",
            displayStory = "Ben works as a baker whose curious mind drives him to constantly experiment with unusual, creative recipe combinations.",
            act2Template = "Baker Ben wants to {Place} mix his baking dough with a mysterious {Character}.",
            act2CharacterWords = new List<string> { "Jar", "Spice", "Recipe" },
            act2PlaceWords = new List<string> { "Boldly", "Keenly", "Wisely", "Anew" }
        },

        // --- EMMA (Diligent) ---
        new NarrativeOutcome
        {
            characterWord = "Emma",
            placeWord = "Officer",
            displayStory = "Emma is a diligent neighborhood officer who double-checks every report and keeps the town safe through untiring dedication.",
            act2Template = "Officer Emma plans to {Place} scan the precinct room to locate the golden {Character}.",
            act2CharacterWords = new List<string> { "Badge", "Report", "Pencil" },
            act2PlaceWords = new List<string> { "Neatly", "Fully", "Alertly", "Calmly" }
        },
        new NarrativeOutcome
        {
            characterWord = "Emma",
            placeWord = "Baker",
            displayStory = "Emma runs her bakery with diligent precision, waking up before dawn every morning to craft perfect, handmade bread.",
            act2Template = "Baker Emma will {Place} prepare her special morning order using her custom {Character}.",
            act2CharacterWords = new List<string> { "Whisk", "Timer", "Scale" },
            act2PlaceWords = new List<string> { "Neatly", "Calmly", "Exactly", "Adeptly" }
        }
    };

    private Dictionary<(string, string), NarrativeOutcome> outcomeDict;
    private bool sceneInitializedThisFrame = false;
>>>>>>> Stashed changes

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

        InitializeOutcomeDictionary();
    }

<<<<<<< Updated upstream
    private void InitializeAdjectiveDictionary()
=======
    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void Start()
    {
        if (!sceneInitializedThisFrame && SceneManager.GetActiveScene().name == mainGameSceneName)
        {
            InitializeAssemblyPhase();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        sceneInitializedThisFrame = false;

        if (scene.name == mainGameSceneName)
        {
            InitializeAssemblyPhase();
        }
    }

    private void InitializeOutcomeDictionary()
    {
        outcomeDict = new Dictionary<(string, string), NarrativeOutcome>();
        foreach (var outcome in narrativeOutcomes)
        {
            var key = (outcome.characterWord.ToLower().Trim(), outcome.placeWord.ToLower().Trim());
            if (!outcomeDict.ContainsKey(key))
            {
                outcomeDict.Add(key, outcome);
            }
        }
    }

    public void InitializeAssemblyPhase()
    {
        if (sceneInitializedThisFrame) return;
        sceneInitializedThisFrame = true;

        ResetGameState();

        SentenceEvents.TriggerSentenceInit(act1AssemblySentenceTemplate);
        Debug.Log("[GameManager] Act 1 Assembly phase initialized with template: " + act1AssemblySentenceTemplate);
    }

    public void ResetGameState()
    {
        SelectedCharacterWord = string.Empty;
        SelectedPlaceWord = string.Empty;

        WordInventory.Instance?.ClearInventory();
        WordVisualManager.Instance?.ClearAllVisuals();

        ChangePhase(GamePhase.Assembly);
    }

    public NarrativeOutcome GetCurrentOutcome()
>>>>>>> Stashed changes
    {
        if (outcomeDict == null) InitializeOutcomeDictionary();

        string charKey = (SelectedCharacterWord ?? "").ToLower().Trim();
        string placeKey = (SelectedPlaceWord ?? "").ToLower().Trim();

        if (outcomeDict.TryGetValue((charKey, placeKey), out NarrativeOutcome outcome))
        {
            return outcome;
        }

        return defaultOutcome;
    }

    public string GetDisplayStory()
    {
        return GetCurrentOutcome().displayStory;
    }

    public string GetAct2SentenceTemplate()
    {
        return GetCurrentOutcome().act2Template;
    }

    public List<string> GetAct2WordsForCategory(WordCategory category)
    {
        NarrativeOutcome outcome = GetCurrentOutcome();

        if (category == WordCategory.Character)
        {
            return outcome.act2CharacterWords ?? new List<string>();
        }
        else if (category == WordCategory.Place)
        {
            return outcome.act2PlaceWords ?? new List<string>();
        }

        return new List<string>();
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

    public void CompleteAssemblyPhase()
    {
        Debug.Log("[GameManager] Assembly Phase complete. Transitioning to DisplayScene.");
        ChangePhase(GamePhase.Display);
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

    public void ProceedToAct2()
    {
        Debug.Log("[GameManager] Transitioning to Act 2 Game Scene.");

        // Clear Act 1 inventory while preserving SelectedCharacterWord and SelectedPlaceWord
        WordInventory.Instance?.ClearInventory();
        WordVisualManager.Instance?.ClearAllVisuals();

<<<<<<< Updated upstream
        string formatted = sentenceTemplate;
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
=======
        ChangePhase(GamePhase.Assembly);
        SceneManager.LoadScene(act2GameSceneName);
>>>>>>> Stashed changes
    }

    public void RestartGame()
    {
        SelectedCharacterWord = string.Empty;
        SelectedPlaceWord = string.Empty;
        CurrentPhase = GamePhase.Assembly;
        SceneManager.LoadScene(mainGameSceneName);
    }
}