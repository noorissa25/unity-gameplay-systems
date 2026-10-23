using UnityEngine;

// In-game pause menu. This goes on the pause menu Canvas in demo.unity
// The Canvas Group starts hidden (alpha 0, not interactable, doesn't block raycasts)
// and pressing Escape shows/hides the whole menu and pauses/unpauses the game
[RequireComponent(typeof(CanvasGroup))]
public class PauseMenuToggle : MonoBehaviour
{
    // Reference to the Canvas Group so I can control the panel and all its buttons at once
    private CanvasGroup canvasGroup;

    void Awake()
    {
        // Grab the Canvas Group on this same GameObject (the Canvas)
        canvasGroup = GetComponent<CanvasGroup>();

        // Let myself know if it's missing so the menu doesn't silently fail
        if (canvasGroup == null)
            Debug.LogError("PauseMenuToggle: No CanvasGroup found on " + gameObject.name);
    }

    void Update()
    {
        // Toggle the menu when Escape is released
        // (Eventually this should be Input.GetButtonUp() with a virtual button from the
        // InputManager so game controllers can open the menu too)
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (canvasGroup.interactable)
            {
                // Menu is open -> hide it and disable its buttons
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.alpha = 0f;

                // Unpause the game
                Time.timeScale = 1f;
            }
            else
            {
                // Menu is closed -> show it and let the buttons be clicked
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
                canvasGroup.alpha = 1f;

                // Pause the game while the menu is up (single player game)
                Time.timeScale = 0f;
            }
        }
    }
}
