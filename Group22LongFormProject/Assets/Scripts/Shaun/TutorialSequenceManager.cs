using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialSequenceManager : MonoBehaviour
{
    [Header("1. Intro Overlay UI")]
    [SerializeField] private CanvasGroup tutorialIntroGroup;
    [SerializeField] private Button nextButton;
    [SerializeField] private float introSlideOffset = 30f;

    [Header("2. Assembly Phase UI & World Objects")]
    [SerializeField] private SpriteRenderer sentenceBoxSprite;
    [SerializeField] private CanvasGroup sentenceContentGroup;
    [SerializeField] private CanvasGroup collectionBoxGroup;

    [TextArea(3, 5)]
    [SerializeField]
    private string tutorialAssemblyTemplate =
        "Welcome to the tutorial! Your goal is to collect words to place in this area in the spaces {Character} and {Place}. To do so, press the start button to begin the Arcade phase.";

    [Header("3. Arcade Phase Gameplay UI & Objects")]
    [SerializeField] private CanvasGroup arcadeUIGroup;
    [SerializeField] private GameObject arcadeWorldObjectsContainer;

    [Header("4. Instruction Text Groups")]
    [SerializeField] private CanvasGroup arcadeInstructionsGroup;     // Text 1: Arcade instructions
    [SerializeField] private CanvasGroup postArcadeInstructionsGroup; // Text 2: Completion instructions

    [Header("5. Post-Arcade Tutorial-Only Elements")]
    [SerializeField] private SpriteRenderer triangleSprite;          // Additional shape sprite 1
    [SerializeField] private SpriteRenderer rectangleSprite;         // Additional shape sprite 2
    [SerializeField] private List<CanvasGroup> extraPostArcadeUIGroups = new List<CanvasGroup>();
    [SerializeField] private List<SpriteRenderer> extraPostArcadeSprites = new List<SpriteRenderer>();

    [Header("Animation Timings")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float delayBetweenFades = 0.15f;

    private List<SpriteRenderer> arcadeSprites = new List<SpriteRenderer>();
    private List<TMP_Text> arcadeTexts = new List<TMP_Text>();
    private bool isTransitioning = false;
    private bool hasEnteredArcade = false;
    private bool hasRevealedPostArcade = false;

    private void Awake()
    {
        if (arcadeWorldObjectsContainer != null)
        {
            arcadeSprites.AddRange(arcadeWorldObjectsContainer.GetComponentsInChildren<SpriteRenderer>(true));
            arcadeTexts.AddRange(arcadeWorldObjectsContainer.GetComponentsInChildren<TMP_Text>(true));
        }

        // 1. Hide Intro initially
        if (tutorialIntroGroup != null)
        {
            tutorialIntroGroup.alpha = 0f;
            tutorialIntroGroup.blocksRaycasts = false;
            tutorialIntroGroup.transform.localPosition -= new Vector3(0f, introSlideOffset, 0f);
        }

        // 2. Hide Assembly Elements initially
        SetSpriteAlpha(sentenceBoxSprite, 0f);
        SetCanvasGroupState(sentenceContentGroup, visible: false);
        SetCanvasGroupState(collectionBoxGroup, visible: false);

        // 3. Hide Arcade Elements & Instructions initially
        SetCanvasGroupState(arcadeUIGroup, visible: false);
        SetCanvasGroupState(arcadeInstructionsGroup, visible: false);
        SetCanvasGroupState(postArcadeInstructionsGroup, visible: false);

        // 4. Hide Post-Arcade Tutorial-Only Elements initially
        SetSpriteAlpha(triangleSprite, 0f);
        SetSpriteAlpha(rectangleSprite, 0f);

        foreach (var group in extraPostArcadeUIGroups)
        {
            SetCanvasGroupState(group, visible: false);
        }

        foreach (var sprite in extraPostArcadeSprites)
        {
            SetSpriteAlpha(sprite, 0f);
        }

        if (arcadeWorldObjectsContainer != null)
        {
            SetWorldElementsAlpha(0f);
            arcadeWorldObjectsContainer.SetActive(false);
        }
    }

    private void OnEnable() => GameManager.OnPhaseChanged += HandlePhaseChanged;
    private void OnDisable() => GameManager.OnPhaseChanged -= HandlePhaseChanged;

    private void Start()
    {
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(OnNextButtonClicked);
        }

        StartCoroutine(AnimateIntroIn());
    }

    private IEnumerator AnimateIntroIn()
    {
        if (tutorialIntroGroup == null) yield break;

        Vector3 targetPos = tutorialIntroGroup.transform.localPosition + new Vector3(0f, introSlideOffset, 0f);
        Vector3 startPos = tutorialIntroGroup.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / fadeDuration);

            tutorialIntroGroup.alpha = t;
            tutorialIntroGroup.transform.localPosition = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        tutorialIntroGroup.alpha = 1f;
        tutorialIntroGroup.blocksRaycasts = true;
        tutorialIntroGroup.interactable = true;
    }

    private void OnNextButtonClicked()
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionToIntroToAssembly());
    }

    private IEnumerator TransitionToIntroToAssembly()
    {
        isTransitioning = true;

        yield return StartCoroutine(FadeGroup(tutorialIntroGroup, 1f, 0f));
        SetCanvasGroupState(tutorialIntroGroup, visible: false);

        yield return new WaitForSeconds(delayBetweenFades);

        yield return StartCoroutine(FadeSprite(sentenceBoxSprite, 0f, 1f));

        yield return new WaitForSeconds(delayBetweenFades);

        SentenceEvents.TriggerSentenceInit(tutorialAssemblyTemplate);

        SetCanvasGroupState(sentenceContentGroup, visible: true);
        SetCanvasGroupState(collectionBoxGroup, visible: true);

        StartCoroutine(FadeGroup(sentenceContentGroup, 0f, 1f));
        yield return StartCoroutine(FadeGroup(collectionBoxGroup, 0f, 1f));

        GameManager.Instance?.ChangePhase(GamePhase.Assembly);

        isTransitioning = false;
    }

    private void HandlePhaseChanged(GamePhase newPhase)
    {
        if (newPhase == GamePhase.Arcade)
        {
            hasEnteredArcade = true;
            StartCoroutine(TransitionToArcadePhase());
        }
        else if (newPhase == GamePhase.Assembly && hasEnteredArcade && !hasRevealedPostArcade)
        {
            // Only reveal tutorial post-arcade elements if the Arcade phase actually succeeded
            if (GameManager.Instance != null && GameManager.Instance.IsArcadeCompleted())
            {
                hasRevealedPostArcade = true;
                StartCoroutine(TransitionToPostArcadeAssembly());
            }
        }
    }

    private IEnumerator TransitionToArcadePhase()
    {
        if (arcadeWorldObjectsContainer != null)
        {
            arcadeWorldObjectsContainer.SetActive(true);
            SetWorldElementsAlpha(0f);
        }

        SetCanvasGroupState(arcadeUIGroup, visible: true);
        SetCanvasGroupState(arcadeInstructionsGroup, visible: true);

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.SmoothStep(0f, 1f, elapsed / fadeDuration);

            if (arcadeUIGroup != null) arcadeUIGroup.alpha = alpha;
            if (arcadeInstructionsGroup != null) arcadeInstructionsGroup.alpha = alpha;
            SetWorldElementsAlpha(alpha);

            yield return null;
        }

        if (arcadeUIGroup != null) arcadeUIGroup.alpha = 1f;
        if (arcadeInstructionsGroup != null) arcadeInstructionsGroup.alpha = 1f;
        SetWorldElementsAlpha(1f);
    }

    private IEnumerator TransitionToPostArcadeAssembly()
    {
        // 1. Fade out Arcade Instructions (Text 1) & Arcade UI
        yield return StartCoroutine(FadeGroup(arcadeInstructionsGroup, 1f, 0f));
        SetCanvasGroupState(arcadeInstructionsGroup, visible: false);
        SetCanvasGroupState(arcadeUIGroup, visible: false);

        yield return new WaitForSeconds(delayBetweenFades);

        // 2. Fade in Tutorial Post-Arcade Instructions & Extra Tutorial Elements only
        // (LockInButton remains hidden and manages itself once both sentence slots are filled)
        SetCanvasGroupState(postArcadeInstructionsGroup, visible: true);
        StartCoroutine(FadeGroup(postArcadeInstructionsGroup, 0f, 1f));

        StartCoroutine(FadeSprite(triangleSprite, 0f, 1f));
        StartCoroutine(FadeSprite(rectangleSprite, 0f, 1f));

        foreach (var group in extraPostArcadeUIGroups)
        {
            if (group != null)
            {
                SetCanvasGroupState(group, visible: true);
                StartCoroutine(FadeGroup(group, 0f, 1f));
            }
        }

        foreach (var sprite in extraPostArcadeSprites)
        {
            if (sprite != null)
            {
                StartCoroutine(FadeSprite(sprite, 0f, 1f));
            }
        }
    }

    // --- HELPER METHODS ---
    private void SetWorldElementsAlpha(float alpha)
    {
        foreach (var s in arcadeSprites)
        {
            if (s != null)
            {
                Color c = s.color;
                c.a = alpha;
                s.color = c;
            }
        }

        foreach (var t in arcadeTexts)
        {
            if (t != null)
            {
                Color c = t.color;
                c.a = alpha;
                t.color = c;
            }
        }
    }

    private IEnumerator FadeSprite(SpriteRenderer sprite, float startAlpha, float endAlpha)
    {
        if (sprite == null) yield break;

        float elapsed = 0f;
        SetSpriteAlpha(sprite, startAlpha);

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / fadeDuration);
            SetSpriteAlpha(sprite, Mathf.Lerp(startAlpha, endAlpha, t));
            yield return null;
        }

        SetSpriteAlpha(sprite, endAlpha);
    }

    private void SetSpriteAlpha(SpriteRenderer sprite, float alpha)
    {
        if (sprite != null)
        {
            Color c = sprite.color;
            c.a = alpha;
            sprite.color = c;
        }
    }

    private IEnumerator FadeGroup(CanvasGroup group, float startAlpha, float endAlpha)
    {
        if (group == null) yield break;

        float elapsed = 0f;
        group.alpha = startAlpha;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / fadeDuration);
            group.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            yield return null;
        }

        group.alpha = endAlpha;
    }

    private void SetCanvasGroupState(CanvasGroup group, bool visible, bool interactable = true)
    {
        if (group == null) return;
        group.alpha = visible ? 1f : 0f;
        group.blocksRaycasts = visible && interactable;
        group.interactable = visible && interactable;
    }
}