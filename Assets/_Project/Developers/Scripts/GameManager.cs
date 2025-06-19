using PlayerSystems;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    PlayerController player;
    
    public Settings settings;
    public float GameTime;
    [SerializeField] List<CheckPoint> checkPoints;

    [Tooltip("This should be empty in all levels exept for the tutorial")]
    [SerializeField] Transform currentCheckP;
    GameObject navigator;

    private void Start()
    {
        settings.GameOver = false;
        settings.Paused = false;
        settings.Score = 0;

        if (settings.CurrentLevelIndex == 1)
        {
            GameTime = settings.GameTimer * 2;
        }
        else
        {
            GameTime = settings.GameTimer;
        }

        GameObject _playerBody = GameObject.FindGameObjectWithTag("Player");
        player = _playerBody.GetComponentInParent<PlayerController>();
        if(checkPoints.Count != 0)
        {
            checkPoints[Random.Range(0, checkPoints.Count)].isActive = true;
        }
        player.enabled = true;

        navigator = GameObject.FindGameObjectWithTag("Navigator");
        navigator.gameObject.SetActive(false);
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
                if(checkPoints.Count > 0)
                {
                    GameCountdown();
                    CheckpointHandeling();
                }
                Navigation();
            }
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            player.enabled = false;
            navigator.SetActive(false);
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
            if (_checkP.isActive)
            {
                currentCheckP = _checkP.transform;

                if (_checkP.isPlayerInTrigger)
                {
                    _checkP.isActive = false;
                    _checkP.isPlayerInTrigger = false;
                    settings.Score++;
                    _completedCheckpoint = _checkP;
                    break;
                }
            }
        }

        if (_completedCheckpoint != null)
        {
            checkPoints.Remove(_completedCheckpoint);

            checkPoints[Random.Range(0, checkPoints.Count)].isActive = true;

            checkPoints.Add(_completedCheckpoint);
        }
    }


    private void Navigation()
    {
        if(navigator == null)
        {
            navigator = GameObject.FindGameObjectWithTag("Navigator");
        }

        if (Keyboard.current.tabKey.isPressed)
        {
            navigator.gameObject.SetActive(true);
        }
        else
        {
            navigator.gameObject.SetActive(false);
        }

        Transform _arrow = navigator.transform.GetChild(0);
        _arrow.LookAt(currentCheckP);
    }
}