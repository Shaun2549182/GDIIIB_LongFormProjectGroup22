using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    // Method 1: Loads the scene named "Tutorial"
    public void LoadTutorial()
    {
        SceneManager.LoadScene("Tutorial");
    }

    public void LoadAct1()
    {
        SceneManager.LoadScene("GameScene");
    }

    // Method 2: Quits the game
    public void QuitGame()
    {
        Debug.Log("Quitting Game..."); // Confirms the click in the Unity Console while inside the Editor
        Application.Quit();
    }
}