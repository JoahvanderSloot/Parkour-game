using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CrosshairSetting : MonoBehaviour
{
    [SerializeField] Settings settings;
    [SerializeField] RectTransform picker;
    [SerializeField] Texture2D colorChart;
    bool isHovering = false;
    Image image;

    private void Start()
    {
        image = GetComponent<Image>();
        image.sprite = settings.Crosshairs[settings.CrosshairIndex];
        image.color = settings.crosshairColor;
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

    public void ChoseColor(BaseEventData _data)
    {
        PointerEventData _pointer = _data as PointerEventData;

        picker.position = _pointer.position;

        Color _pickedColor = colorChart.GetPixel((int)(picker.localPosition.x * (colorChart.width / transform.GetChild(0).GetComponent<RectTransform>().rect.width)), (int)(picker.localPosition.y * (colorChart.height / transform.GetChild(0).GetComponent<RectTransform>().rect.height)));

        settings.crosshairColor = _pickedColor;
        image.color = settings.crosshairColor;
    }
}
