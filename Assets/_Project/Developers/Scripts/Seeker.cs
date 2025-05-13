using UnityEngine;

public class Seeker : MonoBehaviour
{
    [SerializeField] float speed;
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

            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(_targetPosition - transform.position), Time.deltaTime * 5f);
        }
    }
}
