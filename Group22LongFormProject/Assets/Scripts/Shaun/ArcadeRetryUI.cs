using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(TextMeshProUGUI))]
public class ArcadeRetryUI : MonoBehaviour
{
    public static ArcadeRetryUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI promptText;

    [Header("Fade Timings")]
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float displayDuration = 0.8f;
    [SerializeField] private float fadeOutDuration = 0.3f;

    private Coroutine fadeCoroutine;

    private void Awake()
    {
        Instance = this;

        // Auto-grab components attached directly to this same GameObject
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (promptText == null) promptText = GetComponent<TextMeshProUGUI>();

        if (canvasGroup != null) canvasGroup.alpha = 0f;
    }

    public void ShowRetryPrompt(string message = "That's not right...")
    {
        if (promptText != null) promptText.text = message;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(DoFadeSequence());
    }

    private IEnumerator DoFadeSequence()
    {
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            if (canvasGroup != null) canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeInDuration);
            yield return null;
        }
        if (canvasGroup != null) canvasGroup.alpha = 1f;

        yield return new WaitForSeconds(displayDuration);

        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            if (canvasGroup != null) canvasGroup.alpha = Mathf.Clamp01(1f - (elapsed / fadeOutDuration));
            yield return null;
        }
        if (canvasGroup != null) canvasGroup.alpha = 0f;
    }
}