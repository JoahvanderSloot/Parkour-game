using System.Collections;
using UnityEngine;

public class HitPoints : MonoBehaviour
{
    [HideInInspector] public float KillTime;
    [HideInInspector] public bool IsHit;
    Coroutine damageCoroutine;
    GameManager gameManager;

    private void Start()
    {
        gameManager = GameObject.FindFirstObjectByType<GameManager>();
    }

    private void Update()
    {
        if (IsHit)
        {
            if (damageCoroutine == null)
            {
                damageCoroutine = StartCoroutine(Damage());
            }
        }
        else
        {
            if (damageCoroutine != null)
            {
                StopCoroutine(damageCoroutine);
                damageCoroutine = null;
            }
        }
    }

    private IEnumerator Damage()
    {
        yield return new WaitForSeconds(KillTime);
        if (gameManager != null)
        {
            gameManager.settings.GameOver = true;
        }
    }
}
