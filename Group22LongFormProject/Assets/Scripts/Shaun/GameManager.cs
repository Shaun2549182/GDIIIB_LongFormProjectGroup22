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
    public string displayStory; // Story text shown in Act 1 DisplayScene

    [TextArea(2, 4)]
    public string act2Template; // Act 2 template sentence for Assembly Phase

    [TextArea(2, 4)]
    public string act2DisplayStory; // Story text shown in Act 2 DisplayScene

    [Header("Act 2 Word Pools")]
    public List<string> act2CharacterWords; // Object words for Act 2
    public List<string> act2PlaceWords;     // Adverb/Action words for Act 2
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public static event Action<GamePhase> OnPhaseChanged;

    [Header("Scene Names")]
    [SerializeField] private string mainGameSceneName = "GameScene";
    [SerializeField] private string displaySceneName = "DisplayScene";
    [SerializeField] private string act2GameSceneName = "Act2GameScene";
    [SerializeField] private string act2DisplaySceneName = "Act2DisplayScene";
    [SerializeField] private string gameOverSceneName = "GameOverScene";

    [Header("Act 1 Sentence Template")]
    [SerializeField] private string act1AssemblySentenceTemplate = "In the modern day, {Character} is a {Place}.";

    [Header("Narrative Outcomes Database")]
    [SerializeField]
    private NarrativeOutcome defaultOutcome = new NarrativeOutcome
    {
        characterWord = "Default",
        placeWord = "Default",
        displayStory = "The hero set out on an unexpected journey.",
        act2Template = "Later that day, {Character} needed to {Place} find an item.",
        act2DisplayStory = "By searching {Place}, the hero successfully recovered the lost {Character} and saved the day!",
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
            act2DisplayStory = "Searching {Place}, Officer Liam finally retrieved his missing {Character}. Though he knocked over three trash cans in the process, the precinct commended his effort!",
            act2CharacterWords = new List<string> { "Badge", "Radio", "Whistle" },
            act2PlaceWords = new List<string> { "Hastily", "Badly", "Wildly", "Rashly" }
        },
        new NarrativeOutcome
        {
            characterWord = "Liam",
            placeWord = "Baker",
            displayStory = "Liam works as a baker, though his clumsiness often leaves him tripping over flour sacks and covered head-to-toe in dough.",
            act2Template = "Baker Liam needs to {Place} gather ingredients to bake a huge {Character}.",
            act2DisplayStory = "By moving {Place}, Baker Liam gathered every single ingredient and baked a magnificent {Character}! It was slightly lopsided, but delicious.",
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
            act2DisplayStory = "Inspecting {Place}, Officer Ben uncovered a major clue using his trusty {Character}, solving the town mystery before sunset!",
            act2CharacterWords = new List<string> { "Lens", "Folder", "Camera" },
            act2PlaceWords = new List<string> { "Quietly", "Eagerly", "Keenly", "Slyly" }
        },
        new NarrativeOutcome
        {
            characterWord = "Ben",
            placeWord = "Baker",
            displayStory = "Ben works as a baker whose curious mind drives him to constantly experiment with unusual, creative recipe combinations.",
            act2Template = "Baker Ben wants to {Place} mix his baking dough with a mysterious {Character}.",
            act2DisplayStory = "Mixing {Place}, Baker Ben incorporated the mysterious {Character} into his dough, creating a brand new award-winning pastry!",
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
            act2DisplayStory = "Scanning {Place}, Officer Emma successfully located the golden {Character}, receiving a promotion for her methodical work.",
            act2CharacterWords = new List<string> { "Badge", "Report", "Pencil" },
            act2PlaceWords = new List<string> { "Neatly", "Fully", "Alertly", "Calmly" }
        },
        new NarrativeOutcome
        {
            characterWord = "Emma",
            placeWord = "Baker",
            displayStory = "Emma runs her bakery with diligent precision, waking up before dawn every morning to craft perfect, handmade bread.",
            act2Template = "Baker Emma will {Place} prepare her special morning order using her custom {Character}.",
            act2DisplayStory = "Preparing {Place}, Baker Emma utilized her custom {Character} to fulfill the huge morning order with flawless precision.",
            act2CharacterWords = new List<string> { "Whisk", "Timer", "Scale" },
            act2PlaceWords = new List<string> { "Neatly", "Calmly", "Exactly", "Adeptly" }
        }
    };

    private Dictionary<(string, string), NarrativeOutcome> outcomeDict;
    private bool sceneInitializedThisFrame = false;

    public GamePhase CurrentPhase { get; private set; } = GamePhase.Assembly;
    public int CurrentAct { get; private set; } = 1;

    // Act 1 Choices
    public string SelectedCharacterWord { get; private set; }
    public string SelectedPlaceWord { get; private set; }

    // Act 2 Choices
    public string Act2SelectedCharacterWord { get; private set; }
    public string Act2SelectedPlaceWord { get; private set; }

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
        CurrentAct = 1;
        SelectedCharacterWord = string.Empty;
        SelectedPlaceWord = string.Empty;
        Act2SelectedCharacterWord = string.Empty;
        Act2SelectedPlaceWord = string.Empty;

        WordInventory.Instance?.ClearInventory();
        WordVisualManager.Instance?.ClearAllVisuals();

        ChangePhase(GamePhase.Assembly);
    }

    public NarrativeOutcome GetCurrentOutcome()
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

    /// <summary>
    /// Generates the filled-in Act 2 narrative outcome story for Act2DisplayScene.
    /// </summary>
    public string GetAct2DisplayStory()
    {
        NarrativeOutcome outcome = GetCurrentOutcome();
        string storyText = outcome.act2DisplayStory;

        if (string.IsNullOrEmpty(storyText))
        {
            storyText = outcome.act2Template;
        }

        if (string.IsNullOrEmpty(storyText)) return defaultOutcome.displayStory;

        return storyText
            .Replace("{Character}", Act2SelectedCharacterWord)
            .Replace("{Place}", Act2SelectedPlaceWord);
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
    }

    public void CompleteAssemblyPhase()
    {
        Debug.Log($"[GameManager] Assembly Phase complete for Act {CurrentAct}. Transitioning to Display Scene.");
        ChangePhase(GamePhase.Display);

        string targetScene = CurrentAct == 2 ? act2DisplaySceneName : displaySceneName;
        SceneManager.LoadScene(targetScene);
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
        if (CurrentAct == 1)
        {
            SelectedCharacterWord = characterWord;
            SelectedPlaceWord = placeWord;
        }
        else if (CurrentAct == 2)
        {
            Act2SelectedCharacterWord = characterWord;
            Act2SelectedPlaceWord = placeWord;
        }

        ChangePhase(GamePhase.Display);

        string targetScene = CurrentAct == 2 ? act2DisplaySceneName : displaySceneName;
        Debug.Log($"[GameManager] Locking choices for Act {CurrentAct}. Loading scene: {targetScene}");
        SceneManager.LoadScene(targetScene);
    }

    public void ProceedToAct2()
    {
        Debug.Log("[GameManager] Transitioning to Act 2 Game Scene.");

        CurrentAct = 2;

        WordInventory.Instance?.ClearInventory();
        WordVisualManager.Instance?.ClearAllVisuals();

        ChangePhase(GamePhase.Assembly);
        SceneManager.LoadScene(act2GameSceneName);
    }

    public void RestartGame()
    {
        ResetGameState();
        SceneManager.LoadScene(mainGameSceneName);
    }
}