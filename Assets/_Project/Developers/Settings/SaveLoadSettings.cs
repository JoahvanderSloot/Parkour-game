using System.IO;
using TMPro;
using UnityEngine;

public class SaveLoadSettings : MonoBehaviour
{
    public Settings settings;
    [SerializeField] TextMeshProUGUI highscoreText;
    [SerializeField] CrosshairList crosshairImages;

    private string SavePath => Path.Combine(Application.persistentDataPath, "playerData.json");

    private void Start()
    {
        if(settings.IsLaunched == false)
        {
            Load();
            settings.IsLaunched = true;
        }
        highscoreText.text = "Highscore: " + settings.Highscore.ToString();
    }

    [ContextMenu("Save Data")]
    public void Save()
    {
        string json = JsonUtility.ToJson(settings);
        File.WriteAllText(SavePath, json);
        Debug.Log("Data saved to " + SavePath);
    }

    [ContextMenu("Load Data")]
    public void Load()
    {
        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            JsonUtility.FromJsonOverwrite(json, settings);

            settings.Crosshairs.Clear();
            foreach (var _crosshair in crosshairImages.CrosshairImageList)
            {
                settings.Crosshairs.Add(_crosshair);
            }

            Debug.Log("Data loaded from " + SavePath);
        }
        else
        {
            Debug.LogWarning("No save file found at " + SavePath);
        }
    }

    public void QuitGame()
    {
        settings.IsLaunched = false;
        Save();

        #if (UNITY_EDITOR || DEVELOPMENT_BUILD)
            Debug.Log(this.name + " : " + this.GetType() + " : " + System.Reflection.MethodBase.GetCurrentMethod().Name);
        #endif
        #if (UNITY_EDITOR)
            UnityEditor.EditorApplication.isPlaying = false;
        #elif (UNITY_STANDALONE)
             Application.Quit();
        #elif (UNITY_WEBGL)
             Application.OpenURL("itch url ");
        #endif
    }
}
