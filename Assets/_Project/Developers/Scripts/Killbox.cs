using UnityEngine;
using UnityEngine.SceneManagement;

public class Killbox : MonoBehaviour
{
    [SerializeField] bool isLevel2;

    [SerializeField] UiManager UI;
    [SerializeField] GameObject tutorialText;

    private void OnTriggerEnter(Collider other)
    {
        if (isLevel2)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                HitPoints hitPoints = other.gameObject.GetComponentInParent<HitPoints>();
                hitPoints.IsHit = true;
            }
        }
        else
        {
            tutorialText.SetActive(false);
            UI.IsTutorial = true;
            UI.GameManager.settings.GameOver = true;
        }
    }
}
