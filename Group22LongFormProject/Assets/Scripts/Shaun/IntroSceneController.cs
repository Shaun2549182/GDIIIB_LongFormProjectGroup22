using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class IntroSceneController : MonoBehaviour
{
    [Header("Sequence Settings")]
    [SerializeField] private List<CanvasGroup> textCanvasGroups = new List<CanvasGroup>();

    [Header("Animation Settings")]
    [SerializeField] private float fadeDuration = 0.6f;
    [SerializeField] private float slideOffset = 40f; // Distance (pixels) to slide upward

    [Header("Navigation")]
    [SerializeField] private Button nextButton;
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private string finalButtonText = "Begin";
    [SerializeField] private string targetSceneName = "TutorialScene";

    private int currentIndex = 0;
    private List<Vector3> targetPositions = new List<Vector3>();
    private bool isAnimating = false;

    private void Awake()
    {
        // Auto-assign child text component if not manually hooked up in Inspector
        if (buttonText == null && nextButton != null)
        {
            buttonText = nextButton.GetComponentInChildren<TextMeshProUGUI>();
        }

        // Store resting transforms and hide all elements initially
        foreach (var cg in textCanvasGroups)
        {
            if (cg != null)
            {
                targetPositions.Add(cg.transform.localPosition);
                cg.alpha = 0f;
                cg.transform.localPosition -= new Vector3(0f, slideOffset, 0f);
            }
        }
    }

    private void Start()
    {
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(OnNextButtonClicked);
        }

        // Reveal first text element on start
        if (textCanvasGroups.Count > 0)
        {
            ShowTextItem(0);
            CheckAndUpdateButtonText(0);
        }
    }

    private void OnNextButtonClicked()
    {
        if (isAnimating) return; // Prevent rapid-click overlapping animations

        if (currentIndex < textCanvasGroups.Count - 1)
        {
            currentIndex++;
            ShowTextItem(currentIndex);
            CheckAndUpdateButtonText(currentIndex);
        }
        else
        {
            // All text elements displayed; advance to tutorial
            SceneManager.LoadScene(targetSceneName);
        }
    }

    private void CheckAndUpdateButtonText(int index)
    {
        if (index == textCanvasGroups.Count - 1 && buttonText != null)
        {
            buttonText.text = finalButtonText;
        }
    }

    private void ShowTextItem(int index)
    {
        if (index < 0 || index >= textCanvasGroups.Count) return;
        StartCoroutine(AnimateTextIn(textCanvasGroups[index], targetPositions[index]));
    }

    private IEnumerator AnimateTextIn(CanvasGroup cg, Vector3 targetPos)
    {
        isAnimating = true;
        Vector3 startPos = cg.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            cg.alpha = smoothT;
            cg.transform.localPosition = Vector3.Lerp(startPos, targetPos, smoothT);
            yield return null;
        }

        cg.alpha = 1f;
        cg.transform.localPosition = targetPos;
        isAnimating = false;
    }
}