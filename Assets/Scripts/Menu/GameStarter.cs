using UnityEngine;
using UnityEngine.SceneManagement;

// Script for loading my gameplay scene from a UI button
// I use this on the "Start Game" button in GameMenuScene and I reuse it on the
// "Restart Level" button in my in-game pause menu (reloading demo restarts the level)
public class GameStarter : MonoBehaviour
{
    // Hooked up to the Button's OnClick() event in the Inspector
    // (GameStarter -> StartGame)
    public void StartGame()
    {
        // "demo" is the name of my original interactive scene
        SceneManager.LoadScene("demo");

        // Time.timeScale carries over between scenes, so if I restart from the
        // pause menu (where timeScale is 0) the new scene would still be paused
        // Setting it back to 1 right after LoadScene() fixes that
        Time.timeScale = 1f;
    }
}
