using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelSelector : MonoBehaviour
{
    [SerializeField] Settings settings;
    [SerializeField] List<GameObject> levels;
    [SerializeField] TextMeshProUGUI levelName;

    private void Start()
    {
        UpdateCurrentLevel();
    }

    public void NextLevel()
    {
        AudioManager.Instance.Play("Click");
        if (settings.CurrentLevelIndex < levels.Count - 1)
        {
            settings.CurrentLevelIndex++;
        }
        else
        {
            settings.CurrentLevelIndex = 0;
        }
        UpdateCurrentLevel();
    }

    public void PreviousLevel()
    {
        AudioManager.Instance.Play("Click");
        if (settings.CurrentLevelIndex >= 0)
        {
            settings.CurrentLevelIndex--;
        }
        else
        {
            settings.CurrentLevelIndex = levels.Count - 1;
        }
        UpdateCurrentLevel();
    }

    private void UpdateCurrentLevel()
    {
        levels[settings.CurrentLevelIndex].SetActive(true);
        for (int i = 0; i < levels.Count; i++)
        {
            if (i != settings.CurrentLevelIndex)
            {
                levels[i].SetActive(false);
            }
        }

        switch (settings.CurrentLevelIndex)
        {
            case 0:
                levelName.text = "Tutorial";
                break;
            case 1:
                levelName.text = "Level one";
                break;
            case 2:
                levelName.text = "Level two";
                break;
        }
    }
}
