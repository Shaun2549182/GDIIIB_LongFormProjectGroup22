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
    public List<string> act2CharacterWords; // 3 Object words
    public List<string> act2PlaceWords;     // 4 Adverb words

    [Header("Act 3 Settings (Act3GameScene & Act3DisplayScene)")]
    [TextArea(2, 4)] public string act3Template; // Base template using {Character} and {Place}
    public List<string> act3CharacterWords; // 3 Secondary Character words
    public List<string> act3PlaceWords;     // 4 Weather words
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

    public GamePhase CurrentPhase { get; private set; } = GamePhase.Assembly;
    public int CurrentAct { get; private set; } = 1;

    // Selections per Act
    public string SelectedCharacterWord { get; private set; } // Act 1 Character
    public string SelectedPlaceWord { get; private set; }     // Act 1 Occupation

    public string Act2SelectedCharacterWord { get; private set; } // Act 2 Object
    public string Act2SelectedPlaceWord { get; private set; }     // Act 2 Adverb

    public string Act3SelectedCharacterWord { get; private set; } // Act 3 Secondary Character
    public string Act3SelectedPlaceWord { get; private set; }     // Act 3 Weather

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

    // Act 1 & Base Outcome Setup
    private List<NarrativeOutcome> narrativeOutcomes;
    private Dictionary<(string, string), NarrativeOutcome> outcomeDict;

    // Act 2: 72 Unique Handwritten Stories Dictionary
    private Dictionary<(string, string, string, string), string> act2UniqueStories;

    // Act 3: 3 Randomized Suffixes per Branch
    private readonly string[] generalAct3Additions = new string[]
    {
        "And thus, a legendary new story was born that the town would recount for years to come!",
        "Against all odds, teamwork saved the day and brought a warm smile to everyone.",
        "It turned out to be a remarkable event that no one would ever forget."
    };

    private readonly Dictionary<(string, string), string[]> branchAct3Additions = new Dictionary<(string, string), string[]>()
    {
        { ("liam", "officer"), new string[]
            {
                "Even with coffee spilled on his uniform, Officer Liam proved himself a true hero!",
                "The precinct chief immediately awarded them both a shiny badge of honor.",
                "Despite the chaotic mess, the case was officially closed by sunset!"
            }
        },
        { ("liam", "baker"), new string[]
            {
                "Flour was everywhere, but the fresh aroma filled the entire neighborhood!",
                "The bakery line wrapped around the block twice to taste the masterpiece.",
                "The mayor declared it an official town holiday in honor of the sweet success!"
            }
        },
        { ("ben", "officer"), new string[]
            {
                "Adjusting his sunglasses indoors, Officer Ben knew another mystery was solved.",
                "The evidence was filed neatly into precinct history by the end of the shift.",
                "Quietly walking back to patrol, Officer Ben tipped his hat to a job well done."
            }
        },
        { ("ben", "baker"), new string[]
            {
                "The secret recipe was preserved, guaranteeing culinary greatness for years.",
                "Food critics across the country gave the creation a glowing five-star review!",
                "The oven warmth faded, leaving behind the crisp aroma of victory."
            }
        },
        { ("emma", "officer"), new string[]
            {
                "Standing tall with a warm smile, Officer Emma kept the peace once again.",
                "The precinct cheered as the dispatch radio finally fell quiet and calm.",
                "Citizens gathered in the square to applaud the swift and safe resolution."
            }
        },
        { ("emma", "baker"), new string[]
            {
                "By dawn, every basket was full and every customer left with a happy heart.",
                "The cozy kitchen sparkled as the morning light streamed through the glass.",
                "It was undeniably the finest batch of baked goods the town had ever seen."
            }
        }
    };

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
        act2UniqueStories = new Dictionary<(string, string, string, string), string>();

        InitializeNarrativeOutcomes();
        BuildAct2UniqueStories();
    }

    private void InitializeNarrativeOutcomes()
    {
        narrativeOutcomes = new List<NarrativeOutcome>()
        {
            new NarrativeOutcome
            {
                characterWord = "Liam", placeWord = "Officer",
                displayStory = "Liam works as a police officer, though he is clumsy and tends to spill coffee on himself during patrol.",
                act2Template = "Officer Liam must {Place} search the station for his lost {Character}.",
                act2CharacterWords = new List<string> { "Badge", "Radio", "Whistle" },
                act2PlaceWords = new List<string> { "Hastily", "Badly", "Wildly", "Rashly" },
                act3Template = "Braving the {Place}, Officer Liam teamed up with a {Character}.",
                act3CharacterWords = new List<string> { "Rookie", "Stray", "Witness" },
                act3PlaceWords = new List<string> { "Storm", "Fog", "Rain", "Gale" }
            },
            new NarrativeOutcome
            {
                characterWord = "Liam", placeWord = "Baker",
                displayStory = "Liam works as a baker, though his clumsiness often leaves him covered head-to-toe in flour.",
                act2Template = "Baker Liam needs to {Place} gather ingredients for a huge {Character}.",
                act2CharacterWords = new List<string> { "Loaf", "Cake", "Tart" },
                act2PlaceWords = new List<string> { "Oddly", "Madly", "Roughly", "Crudely" },
                act3Template = "Amidst the kitchen {Place}, Baker Liam received help from a {Character}.",
                act3CharacterWords = new List<string> { "Intern", "Pigeon", "Critter" },
                act3PlaceWords = new List<string> { "Heat", "Steam", "Smoke", "Haze" }
            },
            new NarrativeOutcome
            {
                characterWord = "Ben", placeWord = "Officer",
                displayStory = "Ben serves as the town officer, taking his duties seriously while wearing cool sunglasses indoors.",
                act2Template = "Officer Ben decides to {Place} inspect the alley using his {Character}.",
                act2CharacterWords = new List<string> { "Lens", "Folder", "Camera" },
                act2PlaceWords = new List<string> { "Quietly", "Eagerly", "Keenly", "Slyly" },
                act3Template = "Under the heavy {Place}, Officer Ben investigated the scene with a {Character}.",
                act3CharacterWords = new List<string> { "Sleuth", "Partner", "Spy" },
                act3PlaceWords = new List<string> { "Gloom", "Frost", "Chill", "Shadow" }
            },
            new NarrativeOutcome
            {
                characterWord = "Ben", placeWord = "Baker",
                displayStory = "Ben works as a master baker, famous for crafting amazingly creative pastry sculptures.",
                act2Template = "Baker Ben wants to {Place} mix his dough with a mystery {Character}.",
                act2CharacterWords = new List<string> { "Jar", "Spice", "Recipe" },
                act2PlaceWords = new List<string> { "Boldly", "Keenly", "Wisely", "Anew" },
                act3Template = "Swept up in a sudden {Place}, Baker Ben teamed up with a local {Character}.",
                act3CharacterWords = new List<string> { "Foodie", "Rival", "Critic" },
                act3PlaceWords = new List<string> { "Breeze", "Draft", "Warmth", "Mist" }
            },
            new NarrativeOutcome
            {
                characterWord = "Emma", placeWord = "Officer",
                displayStory = "Emma is a quick-thinking neighborhood officer who keeps the town safe with a friendly smile.",
                act2Template = "Officer Emma plans to {Place} scan the precinct for the golden {Character}.",
                act2CharacterWords = new List<string> { "Badge", "Report", "Pencil" },
                act2PlaceWords = new List<string> { "Neatly", "Fully", "Alertly", "Calmly" },
                act3Template = "As the {Place} set over the city, Officer Emma coordinated with the {Character}.",
                act3CharacterWords = new List<string> { "Captain", "Deputy", "Escort" },
                act3PlaceWords = new List<string> { "Clear", "Dusk", "Dawn", "Sun" }
            },
            new NarrativeOutcome
            {
                characterWord = "Emma", placeWord = "Baker",
                displayStory = "Emma runs a cozy bakery, waking up at dawn to bake sweet, handmade bread for the locals.",
                act2Template = "Baker Emma will {Place} prepare her special order using her {Character}.",
                act2CharacterWords = new List<string> { "Whisk", "Timer", "Scale" },
                act2PlaceWords = new List<string> { "Neatly", "Calmly", "Exactly", "Adeptly" },
                act3Template = "Surrounded by early {Place}, Baker Emma worked alongside a reliable {Character}.",
                act3CharacterWords = new List<string> { "Patron", "Courier", "Vendor" },
                act3PlaceWords = new List<string> { "Calm", "Frost", "Warmth", "Light" }
            }
        };

        foreach (var outcome in narrativeOutcomes)
        {
            var key = (outcome.characterWord.ToLower().Trim(), outcome.placeWord.ToLower().Trim());
            if (!outcomeDict.ContainsKey(key)) outcomeDict.Add(key, outcome);
        }
    }

    public void AddAct2Story(string charWord, string occWord, string objWord, string advWord, string storyText)
    {
        var key = (charWord.ToLower().Trim(), occWord.ToLower().Trim(), objWord.ToLower().Trim(), advWord.ToLower().Trim());
        act2UniqueStories[key] = storyText;
    }

    private void BuildAct2UniqueStories()
    {
        // 1. LIAM + OFFICER
        AddAct2Story("Liam", "Officer", "Badge", "Hastily", "Officer Liam hastily turned the station upside down, eventually finding his missing badge inside the breakroom microwave!");
        AddAct2Story("Liam", "Officer", "Badge", "Badly", "Officer Liam badly misplaced his badge, accidentally pinning it onto a stuffed teddy bear in the chief's office.");
        AddAct2Story("Liam", "Officer", "Badge", "Wildly", "Officer Liam wildly brandished his lost badge after discovering it tangled in the station window blinds.");
        AddAct2Story("Liam", "Officer", "Badge", "Rashly", "Officer Liam rashly pulled out his badge to identify himself to a potted fern in the precinct lobby.");
        AddAct2Story("Liam", "Officer", "Radio", "Hastily", "Officer Liam hastily grabbed his radio, unaware it was broadcasting loud static directly to the captain's headset.");
        AddAct2Story("Liam", "Officer", "Radio", "Badly", "Officer Liam badly damaged his radio when he dropped it into his morning mug of dark roast coffee.");
        AddAct2Story("Liam", "Officer", "Radio", "Wildly", "Officer Liam wildly waved his radio in the air, trying to catch a signal from across the station.");
        AddAct2Story("Liam", "Officer", "Radio", "Rashly", "Officer Liam rashly pressed the emergency channel button on his radio just to ask if anyone had seen his sandwich.");
        AddAct2Story("Liam", "Officer", "Whistle", "Hastily", "Officer Liam hastily blew his whistle inside the bullpen, startling every detective at their desk.");
        AddAct2Story("Liam", "Officer", "Whistle", "Badly", "Officer Liam badly misjudged his pitch, squeaking his whistle so hard that the precinct cat dashed under a rug.");
        AddAct2Story("Liam", "Officer", "Whistle", "Wildly", "Officer Liam wildly blew his whistle to direct hallway foot traffic during shift change.");
        AddAct2Story("Liam", "Officer", "Whistle", "Rashly", "Officer Liam rashly traded his lunch for a shiny brass whistle from the lost-and-found bin.");

        // 2. LIAM + BAKER
        AddAct2Story("Liam", "Baker", "Loaf", "Oddly", "Baker Liam oddly shaped the sourdough loaf so that it looked like a sleeping kitten.");
        AddAct2Story("Liam", "Baker", "Loaf", "Madly", "Baker Liam madly kneaded the giant bread loaf until flour covered every wall in the kitchen.");
        AddAct2Story("Liam", "Baker", "Loaf", "Roughly", "Baker Liam roughly tossed the finished loaf onto the cooling rack, splitting the top crust in two.");
        AddAct2Story("Liam", "Baker", "Loaf", "Crudely", "Baker Liam crudely carved initials into the top of the loaf before sliding it into the wood-fired oven.");
        AddAct2Story("Liam", "Baker", "Cake", "Oddly", "Baker Liam oddly decorated the anniversary cake with pickle slices instead of candied cherries.");
        AddAct2Story("Liam", "Baker", "Cake", "Madly", "Baker Liam madly stacked five layers of sponge cake without checking if the base layer was fully cooled.");
        AddAct2Story("Liam", "Baker", "Cake", "Roughly", "Baker Liam roughly spread the chocolate frosting, creating a rustic masterpiece by complete accident.");
        AddAct2Story("Liam", "Baker", "Cake", "Crudely", "Baker Liam crudely drew a smiley face on the birthday cake using leftover strawberry jam.");
        AddAct2Story("Liam", "Baker", "Tart", "Oddly", "Baker Liam oddly seasoned the lemon tart with a dash of black pepper and sea salt.");
        AddAct2Story("Liam", "Baker", "Tart", "Madly", "Baker Liam madly whipped the cream for the tart until the bowl overflowed onto his shoes.");
        AddAct2Story("Liam", "Baker", "Tart", "Roughly", "Baker Liam roughly chopped the fresh berries for the tart, splashing juice across his apron.");
        AddAct2Story("Liam", "Baker", "Tart", "Crudely", "Baker Liam crudely crimped the tart pastry edge, giving it an unmistakable homemade look.");

        // 3. BEN + OFFICER
        AddAct2Story("Ben", "Officer", "Lens", "Quietly", "Officer Ben quietly adjusted his magnifying lens to examine the mysterious footprint near the alley door.");
        AddAct2Story("Ben", "Officer", "Lens", "Eagerly", "Officer Ben eagerly polished his observation lens, ready to scrutinize every detail of the crime scene.");
        AddAct2Story("Ben", "Officer", "Lens", "Keenly", "Officer Ben keenly focused his lens on a tiny speck of evidence left on the window sill.");
        AddAct2Story("Ben", "Officer", "Lens", "Slyly", "Officer Ben slyly peeked through his tinted lens while pretending to read the morning newspaper.");
        AddAct2Story("Ben", "Officer", "Folder", "Quietly", "Officer Ben quietly filed the secret case folder inside the lockbox behind his desk.");
        AddAct2Story("Ben", "Officer", "Folder", "Eagerly", "Officer Ben eagerly opened the case folder, hoping to find the missing suspect's description.");
        AddAct2Story("Ben", "Officer", "Folder", "Keenly", "Officer Ben keenly cross-referenced the documents in the folder to spot the contradiction in the alibi.");
        AddAct2Story("Ben", "Officer", "Folder", "Slyly", "Officer Ben slyly slipped the confidential folder into his trench coat before heading out on patrol.");
        AddAct2Story("Ben", "Officer", "Camera", "Quietly", "Officer Ben quietly snapped a photograph of the street sign to confirm the time of the occurrence.");
        AddAct2Story("Ben", "Officer", "Camera", "Eagerly", "Officer Ben eagerly lined up the camera lens to capture the key piece of evidence before rain set in.");
        AddAct2Story("Ben", "Officer", "Camera", "Keenly", "Officer Ben keenly documented every angle of the precinct room using his vintage camera.");
        AddAct2Story("Ben", "Officer", "Camera", "Slyly", "Officer Ben slyly hid his camera behind a stack of books to record the suspect's movements.");

        // 4. BEN + BAKER
        AddAct2Story("Ben", "Baker", "Jar", "Boldly", "Baker Ben boldly unsealed the ancient ceramic jar to reveal a fragrant sourdough starter.");
        AddAct2Story("Ben", "Baker", "Jar", "Keenly", "Baker Ben keenly inspected the glass jar for air bubbles to test the yeast fermentation.");
        AddAct2Story("Ben", "Baker", "Jar", "Wisely", "Baker Ben wisely stored the vanilla sugar jar on the highest shelf away from humidity.");
        AddAct2Story("Ben", "Baker", "Jar", "Anew", "Baker Ben anew sterilized every glass storage jar before filling them with custom marmalade.");
        AddAct2Story("Ben", "Baker", "Spice", "Boldly", "Baker Ben boldly added a pinch of rare cinnamon spice to his morning bun dough.");
        AddAct2Story("Ben", "Baker", "Spice", "Keenly", "Baker Ben keenly smelled the freshly ground nutmeg spice to verify its origin and quality.");
        AddAct2Story("Ben", "Baker", "Spice", "Wisely", "Baker Ben wisely balanced the pungent spice blend so it wouldn't overpower the delicate pastry.");
        AddAct2Story("Ben", "Baker", "Spice", "Anew", "Baker Ben anew combined rare aromatic spices to create a secret ingredient for his bread.");
        AddAct2Story("Ben", "Baker", "Recipe", "Boldly", "Baker Ben boldly altered the classic croissant recipe by doubling the butter layers.");
        AddAct2Story("Ben", "Baker", "Recipe", "Keenly", "Baker Ben keenly studied the handwritten antique recipe to master the ancestral baking technique.");
        AddAct2Story("Ben", "Baker", "Recipe", "Wisely", "Baker Ben wisely documented his updated pastry recipe in a bound leather journal.");
        AddAct2Story("Ben", "Baker", "Recipe", "Anew", "Baker Ben anew drafted a revolutionary recipe that promised the fluffiest brioche in town.");

        // 5. EMMA + OFFICER
        AddAct2Story("Emma", "Officer", "Badge", "Neatly", "Officer Emma neatly pinned her gold badge to her jacket lapel before stepping out to address the crowd.");
        AddAct2Story("Emma", "Officer", "Badge", "Fully", "Officer Emma fully polished her brass badge until it shone like a mirror in the morning sun.");
        AddAct2Story("Emma", "Officer", "Badge", "Alertly", "Officer Emma alertly tapped her badge to remind the panicked citizens that help had arrived.");
        AddAct2Story("Emma", "Officer", "Badge", "Calmly", "Officer Emma calmly presented her official badge, instantly diffusing the tense standoff.");
        AddAct2Story("Emma", "Officer", "Report", "Neatly", "Officer Emma neatly organized the incident report pages into a pristine blue binder.");
        AddAct2Story("Emma", "Officer", "Report", "Fully", "Officer Emma fully detailed every timeline event in her official shift report.");
        AddAct2Story("Emma", "Officer", "Report", "Alertly", "Officer Emma alertly reviewed the overnight patrol report for any unusual activity spikes.");
        AddAct2Story("Emma", "Officer", "Report", "Calmly", "Officer Emma calmly submitted her comprehensive case report directly to the precinct captain.");
        AddAct2Story("Emma", "Officer", "Pencil", "Neatly", "Officer Emma neatly sharpened her yellow pencil before taking notes during the witness interview.");
        AddAct2Story("Emma", "Officer", "Pencil", "Fully", "Officer Emma fully filled two notepad pages using her trusty mechanical pencil.");
        AddAct2Story("Emma", "Officer", "Pencil", "Alertly", "Officer Emma alertly jotted down the getaway car's license plate using her quick pencil.");
        AddAct2Story("Emma", "Officer", "Pencil", "Calmly", "Officer Emma calmly sketched a quick map of the intersection using her lead pencil.");

        // 6. EMMA + BAKER
        AddAct2Story("Emma", "Baker", "Whisk", "Neatly", "Baker Emma neatly hung her stainless steel whisk on the rack above the mixing station.");
        AddAct2Story("Emma", "Baker", "Whisk", "Calmly", "Baker Emma calmly whipped the egg whites into stiff meringue peaks using her favorite whisk.");
        AddAct2Story("Emma", "Baker", "Whisk", "Exactly", "Baker Emma exactly timed her whisk strokes to ensure the batter stayed light and airy.");
        AddAct2Story("Emma", "Baker", "Whisk", "Adeptly", "Baker Emma adeptly folded the chocolate ganache using a flexible wire whisk.");
        AddAct2Story("Emma", "Baker", "Timer", "Neatly", "Baker Emma neatly aligned three digital timers on the flour-dusted kitchen counter.");
        AddAct2Story("Emma", "Baker", "Timer", "Calmly", "Baker Emma calmly set the baking timer, trusting her instincts for the perfect golden crust.");
        AddAct2Story("Emma", "Baker", "Timer", "Exactly", "Baker Emma exactly calibrated the oven timer down to the final remaining second.");
        AddAct2Story("Emma", "Baker", "Timer", "Adeptly", "Baker Emma adeptly tracked multiple bread loaves baking simultaneously across four timers.");
        AddAct2Story("Emma", "Baker", "Scale", "Neatly", "Baker Emma neatly wiped down the digital scale after measuring out the artisan rye flour.");
        AddAct2Story("Emma", "Baker", "Scale", "Calmly", "Baker Emma calmly calibrated the balance scale before weighing the precious saffron threads.");
        AddAct2Story("Emma", "Baker", "Scale", "Exactly", "Baker Emma exactly weighed 500 grams of bread flour to ensure batch consistency.");
        AddAct2Story("Emma", "Baker", "Scale", "Adeptly", "Baker Emma adeptly balanced the kitchen scale while portioning out dough balls for rolls.");
    }

    public NarrativeOutcome GetCurrentOutcome()
    {
        // Fixed: Updated to call InitializeNarrativeDatabase() instead of InitializeOutcomeDictionary()
        if (outcomeDict == null) InitializeNarrativeDatabase();

        string charKey = (SelectedCharacterWord ?? "").ToLower().Trim();
        string placeKey = (SelectedPlaceWord ?? "").ToLower().Trim();

        if (outcomeDict.TryGetValue((charKey, placeKey), out NarrativeOutcome outcome))
        {
            return outcome;
        }

        return (narrativeOutcomes != null && narrativeOutcomes.Count > 0) ? narrativeOutcomes[0] : default;
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
                ArcadeRetryUI.Instance?.ShowRetryPrompt("Try Again!");
                ArcadeStageController.Instance?.ResetStage();
            }
            else
            {
                if (DeductLife())
                {
                    ArcadeRetryUI.Instance?.ShowRetryPrompt("Try Again!");
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
            return (category == WordCategory.Character) ? (outcome.act2CharacterWords ?? new List<string>()) : (outcome.act2PlaceWords ?? new List<string>());
        }
        else if (act == 3)
        {
            return (category == WordCategory.Character) ? (outcome.act3CharacterWords ?? new List<string>()) : (outcome.act3PlaceWords ?? new List<string>());
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
        WordVisualManager.Instance?.ClearAllVisuals();
        ChangePhase(GamePhase.Assembly);
    }

    // --- DISPLAY STORY GENERATORS ---
    public string GetDisplayStory() => GetAct1DisplayStory();

    public string GetAct1DisplayStory() => GetCurrentOutcome().displayStory;

    public string GetAct2SentenceTemplate() => GetCurrentOutcome().act2Template;

    public string GetAct3SentenceTemplate() => GetCurrentOutcome().act3Template;

    public string GetAct2DisplayStory()
    {
        // 1. Check exact 72-outcome dictionary match
        var key = (
            (SelectedCharacterWord ?? "").ToLower().Trim(),
            (SelectedPlaceWord ?? "").ToLower().Trim(),
            (Act2SelectedCharacterWord ?? "").ToLower().Trim(),
            (Act2SelectedPlaceWord ?? "").ToLower().Trim()
        );

        if (act2UniqueStories.TryGetValue(key, out string uniqueStory))
        {
            return uniqueStory;
        }

        // 2. Fallback to template replacement if key is missing
        NarrativeOutcome outcome = GetCurrentOutcome();
        return outcome.act2Template
            .Replace("{Character}", Act2SelectedCharacterWord ?? "[Object]")
            .Replace("{Place}", Act2SelectedPlaceWord ?? "[Adverb]");
    }

    public string GetAct3DisplayStory()
    {
        // Act 3: Dynamic Template + Randomized Suffix
        NarrativeOutcome outcome = GetCurrentOutcome();
        string baseTemplate = outcome.act3Template;

        if (string.IsNullOrEmpty(baseTemplate))
        {
            baseTemplate = "Braving the {Place}, the hero sought out a {Character}.";
        }

        string baseSentence = baseTemplate
            .Replace("{Character}", Act3SelectedCharacterWord ?? "[Secondary Character]")
            .Replace("{Place}", Act3SelectedPlaceWord ?? "[Weather]");

        string charKey = (SelectedCharacterWord ?? "").ToLower().Trim();
        string placeKey = (SelectedPlaceWord ?? "").ToLower().Trim();
        string randomSuffix = "";

        if (branchAct3Additions.TryGetValue((charKey, placeKey), out string[] additions))
        {
            int randomIndex = UnityEngine.Random.Range(0, additions.Length);
            randomSuffix = additions[randomIndex];
        }
        else
        {
            int randomIndex = UnityEngine.Random.Range(0, generalAct3Additions.Length);
            randomSuffix = generalAct3Additions[randomIndex];
        }

        return $"{baseSentence} {randomSuffix}";
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
        SceneManager.LoadScene(mainGameSceneName); // Loads "GameScene"
    }
}