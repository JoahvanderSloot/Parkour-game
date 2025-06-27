using UnityEngine;
using UnityEngine.SceneManagement;

public class Buttons : MonoBehaviour
{
    [Header("Only for the StartGame button")]
    [SerializeField] Settings settings;

    public void PointerEnter()
    {
        transform.localScale = new Vector2(1.1f, 1.1f);
        AudioManager.Instance.Play("Hover");
    }

    public void PointerExit()
    {
        transform.localScale = Vector2.one;
    }

    public void StartGame()
    {
        AudioManager.Instance.Play("Click");
        switch (settings.CurrentLevelIndex)
        {
            case 0:
                SceneManager.LoadScene("Tutorial");
                break;
            case 1:
                SceneManager.LoadScene("Level1");
                break;
            case 2:
                SceneManager.LoadScene("Level2");
                break;
        }
    }

    public void UnPause()
    {
        AudioManager.Instance.Play("Click");
        GameManager _gameManager = FindFirstObjectByType<GameManager>();
        if (_gameManager != null)
        {
            _gameManager.settings.Paused = false;
        }
    }

    public void MainMenu()
    {
        AudioManager.Instance.Play("Click");
        SceneManager.LoadScene("MainMenu");
    }

    public void SettingsMenu()
    {
        AudioManager.Instance.Play("Click");
        SceneManager.LoadScene("Settings");
    }
}