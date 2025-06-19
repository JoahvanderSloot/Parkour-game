using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class Seeker : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] float killSpeed;
    float realSpeed;
    GameManager gameManager;

    [Header("Player Detection")]
    [SerializeField] GameObject flashLight;
    [SerializeField] float detectionRange;
    Transform player;

    [Header("Avoid Buildings")]
    [SerializeField] LayerMask obstacleMask;

    private void Start()
    {
        realSpeed = speed;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        gameManager = FindFirstObjectByType<GameManager>();
    }

    private void Update()
    {
        if (player != null && !gameManager.settings.GameOver)
        {
            Vector3 _targetPosition = player.position;
            _targetPosition.y = transform.position.y;

            transform.position = Vector3.MoveTowards(transform.position, _targetPosition, realSpeed * Time.deltaTime);

            float _distance = Vector3.Distance(transform.position, _targetPosition);

            if (_distance > 3.5f)
            {
                realSpeed = speed + _distance;
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

            SeekerHeight();
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
                    _hitPoints.KillTime = killSpeed;
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
        Collider[] _obstacles = Physics.OverlapSphere(transform.position, 2.5f, obstacleMask);
        Vector3 _avoidanceDirection = Vector3.zero;

        foreach (Collider obstacle in _obstacles)
        {
            Vector3 _directionAway = transform.position - obstacle.ClosestPoint(transform.position);
            float _distance = _directionAway.magnitude;

            if (_distance > 0f)
            {
                _avoidanceDirection += _directionAway.normalized / _distance;
            }
        }

        if (_avoidanceDirection != Vector3.zero)
        {
            Vector3 _moveDirection = (player.position - transform.position).normalized + _avoidanceDirection.normalized;
            _moveDirection.y = 0f;

            transform.position += _moveDirection.normalized * realSpeed * Time.deltaTime;;
        }
    }

    private void SeekerHeight()
    {
        float _targetY = player.position.y + 5f;

        RaycastHit _hit;
        Vector3 _rayOrigin = new Vector3(transform.position.x, _targetY, transform.position.z);
        float _rayDistance = 5f;

        if (Physics.Raycast(_rayOrigin, Vector3.down, out _hit, _rayDistance, obstacleMask))
        {
            _targetY = _hit.point.y - 0.1f;
        }

        transform.position = new Vector3(transform.position.x, Mathf.Min(player.position.y + 5f, _targetY), transform.position.z);
    }
}
