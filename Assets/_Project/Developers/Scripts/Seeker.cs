using UnityEngine;

public class Seeker : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] GameObject flashLight;
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

            if (Vector3.Distance(transform.position, _targetPosition) > 5f)
            {
                realSpeed = speed * 2;
            }
            else
            {
                realSpeed = speed;
            }

            Vector3 _lookDirection = _targetPosition - transform.position;
            Quaternion _targetRotation = Quaternion.LookRotation(_lookDirection);
            flashLight.transform.rotation = Quaternion.Slerp(flashLight.transform.rotation, _targetRotation, Time.deltaTime * 5f);

            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(_lookDirection), Time.deltaTime * 5f);
        }
    }
}