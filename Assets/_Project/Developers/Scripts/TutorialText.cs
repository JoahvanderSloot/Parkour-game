using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TutorialText : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] TextMeshProUGUI infoText;
    [SerializeField] List<string> tutorialTexts;
    [SerializeField] float segmentDistance;

    private void Update()
    {
        int _index = Mathf.FloorToInt(player.transform.position.x / segmentDistance);

        _index = Mathf.Clamp(_index, 0, tutorialTexts.Count - 1);

        infoText.text = tutorialTexts[_index];
    }
}
