using PlayerSystems;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SocialPlatforms.Impl;

public class GameManager : MonoBehaviour
{
    public Settings settings;
    public float GameTime;
    [SerializeField] List<CheckPoint> checkPoints;
    PlayerController player;

    private void Start()
    {
        settings.GameOver = false;
        settings.Paused = false;
        settings.Score = 0;
        GameTime = settings.GameTimer;
        checkPoints[Random.Range(0, checkPoints.Count)].isActive = true;
        GameObject _playerBody = GameObject.FindGameObjectWithTag("Player");
        player = _playerBody.GetComponentInParent<PlayerController>();
        player.enabled = true;
    }

    private void Update()
    {
        if (!settings.GameOver)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                settings.Paused = !settings.Paused;
            }
            Time.timeScale = settings.Paused ? 0 : 1;

            Cursor.lockState = settings.Paused ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = settings.Paused;
            player.enabled = !settings.Paused;

            if (!settings.Paused)
            {
                GameCountdown();
                CheckpointHandeling();
            }
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            player.enabled = false;
        }
    }

    public void GameCountdown()
    {
        GameTime -= Time.deltaTime;
        if (GameTime <= 0)
        {
            if (settings.Score > settings.Highscore)
            {
                settings.Highscore = settings.Score;
            }
            GameTime = 0;
            settings.GameOver = true;
        }
    }

    private void CheckpointHandeling()
    {
        CheckPoint _completedCheckpoint = null;

        foreach (CheckPoint _checkP in checkPoints)
        {
            if (_checkP.isActive && _checkP.isPlayerInTrigger)
            {
                _checkP.isActive = false;
                _checkP.isPlayerInTrigger = false;
                settings.Score++;
                _completedCheckpoint = _checkP;
                break;
            }
        }

        if (_completedCheckpoint != null)
        {
            checkPoints.Remove(_completedCheckpoint);

            checkPoints[Random.Range(0, checkPoints.Count)].isActive = true;

            checkPoints.Add(_completedCheckpoint);
        }
    }

}