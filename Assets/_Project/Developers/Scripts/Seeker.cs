using Unity.Mathematics;
using UnityEngine;

public class Seeker : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] GameObject flashLight;
    [SerializeField] float detectionRange;
    float realSpeed;
    Transform player;

    private void Start()
    {
        realSpeed = speed;
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        if (player != null)
        {
            Vector3 _targetPosition = player.position;
            _targetPosition.y = transform.position.y;

            transform.position = Vector3.MoveTowards(transform.position, _targetPosition, realSpeed * Time.deltaTime);

            float _distance = Vector3.Distance(transform.position, _targetPosition);

            if (_distance > 5f)
            {
                realSpeed = speed * 2;
            }
            else
            {
                realSpeed = speed;
            }

            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(_targetPosition - transform.position), Time.deltaTime * 5f);

            if (_distance > 0.3f)
            {
                float _angle = Mathf.InverseLerp(0f, 10f, _distance);
                _angle = 2f - _angle;
                Vector3 _lookDir = transform.forward + Vector3.down * _angle;

                flashLight.transform.rotation = Quaternion.LookRotation(_lookDir);
            }
            else
            {
                Quaternion targetRotation = Quaternion.Euler(90, 0, 0);
                flashLight.transform.rotation = Quaternion.Lerp(flashLight.transform.rotation, targetRotation, Time.deltaTime * 5f);
            }

            KillPlayer();
        }
    }

    private void KillPlayer()
    {
        RaycastHit _hit;
        Vector3 _raycastOrigin = transform.position;
        _raycastOrigin.y = player.position.y;

        if (Physics.Raycast(_raycastOrigin, (player.position - _raycastOrigin).normalized, out _hit, detectionRange))
        {
            if (_hit.collider.CompareTag("Player"))
            {
                Debug.Log("Player hit by raycast!");
            }
        }
    }
}
