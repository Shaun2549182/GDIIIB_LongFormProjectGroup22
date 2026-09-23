using UnityEngine;

public class QuitGameHandler : MonoBehaviour
{
    /// <summary>
    /// Call this method from your Quit Button's OnClick event in the Inspector.
    /// </summary>
    public void QuitGame()
    {
  

        // Standard build application quit
        Application.Quit();

    }
}