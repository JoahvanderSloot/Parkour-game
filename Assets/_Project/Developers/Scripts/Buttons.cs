using UnityEngine;
using UnityEngine.SceneManagement;

public class Buttons : MonoBehaviour
{
    public void PointerEnter()
    {
        transform.localScale = new Vector2(1.1f, 1.1f);
    }

    public void PointerExit()
    {
        transform.localScale = Vector2.one;
    }

    public void StartGame()
    {
        SceneManager.LoadScene("JoahTest");
    }

    public void UnPause()
    {
        GameManager _gameManager = FindFirstObjectByType<GameManager>();
        if (_gameManager != null)
        {
            _gameManager.settings.Paused = false;
        }
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void SettingsMenu()
    {
        SceneManager.LoadScene("Settings");
    }
}