using UnityEngine;
using UnityEngine.UI;

public class ColorPickButton : MonoBehaviour
{
    [SerializeField] private Settings settings;

    [SerializeField] private Texture2D colorChart;
    [SerializeField] private Image crosshairIMG;
    [SerializeField] private GameObject chart;

    [SerializeField] private RectTransform cursor;
    [SerializeField] private Image cursorColor;

    public void PickColor()
    {
        RectTransform _chartRect = chart.GetComponent<RectTransform>();

        Vector2 _localPoint;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_chartRect, Input.mousePosition, null, out _localPoint))
            return;

        cursor.transform.position = Input.mousePosition;

        float _pivotX = _chartRect.pivot.x;
        float _pivotY = _chartRect.pivot.y;

        float _normalizedX = (_localPoint.x / _chartRect.rect.width) + _pivotX;
        float _normalizedY = (_localPoint.y / _chartRect.rect.height) + _pivotY;

        int _texX = Mathf.Clamp(Mathf.RoundToInt(_normalizedX * colorChart.width), 0, colorChart.width - 1);
        int _texY = Mathf.Clamp(Mathf.RoundToInt(_normalizedY * colorChart.height), 0, colorChart.height - 1);

        Color _pickedColor = colorChart.GetPixel(_texX, _texY);

        cursorColor.color = _pickedColor;
        settings.crosshairColor = _pickedColor;
        crosshairIMG.color = _pickedColor;
    }
}