using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    public bool isActive;
    public bool isPlayerInTrigger;

    private void Update()
    {
        if (isActive)
        {
            MeshRenderer _renderer = GetComponent<MeshRenderer>();
            _renderer.material.color = Color.green;
        }
        else
        {
            MeshRenderer _renderer = GetComponent<MeshRenderer>();
            _renderer.material.color = Color.black;
        }
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
