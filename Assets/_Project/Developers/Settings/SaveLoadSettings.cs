using System.IO;
using UnityEngine;

public class SaveLoadSettings : MonoBehaviour
{
    public Settings settings;

    private string SavePath => Path.Combine(Application.persistentDataPath, "playerData.json");

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
            Debug.Log("Data loaded from " + SavePath);
        }
        else
        {
            Debug.LogWarning("No save file found at " + SavePath);
        }
    }
}
