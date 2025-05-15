using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public Settings settings;

    private void Start()
    {
        settings.Paused = false;
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            settings.Paused = !settings.Paused;
        }
        Time.timeScale = settings.Paused ? 0 : 1;

        Cursor.lockState = settings.Paused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = settings.Paused;
    }
}
