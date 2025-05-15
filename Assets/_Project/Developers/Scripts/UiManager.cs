using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    [SerializeField] Settings settings;

    HitPoints hitPoints;
    [SerializeField] Image damageFlashImage;
    float alphaValue = 0f;

    [SerializeField] Image crosshair;

    [SerializeField] GameObject EscMenu;

    private void Start()
    {
        hitPoints = GameObject.FindWithTag("Player").GetComponentInParent<HitPoints>();
        crosshair.sprite = settings.Crosshairs[settings.CrosshairIndex];
        crosshair.color = settings.crosshairColor;
    }

    private void Update()
    {
        EscMenu.SetActive(settings.Paused);

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
