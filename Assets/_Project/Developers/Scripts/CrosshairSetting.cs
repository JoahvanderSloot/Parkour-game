using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CrosshairSetting : MonoBehaviour
{
    [SerializeField] Settings settings;
    [SerializeField] Image image;

    private void Start()
    {
        image.sprite = settings.Crosshairs[settings.CrosshairIndex];
        image.color = settings.CrosshairColor;
    }

    public void Next()
    {
        if (settings.Crosshairs.Count - 1 > settings.CrosshairIndex)
        {
            settings.CrosshairIndex++;
        }
        else
        {
            settings.CrosshairIndex = 0;
        }
        image.sprite = settings.Crosshairs[settings.CrosshairIndex];
    }

    public void Prev()
    {
        if (settings.CrosshairIndex > 0)
        {
            settings.CrosshairIndex--;
        }
        else
        {
            settings.CrosshairIndex = settings.Crosshairs.Count - 1;
        }
        image.sprite = settings.Crosshairs[settings.CrosshairIndex];
    }
}