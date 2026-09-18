using System.Collections;
using UnityEngine;
using TMPro; // Optional: for visual timer feedback

public class ArcadeTimer : MonoBehaviour
{
    [SerializeField] private float arcadeDuration = 30f; // 1 minute
    [SerializeField] private TextMeshProUGUI timerText;  // Optional UI text element

    private Coroutine timerCoroutine;

    private void OnEnable() => GameManager.OnPhaseChanged += HandlePhaseChanged;
    private void OnDisable() => GameManager.OnPhaseChanged -= HandlePhaseChanged;

    private void HandlePhaseChanged(GamePhase newPhase)
    {
        if (newPhase == GamePhase.Arcade)
        {
            if (timerCoroutine != null) StopCoroutine(timerCoroutine);
            timerCoroutine = StartCoroutine(RunArcadeTimer());
        }
        else
        {
            if (timerCoroutine != null) StopCoroutine(timerCoroutine);
            if (timerText != null) timerText.gameObject.SetActive(false);
        }
    }

    private IEnumerator RunArcadeTimer()
    {
        float timeRemaining = arcadeDuration;

        if (timerText != null) timerText.gameObject.SetActive(true);

        while (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;

            if (timerText != null)
            {
                timerText.text = Mathf.CeilToInt(timeRemaining).ToString("00");
            }

            yield return null;
        }

        // Time expired: Validate collected words before changing phase
        GameManager.Instance.CompleteArcadePhase();
    }
}