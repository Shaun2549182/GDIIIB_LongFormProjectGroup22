using System.Collections;
using UnityEngine;
using TMPro;

public class ArcadeTimer : MonoBehaviour
{
    public static ArcadeTimer Instance { get; private set; }

    [SerializeField] private float arcadeDuration = 30f;
    [SerializeField] private TextMeshProUGUI timerText;

    private Coroutine timerCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        GameManager.OnPhaseChanged += HandlePhaseChanged;

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

    public void StartTimer()
    {
        StopTimer();

        if (timerText != null)
        {
            timerText.gameObject.SetActive(true);
            timerText.text = Mathf.CeilToInt(arcadeDuration).ToString("00");
        }

        timerCoroutine = StartCoroutine(RunArcadeTimer());
    }

    public void StopTimer()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }

        if (timerText != null)
        {
            timerText.gameObject.SetActive(false);
        }
    }

    public void ResetTimer()
    {
        StartTimer();
    }

    private IEnumerator RunArcadeTimer()
    {
        float timeRemaining = arcadeDuration;

        while (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;

            if (timerText != null)
            {
                timerText.text = Mathf.CeilToInt(Mathf.Max(0, timeRemaining)).ToString("00");
            }

            yield return null;
        }

        GameManager.Instance?.CompleteArcadePhase();
    }
}