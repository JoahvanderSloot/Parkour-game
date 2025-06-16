using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    HitPoints hitPoints;
    float alphaValue = 0f;

    [Header("Game UI")]
    [SerializeField] Image crosshair;
    [SerializeField] Image damageFlashImage;
    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] TextMeshProUGUI scoreText;

    [Header("Game over")]
    [SerializeField] GameObject GameOver;
    [SerializeField] TextMeshProUGUI gameOverText;
    [SerializeField] TextMeshProUGUI highscoreText;
    [SerializeField] TextMeshProUGUI finalScore;

    [Header("Other")]
    [SerializeField] GameObject EscMenu;
    public GameManager GameManager;
    [HideInInspector] public bool IsTutorial;

    private void Start()
    {
        IsTutorial = false;
        hitPoints = GameObject.FindWithTag("Player").GetComponentInParent<HitPoints>();
        crosshair.sprite = GameManager.settings.Crosshairs[GameManager.settings.CrosshairIndex];
        crosshair.color = GameManager.settings.crosshairColor;
    }

    private void Update()
    {
        GameOver.SetActive(GameManager.settings.GameOver);

        if (GameManager.settings.GameOver)
        {
            timerText.text = "0";
            EscMenu.SetActive(false);
            damageFlashImage.color = new Color(1, 0, 0, 0);
            scoreText.gameObject.SetActive(false);
            highscoreText.text = "Highscore: " + GameManager.settings.Highscore.ToString("0");
            finalScore.text = "Score: " + GameManager.settings.Score.ToString("0");

            if (!IsTutorial)
            {
                if (GameManager.GameTime == 0)
                {
                    gameOverText.text = "Times up!";
                }
                else
                {
                    gameOverText.text = "Game Over!";
                }
            }
            else
            {
                gameOverText.text = "Tutorial Finished!";
            }

            return;
        }

        EscMenu.SetActive(GameManager.settings.Paused);
        timerText.text = GameManager.GameTime.ToString("0;00");
        scoreText.text = "(" + GameManager.settings.Score.ToString() + ")";

        if (hitPoints != null)
        {
            damageFlashImage.color = new Color(1, 0, 0, alphaValue / 15);

            if (hitPoints.IsHit)
            {
                if(alphaValue < 5)
                {
                    alphaValue += Time.deltaTime * 6f;
                }
                else
                {
                    alphaValue = 5;
                }
            }
            else
            {
                if(alphaValue > 0f)
                {
                    alphaValue -= Time.deltaTime * 10f;
                }
                else
                {
                    alphaValue = 0f;
                }
            }
        }
    }
}
