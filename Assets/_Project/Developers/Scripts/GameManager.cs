using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public Settings settings;
    public float gameTime;

    private void Start()
    {
        settings.Paused = false;
        gameTime = settings.GameTimer;
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

        if (!settings.Paused)
        {
            GameTime();
        }
    }

    public void GameTime()
    {
        gameTime -= Time.deltaTime;
        if (gameTime <= 0)
        {
            Debug.Log("Game Over");
        }
    }
}