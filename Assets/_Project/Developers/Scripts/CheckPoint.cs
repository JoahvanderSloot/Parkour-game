using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    public bool isActive;
    [HideInInspector] public bool isPlayerInTrigger;
    [SerializeField] GameObject visualObj;

    private void Update()
    {
        visualObj.SetActive(isActive);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = false;
        }
    }
}
