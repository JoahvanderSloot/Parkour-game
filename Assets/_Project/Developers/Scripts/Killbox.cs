using UnityEngine;
using UnityEngine.SceneManagement;

public class Killbox : MonoBehaviour
{
    [SerializeField] bool isLevel2;

    [Header("Only for level 2")]
    [SerializeField] GameObject drone;

    [Header("Only for tutorial level")]
    [SerializeField] UiManager UI;
    [SerializeField] GameObject tutorialText;

    private void OnTriggerEnter(Collider other)
    {
        if (isLevel2)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                drone.SetActive(false);
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
