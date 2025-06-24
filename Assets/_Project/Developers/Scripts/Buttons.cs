using UnityEngine;
using UnityEngine.SceneManagement;

public class Buttons : MonoBehaviour
{
    [Header("Only for the StartGame button")]
    [SerializeField] Settings settings;

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

    public void QuitGame()
    {
        #if (UNITY_EDITOR || DEVELOPMENT_BUILD)
                Debug.Log(this.name + " : " + this.GetType() + " : " + System.Reflection.MethodBase.GetCurrentMethod().Name);
        #endif
        #if (UNITY_EDITOR)
                UnityEditor.EditorApplication.isPlaying = false;
        #elif (UNITY_STANDALONE)
             Application.Quit();
        #elif (UNITY_WEBGL)
             Application.OpenURL("itch url ");
        #endif
    }
}