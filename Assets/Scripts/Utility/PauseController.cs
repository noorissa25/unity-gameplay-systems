using UnityEngine;

namespace GameplaySystems.Utility
{
    // Starts the game paused so the initial physics setup can be observed,
    // then toggles pause/unpause on "p".
    public class PauseController : MonoBehaviour
    {
        [SerializeField] private KeyCode toggleKey = KeyCode.P;

        private bool isPaused = true;
        private float previousTimeScale = 1f;

        void Start()
        {
            previousTimeScale = Time.timeScale;
            isPaused = true;
            Time.timeScale = 0f;
        }

        void Update()
        {
            if (Input.GetKeyUp(toggleKey))
            {
                TogglePause();
            }
        }

        public void TogglePause()
        {
            isPaused = !isPaused;

            if (isPaused)
            {
                previousTimeScale = Time.timeScale > 0f ? Time.timeScale : previousTimeScale;
                Time.timeScale = 0f;
            }
            else
            {
                Time.timeScale = previousTimeScale > 0f ? previousTimeScale : 1f;
            }
        }
    }
}
