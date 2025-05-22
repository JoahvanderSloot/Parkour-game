using TMPro;
using UnityEngine;
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
    [SerializeField] GameManager gameManager;

    private void Start()
    {
        hitPoints = GameObject.FindWithTag("Player").GetComponentInParent<HitPoints>();
        crosshair.sprite = gameManager.settings.Crosshairs[gameManager.settings.CrosshairIndex];
        crosshair.color = gameManager.settings.crosshairColor;
    }

    private void Update()
    {
        GameOver.SetActive(gameManager.settings.GameOver);

        if (gameManager.settings.GameOver)
        {
            timerText.text = "0.00";
            EscMenu.SetActive(false);
            damageFlashImage.color = new Color(1, 0, 0, 0);
            scoreText.gameObject.SetActive(false);
            highscoreText.text = "Highscore: " + gameManager.settings.Highscore.ToString("0");
            finalScore.text = "Score: " + gameManager.settings.Score.ToString("0");

            if (gameManager.GameTime == 0)
            {
                gameOverText.text = "Times up!";
            }
            else
            {
                gameOverText.text = "Game Over!";
            }

            return;
        }

        EscMenu.SetActive(gameManager.settings.Paused);
        timerText.text = gameManager.GameTime.ToString("0.00");
        scoreText.text = "[" + gameManager.settings.Score.ToString() + "]";

        if (hitPoints != null)
        {
            damageFlashImage.color = new Color(1, 0, 0, alphaValue / 10);

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
