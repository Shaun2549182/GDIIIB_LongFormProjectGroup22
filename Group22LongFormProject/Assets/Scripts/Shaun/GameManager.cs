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
    public string placeWord;

    [Header("Act 1 Outcome (DisplayScene)")]
    [TextArea(2, 4)]
    public string displayStory;

    [Header("Act 2 Settings (Act2GameScene & Act2DisplayScene)")]
    [TextArea(2, 4)] public string act2Template;
    public List<string> act2CharacterWords; // Discovered Object words (1-word)
    public List<string> act2PlaceWords;     // Location Branch words (1-word)

    [Header("Act 3 Settings (Act3GameScene & Act3DisplayScene)")]
    [TextArea(2, 4)] public string act3Template;
    public List<string> act3CharacterWords; // Encounter Figure words (1-word)
    public List<string> act3PlaceWords;     // Final Outcome words (1-word)
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public static event Action<GamePhase> OnPhaseChanged;
    public static event Action<int> OnHealthChanged;

    [Header("Scene Names")]
    [SerializeField] private string mainGameSceneName = "GameScene";
    [SerializeField] private string displaySceneName = "DisplayScene";
    [SerializeField] private string act2GameSceneName = "Act2GameScene";
    [SerializeField] private string act2DisplaySceneName = "Act2DisplayScene";
    [SerializeField] private string act3GameSceneName = "Act3GameScene";
    [SerializeField] private string act3DisplaySceneName = "Act3DisplayScene";
    [SerializeField] private string gameOverSceneName = "GameOverScene";

    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 7;
    public int CurrentHealth { get; private set; }

    [Header("Act 2 Location Brick Scale Modifiers")]
    [SerializeField] private float largeLocationBrickScale = 1.25f; // +25%
    [SerializeField] private float smallLocationBrickScale = 0.75f; // -25%

    public GamePhase CurrentPhase { get; private set; } = GamePhase.Assembly;
    public int CurrentAct { get; private set; } = 1;

    // Selections per Act
    public string SelectedCharacterWord { get; private set; } // Act 1 Character (Liam, Ben, Emma)
    public string SelectedPlaceWord { get; private set; }     // Act 1 Role (Officer, Princess, Baker)

    public string Act2SelectedCharacterWord { get; private set; } // Act 2 Discovered Object
    public string Act2SelectedPlaceWord { get; private set; }     // Act 2 Location Branch

    public string Act3SelectedCharacterWord { get; private set; } // Act 3 Encounter Figure
    public string Act3SelectedPlaceWord { get; private set; }     // Act 3 Final Outcome

    public string GetCurrentActCharacterWord()
    {
        return CurrentAct switch
        {
            2 => Act2SelectedCharacterWord,
            3 => Act3SelectedCharacterWord,
            _ => SelectedCharacterWord
        };
    }

    public string GetCurrentActPlaceWord()
    {
        return CurrentAct switch
        {
            2 => Act2SelectedPlaceWord,
            3 => Act3SelectedPlaceWord,
            _ => SelectedPlaceWord
        };
    }

    private List<NarrativeOutcome> narrativeOutcomes;
    private Dictionary<(string, string), NarrativeOutcome> outcomeDict;

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

        CurrentHealth = maxHealth;
        InitializeNarrativeDatabase();
    }

    private void InitializeNarrativeDatabase()
    {
        outcomeDict = new Dictionary<(string, string), NarrativeOutcome>();
        narrativeOutcomes = new List<NarrativeOutcome>();

        string[] characters = { "Liam", "Ben", "Emma" };

        foreach (string character in characters)
        {
            // 1. OFFICER STORY
            narrativeOutcomes.Add(new NarrativeOutcome
            {
                characterWord = character,
                placeWord = "Officer",
                displayStory = $"In an age when trouble seemed to lurk behind every corner, a sharp-eyed officer named {character} kept watch over the town, ensuring that justice was always served.",
                act2Template = "While patrolling the quiet outskirts of town, {Hero} discovers an abandoned alley leading to a {Place} and spots a {Character}.",
                act2CharacterWords = new List<string> { "Artifact", "Flare", "Key" },
                act2PlaceWords = new List<string> { "Warehouse", "Watchtower", "Dungeon" },
                act3Template = "Suddenly, a {Character} stepped into the light, and by the end of the standoff {Hero} was {Place}.",
                act3CharacterWords = new List<string> { "Thief", "Sniper", "Guard" },
                act3PlaceWords = new List<string> { "Victorious", "Surrounded", "Ambushed", "Trapped" }
            });

            // 2. PRINCESS STORY
            narrativeOutcomes.Add(new NarrativeOutcome
            {
                characterWord = character,
                placeWord = "Princess",
                displayStory = $"Far beyond the bustling town square, tucked within the quiet halls of a velvet-draped palace, lived Princess {character}, whose unconventional presence redefined the royal court.",
                act2Template = "While playing inside the castle, {Hero} discovers a hidden door leading to a {Place} and finds a {Character}.",
                act2CharacterWords = new List<string> { "Necklace", "Candle", "Tube" },
                act2PlaceWords = new List<string> { "Safe", "Bedroom", "Basement" },
                act3Template = "Inside the secret chamber, Princess {Hero} encountered a {Character} and was ultimately {Place}.",
                act3CharacterWords = new List<string> { "Guard", "Scientist", "Goblin" },
                act3PlaceWords = new List<string> { "Arrested", "Captured", "Released", "Saved" }
            });

            // 3. BAKER STORY
            narrativeOutcomes.Add(new NarrativeOutcome
            {
                characterWord = character,
                placeWord = "Baker",
                displayStory = $"Deep in the heart of the kingdom, where the aroma of toasted grain drifted through the cobblestone streets, a gentle baker named {character} coaxed warmth into every morning loaf.",
                act2Template = "While preparing the morning dough, {Hero} discovers a hidden hatch leading to a {Place} and finds a {Character}.",
                act2CharacterWords = new List<string> { "Book", "Jar", "Lantern" },
                act2PlaceWords = new List<string> { "Cellar", "Pantry", "Tunnel" },
                act3Template = "At the end of the passage, {Hero} came face-to-face with a {Character} and was soon {Place}.",
                act3CharacterWords = new List<string> { "Master", "Owl", "Stranger" },
                act3PlaceWords = new List<string> { "Rescued", "Robbed", "Escaped", "Cornered" }
            });
        }

        foreach (var outcome in narrativeOutcomes)
        {
            var key = (outcome.characterWord.ToLower().Trim(), outcome.placeWord.ToLower().Trim());
            if (!outcomeDict.ContainsKey(key))
            {
                outcomeDict.Add(key, outcome);
            }
        }
    }

    public NarrativeOutcome GetCurrentOutcome()
    {
        if (outcomeDict == null) InitializeNarrativeDatabase();

        string charKey = (SelectedCharacterWord ?? "").ToLower().Trim();
        string placeKey = (SelectedPlaceWord ?? "").ToLower().Trim();

        if (outcomeDict.TryGetValue((charKey, placeKey), out NarrativeOutcome outcome))
        {
            return outcome;
        }

        return (narrativeOutcomes != null && narrativeOutcomes.Count > 0) ? narrativeOutcomes[0] : default;
    }

    // --- ACT 2 LOCATION BRICK SCALE MODIFIER ---
    public float GetAct2LocationBrickScaleMultiplier()
    {
        if (CurrentAct < 2 || string.IsNullOrEmpty(Act2SelectedPlaceWord))
        {
            return 1f;
        }

        string locationKey = Act2SelectedPlaceWord.ToLower().Trim();

        return locationKey switch
        {
            // Spacious locations (+25%)
            "warehouse" or "bedroom" or "cellar" => largeLocationBrickScale,

            // Narrow / cramped locations (-25%)
            "watchtower" or "dungeon" or "safe" or "basement" or "pantry" or "tunnel" => smallLocationBrickScale,

            _ => 1f
        };
    }

    // --- HEALTH MANAGEMENT ---
    public bool DeductLife()
    {
        CurrentHealth--;
        OnHealthChanged?.Invoke(CurrentHealth);

        if (CurrentHealth <= 0)
        {
            ChangePhase(GamePhase.GameOver);
            return false;
        }
        return true;
    }

    public void ResetHealth()
    {
        CurrentHealth = maxHealth;
        OnHealthChanged?.Invoke(CurrentHealth);
    }

    // --- WORD SELECTION SETTERS ---
    public void SetSelectedCharacter(string character)
    {
        if (CurrentAct == 1) SelectedCharacterWord = character;
        else if (CurrentAct == 2) Act2SelectedCharacterWord = character;
        else if (CurrentAct == 3) Act3SelectedCharacterWord = character;
    }

    public void SetSelectedPlace(string place)
    {
        if (CurrentAct == 1) SelectedPlaceWord = place;
        else if (CurrentAct == 2) Act2SelectedPlaceWord = place;
        else if (CurrentAct == 3) Act3SelectedPlaceWord = place;
    }

    // --- ARCADE STATE CHECK & COMPLETION ---
    public bool IsArcadeCompleted()
    {
        int characterCount = WordInventory.Instance != null ? WordInventory.Instance.GetWordsByCategory(WordCategory.Character).Count : 0;
        int placeCount = WordInventory.Instance != null ? WordInventory.Instance.GetWordsByCategory(WordCategory.Place).Count : 0;
        return (characterCount >= 1 && placeCount >= 1);
    }

    public void CompleteArcadePhase()
    {
        if (IsArcadeCompleted())
        {
            Debug.Log("[GameManager] Requirements met! Transitioning to Assembly Phase.");
            ChangePhase(GamePhase.Assembly);
        }
        else
        {
            bool isTutorial = SceneManager.GetActiveScene().name.IndexOf("Tutorial", StringComparison.OrdinalIgnoreCase) >= 0;

            if (isTutorial)
            {
                ArcadeRetryUI.Instance?.ShowRetryPrompt("That's not quite right...");
                ArcadeStageController.Instance?.ResetStage();
            }
            else
            {
                if (DeductLife())
                {
                    ArcadeRetryUI.Instance?.ShowRetryPrompt("That's not quite right...");
                    ArcadeStageController.Instance?.ResetStage();
                }
            }
        }
    }

    // --- WORD POOL QUERY HELPERS ---
    public List<string> GetActWordsForCategory(int act, WordCategory category)
    {
        NarrativeOutcome outcome = GetCurrentOutcome();

        if (act == 2)
        {
            return (category == WordCategory.Character)
                ? (outcome.act2CharacterWords ?? new List<string>())
                : (outcome.act2PlaceWords ?? new List<string>());
        }
        else if (act == 3)
        {
            return (category == WordCategory.Character)
                ? (outcome.act3CharacterWords ?? new List<string>())
                : (outcome.act3PlaceWords ?? new List<string>());
        }

        return new List<string>();
    }

    public List<string> GetAct2WordsForCategory(WordCategory category) => GetActWordsForCategory(2, category);
    public List<string> GetAct3WordsForCategory(WordCategory category) => GetActWordsForCategory(3, category);

    // --- PHASE & TRANSITION MANAGEMENT ---
    public void ChangePhase(GamePhase newPhase)
    {
        CurrentPhase = newPhase;
        OnPhaseChanged?.Invoke(newPhase);

        if (newPhase == GamePhase.GameOver)
        {
            SceneManager.LoadScene(gameOverSceneName);
        }
    }

    public void LockInChoices(string characterWord = null, string placeWord = null)
    {
        if (!string.IsNullOrEmpty(characterWord)) SetSelectedCharacter(characterWord);
        if (!string.IsNullOrEmpty(placeWord)) SetSelectedPlace(placeWord);

        ChangePhase(GamePhase.Display);

        string targetScene = displaySceneName;
        if (CurrentAct == 2) targetScene = act2DisplaySceneName;
        else if (CurrentAct == 3) targetScene = act3DisplaySceneName;

        SceneManager.LoadScene(targetScene);
    }

    public void ProceedToAct2()
    {
        CurrentAct = 2;
        ResetActState();
        SceneManager.LoadScene(act2GameSceneName);
    }

    public void ProceedToAct3()
    {
        CurrentAct = 3;
        ResetActState();
        SceneManager.LoadScene(act3GameSceneName);
    }

    private void ResetActState()
    {
        WordInventory.Instance?.ClearInventory();
        ChangePhase(GamePhase.Assembly);
    }

    // --- DISPLAY STORY GENERATORS ---
    public string GetDisplayStory() => GetAct1DisplayStory();

    public string GetAct1DisplayStory() => GetCurrentOutcome().displayStory;

    public string GetAct2SentenceTemplate()
    {
        string hero = string.IsNullOrEmpty(SelectedCharacterWord) ? "the hero" : SelectedCharacterWord;
        return GetCurrentOutcome().act2Template.Replace("{Hero}", hero);
    }

    public string GetAct3SentenceTemplate()
    {
        string hero = string.IsNullOrEmpty(SelectedCharacterWord) ? "the hero" : SelectedCharacterWord;
        return GetCurrentOutcome().act3Template.Replace("{Hero}", hero);
    }

    private string ExpandNarrativeWord(string word)
    {
        if (string.IsNullOrEmpty(word)) return word;

        return word.ToLower().Trim() switch
        {
            // Act 2 Items
            "artifact" => "Stolen Artifact",
            "flare" => "Signal Flare",
            "key" => "Iron Key",
            "tube" => "Glass Tube",
            "book" => "Recipe Book",
            "jar" => "Ingredient Jar",
            "lantern" => "Rusty Lantern",

            // Act 3 Encounters
            "guard" => (SelectedPlaceWord ?? "").ToLower().Trim() == "princess" ? "Palace Guard" : "Rogue Guard",
            "scientist" => "Mad Scientist",
            "goblin" => "Smoke Goblin",
            "master" => "Master Baker",
            "owl" => "Talking Owl",
            "stranger" => "Shadowy Stranger",

            _ => word
        };
    }

    public string GetAct2DisplayStory()
    {
        string hero = string.IsNullOrEmpty(SelectedCharacterWord) ? "Liam" : SelectedCharacterWord;
        string role = (SelectedPlaceWord ?? "").ToLower().Trim();
        string location = (Act2SelectedPlaceWord ?? "").ToLower().Trim();
        string item = string.IsNullOrEmpty(Act2SelectedCharacterWord) ? "mysterious item" : ExpandNarrativeWord(Act2SelectedCharacterWord);

        // BAKER BRANCHES
        if (role == "baker")
        {
            return location switch
            {
                "cellar" => $"In the cellar, {hero} found an old {item} that belonged to the legendary master baker. As {hero} examined it, they accidentally tripped over a flour sack, crashed into the shelf, and saw nothing but a glowing light brightening the dark room—which swallowed {hero} whole into a hall full of bakers!",
                "pantry" => $"In the pantry, {hero} found a mysterious {item} hidden behind the flour bins. As {hero} opened it, a cloud of glowing dust startled a creature that knocked over a heavy shelf and spoke to {hero}, warning that the bakery was in danger!",
                "tunnel" => $"Navigating the damp tunnel, {hero} found a rusty {item} wedged between the stones. Before reaching it, {hero} heard footsteps approaching from the dark, ducked behind a pile of rubble, and picked up a dropped pouch that unlocked a hidden secret door leading deeper underground!",
                _ => $"While preparing the morning dough, {hero} discovered a hidden hatch leading to the {Act2SelectedPlaceWord} and found a {item}."
            };
        }

        // PRINCESS BRANCHES
        if (role == "princess")
        {
            return location switch
            {
                "safe" => $"In the safe, Princess {hero} found a {item} that belonged to the old Queen. As {hero} stepped closer, a strange figure appeared; trying to scream, {hero} tripped and broke the glass next to the pedestal, setting off a loud alarm!",
                "bedroom" => $"The bedroom was very dark, lit only by a mysterious {item} that was not connected to any outlet. Just as Princess {hero} stepped closer, the pedestal started shaking and a secret door opened leading to a room full of treasure!",
                "basement" => $"Princess {hero} walked along a narrow passage until seeing light coming from a door. Opening it revealed a secret room containing a {item}, a strange glowing liquid, and a symbol on the floor that suddenly lit up!",
                _ => $"While playing inside the castle, Princess {hero} discovered a hidden door leading to the {Act2SelectedPlaceWord} and found a {item}."
            };
        }

        // OFFICER BRANCHES
        return location switch
        {
            "warehouse" => $"Inside the dark warehouse, Officer {hero} spotted a {item} hidden under a wooden crate. Suddenly, a strange sound echoed from the corner as an intruder stepped into the light, forcing {hero} to duck behind a stack of dusty cargo boxes and pull a loose circuit breaker!",
            "watchtower" => $"At the top of the watchtower, Officer {hero} spotted a {item} left near the ledge. Suddenly, footsteps echoed from below as an intruder stepped into the light, prompting {hero} to duck into the shadows and sprint toward a narrow maintenance path!",
            "dungeon" => $"Deep within the damp dungeon, Officer {hero} spotted a {item} hidden inside an open cell. Suddenly, a strange sound echoed from the corridor as a figure stepped into the light, leading {hero} to slip past and discover a hidden passage into the sub-levels!",
            _ => $"While patrolling the quiet outskirts of town, Officer {hero} discovered an abandoned alley leading to the {Act2SelectedPlaceWord} and spotted a {item}."
        };
    }

    public string GetAct3DisplayStory()
    {
        string hero = string.IsNullOrEmpty(SelectedCharacterWord) ? "Liam" : SelectedCharacterWord;
        string role = (SelectedPlaceWord ?? "").ToLower().Trim();
        string encounter = string.IsNullOrEmpty(Act3SelectedCharacterWord) ? "mysterious figure" : ExpandNarrativeWord(Act3SelectedCharacterWord);
        string ending = string.IsNullOrEmpty(Act3SelectedPlaceWord) ? "changed forever" : Act3SelectedPlaceWord;
        string endingLower = ending.ToLower();

        // BAKER ENDING
        if (role == "baker")
        {
            return $"Deep within the secret passage, the {encounter} confronted {hero} about the fate of the bakery. With seconds to spare as the ground began to shake, {hero} made a decisive choice—and in the end, {hero} was {endingLower}!";
        }

        // PRINCESS ENDING
        if (role == "princess")
        {
            return $"As the hidden chamber doors slammed shut, Princess {hero} stood face-to-face with the {encounter}. Caught in the middle of the royal mystery, Princess {hero} tried to flee—and was ultimately {endingLower}!";
        }

        // OFFICER ENDING
        return $"Just as Officer {hero} reached for the exit latch, the lights flickered back on and the {encounter} blocked the path. Standing firm until the very last second, Officer {hero} realized the investigation left them {endingLower}!";
    }

    public void RestartGame()
    {
        CurrentAct = 1;
        ResetHealth();

        SelectedCharacterWord = string.Empty;
        SelectedPlaceWord = string.Empty;
        Act2SelectedCharacterWord = string.Empty;
        Act2SelectedPlaceWord = string.Empty;
        Act3SelectedCharacterWord = string.Empty;
        Act3SelectedPlaceWord = string.Empty;

        ResetActState();
        WordVisualManager.Instance?.ClearAllVisuals();
        SceneManager.LoadScene(mainGameSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void StartMainGame()
    {
        CurrentAct = 1;
        SelectedCharacterWord = string.Empty;
        SelectedPlaceWord = string.Empty;

        ResetActState();
        WordVisualManager.Instance?.ClearAllVisuals();
        SceneManager.LoadScene(mainGameSceneName);
    }
}