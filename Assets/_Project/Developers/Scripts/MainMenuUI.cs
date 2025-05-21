using TMPro;
using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI highscoreText;
    [SerializeField] Settings settings;

    private void Start()
    {
        highscoreText.text = "Highscore: " + settings.Highscore.ToString();
    }
}
