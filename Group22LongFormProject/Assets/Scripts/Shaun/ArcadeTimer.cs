using System.Collections;
using UnityEngine;
using TMPro;

public class ArcadeTimer : MonoBehaviour
{
    [SerializeField] private float arcadeDuration = 30f;
    [SerializeField] private TextMeshProUGUI timerText;

    private Coroutine timerCoroutine;

    private void OnEnable()
    {
        GameManager.OnPhaseChanged += HandlePhaseChanged;

        // Catch phase state if component was enabled after the event was dispatched
        if (GameManager.Instance != null && GameManager.Instance.CurrentPhase == GamePhase.Arcade)
        {
            StartTimer();
        }
    }

    private void OnDisable()
    {
        GameManager.OnPhaseChanged -= HandlePhaseChanged;
        StopTimer();
    }

    private void HandlePhaseChanged(GamePhase newPhase)
    {
        if (newPhase == GamePhase.Arcade)
        {
            StartTimer();
        }
        else
        {
            StopTimer();
        }
    }

    private void StartTimer()
    {
        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        if (timerText != null) timerText.gameObject.SetActive(true);
        timerCoroutine = StartCoroutine(RunArcadeTimer());
    }

    private void StopTimer()
    {
        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        if (timerText != null) timerText.gameObject.SetActive(false);
    }

    private IEnumerator RunArcadeTimer()
    {
        float timeRemaining = arcadeDuration;

        while (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;

            if (timerText != null)
            {
                timerText.text = Mathf.CeilToInt(timeRemaining).ToString("00");
            }

            yield return null;
        }

        // Time expired: Complete arcade phase
        GameManager.Instance?.CompleteArcadePhase();
    }
}