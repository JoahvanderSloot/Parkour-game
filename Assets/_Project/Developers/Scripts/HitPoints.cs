using UnityEngine;

public class HitPoints : MonoBehaviour
{
    public bool IsHit;

    private void Update()
    {
        if (IsHit)
        {
            Debug.Log("Player hit");
        }
    }
}
