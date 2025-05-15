using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class Seeker : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] GameObject flashLight;
    [SerializeField] float detectionRange;
    float realSpeed;
    Transform player;
    [SerializeField] LayerMask obstacleMask;
    [SerializeField] float sidestepSpeed;
    [SerializeField] float sidestepDistance;

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

            transform.position = new Vector3(transform.position.x, player.position.y + 5, transform.position.z);

            KillPlayer();
            AvoidBuildings();
        }
    }

    private void KillPlayer()
    {
        RaycastHit _hit;
        Vector3 _raycastOrigin = transform.position;

        Debug.DrawRay(_raycastOrigin, (player.position - _raycastOrigin).normalized * detectionRange, Color.red, 0.1f, false);
        if (Physics.Raycast(_raycastOrigin, (player.position - _raycastOrigin).normalized, out _hit, detectionRange))
        {
            if (_hit.collider.CompareTag("Player"))
            {
                HitPoints _hitPoints = _hit.collider.gameObject.GetComponentInParent<HitPoints>();
                if (_hitPoints != null)
                {
                    _hitPoints.IsHit = true;
                }
            }
        }
        else
        {
            HitPoints _hitPoints = player.GetComponentInParent<HitPoints>();
            if (_hitPoints != null)
            {
                _hitPoints.IsHit = false;
            }
        }
    }

    private void AvoidBuildings()
    {
        Vector3 _directionToPlayer = (player.position - transform.position).normalized;
        _directionToPlayer.y = 0;

        if (Physics.Raycast(transform.position, _directionToPlayer, out RaycastHit hit, detectionRange, obstacleMask))
        {
            Vector3 _rightStep = Vector3.Cross(Vector3.up, _directionToPlayer) * sidestepDistance;
            Vector3 _rightCheck = transform.position + _rightStep;

            if (!Physics.Raycast(_rightCheck, _directionToPlayer, detectionRange, obstacleMask))
            {
                transform.position = Vector3.MoveTowards(transform.position, transform.position + _rightStep, sidestepSpeed * Time.deltaTime);
            }
            else
            {
                Vector3 _leftStep = -_rightStep;
                Vector3 _leftCheck = transform.position + _leftStep;

                if (!Physics.Raycast(_leftCheck, _directionToPlayer, detectionRange, obstacleMask))
                {
                    transform.position = Vector3.MoveTowards(transform.position, transform.position + _leftStep, sidestepSpeed * Time.deltaTime);
                }
            }
        }
    }

}
