using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public Settings settings;
    public float gameTime;
    [SerializeField] List<CheckPoint> checkPoints;

    private void Start()
    {
        settings.Paused = false;
        gameTime = settings.GameTimer;
        checkPoints[Random.Range(0, checkPoints.Count)].isActive = true;
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            settings.Paused = !settings.Paused;
        }
        Time.timeScale = settings.Paused ? 0 : 1;

        Cursor.lockState = settings.Paused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = settings.Paused;

        if (!settings.Paused)
        {
            GameTime();
            CheckpointHandeling();
        }
    }

    public void GameTime()
    {
        gameTime -= Time.deltaTime;
        if (gameTime <= 0)
        {
            Debug.Log("Game Over");
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