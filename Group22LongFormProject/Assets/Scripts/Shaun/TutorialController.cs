using UnityEngine;

public class TutorialSceneController : MonoBehaviour
{
    [SerializeField] private GameObject arcadeInstructionObject;
    [SerializeField] private GameObject postArcadeInstructionObject;

    private bool hasCompletedArcade = false;

    private void OnEnable() => GameManager.OnPhaseChanged += HandlePhaseChanged;
    private void OnDisable() => GameManager.OnPhaseChanged -= HandlePhaseChanged;

    private void Start()
    {
        // Ensure both text objects start hidden
        HideAllInstructions();
    }

    private void HandlePhaseChanged(GamePhase newPhase)
    {
        if (newPhase == GamePhase.Arcade)
        {
            hasCompletedArcade = true;

            // Show Arcade text, ensure Post-Arcade text is hidden
            if (postArcadeInstructionObject != null) postArcadeInstructionObject.SetActive(false);
            if (arcadeInstructionObject != null) arcadeInstructionObject.SetActive(true);
        }
        else if (newPhase == GamePhase.Assembly && hasCompletedArcade)
        {
            // Hide Arcade text, show Post-Arcade text
            if (arcadeInstructionObject != null) arcadeInstructionObject.SetActive(false);
            if (postArcadeInstructionObject != null) postArcadeInstructionObject.SetActive(true);
        }
        else
        {
            // Hide both text objects during other phase transitions
            HideAllInstructions();
        }
    }

    private void HideAllInstructions()
    {
        if (arcadeInstructionObject != null) arcadeInstructionObject.SetActive(false);
        if (postArcadeInstructionObject != null) postArcadeInstructionObject.SetActive(false);
    }
}