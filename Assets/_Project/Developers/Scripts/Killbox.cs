using UnityEngine;
using UnityEngine.SceneManagement;

public class Killbox : MonoBehaviour
{
    [SerializeField] UiManager UI;
    [SerializeField] GameObject tutorialText;

    private void OnTriggerEnter(Collider other)
    {
        tutorialText.SetActive(false);
        UI.IsTutorial = true;
        UI.GameManager.settings.GameOver = true;
    }
}
