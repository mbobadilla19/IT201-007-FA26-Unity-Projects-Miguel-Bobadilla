using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public TMP_Text buttonLabel;

    private bool isPaused;
    private float previousTimeScale = 1f;
    private bool previousAudioPause;

    void Start()
    {
        UpdateLabel();
    }

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.pKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (isPaused)
        {
            Resume();
            return;
        }

        previousTimeScale = Time.timeScale;
        previousAudioPause = AudioListener.pause;
        isPaused = true;
        Time.timeScale = 0f;
        AudioListener.pause = true;
        UpdateLabel();
    }

    public void Resume()
    {
        if (!isPaused)
        {
            return;
        }

        Time.timeScale = previousTimeScale;
        AudioListener.pause = previousAudioPause;
        isPaused = false;
        UpdateLabel();
    }

    void UpdateLabel()
    {
        if (buttonLabel != null)
        {
            buttonLabel.text = isPaused ? "Resume" : "Pause";
        }
    }

    void OnDisable()
    {
        // Restore normal state when leaving the scene or stopping Play mode.
        Resume();
    }
}