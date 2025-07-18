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
    public Color CrosshairColor;
    public float CrosshairSize;
    [Header("Game Settings")]
    public bool Paused;
    public bool GameOver;
    [Header("Game Info")]
    public int Score;
    public int Highscore;
    public float GameTimer;
    public int CurrentLevelIndex;
    public bool IsLaunched;
}
