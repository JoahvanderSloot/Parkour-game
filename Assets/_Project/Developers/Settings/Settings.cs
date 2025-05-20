using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Settings.asset", menuName = "Settings", order = 0)]
public class Settings : ScriptableObject
{
    [Header("Generic Settings")]
    public bool Audio;
    public bool Music;
    [Header("Crosshair Settings")]
    public List<Sprite> Crosshairs;
    public int CrosshairIndex;
    public Color crosshairColor;
    [Header("Game Settings")]
    public bool Paused;
    public float GameTimer;
    [Header("Game Info")]
    public int Score;
    public int Highscore;
}
