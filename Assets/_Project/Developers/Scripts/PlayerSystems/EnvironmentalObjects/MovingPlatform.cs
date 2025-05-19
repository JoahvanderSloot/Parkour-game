using System.Collections;
using System.Collections.Generic;
using Extensions;
using UnityEngine;

namespace PlayerSystems.EnvironmentalObjects {
    [RequireComponent(typeof(TriggerArea), typeof(Rigidbody))]
    public class MovingPlatform : MonoBehaviour {
        [SerializeField] float movementSpeed = 5f;
        [SerializeField] float waitTime = 2f;
        [SerializeField] bool reverseDirection;
        [SerializeField] List<Transform> waypoints;

        bool isWaiting;
        int currentWaypointIndex;
        Transform currentWaypoint;

        TriggerArea triggerArea;
        Rigidbody rb;

        void Start() {
            triggerArea = GetComponent<TriggerArea>();
            rb = GetComponent<Rigidbody>();

            rb.freezeRotation = true;
            rb.useGravity = false;
            rb.isKinematic = true;

            if (waypoints.Count <= 0)
                Debug.LogWarning("No waypoints assigned to the moving platform: " + name);
            else
                currentWaypoint = waypoints[currentWaypointIndex];

            StartCoroutine(WaitRoutine());
            StartCoroutine(LateFixedUpdate());
        }

        IEnumerator WaitRoutine() {
            WaitForSeconds duration = WaitFor.Seconds(waitTime);
            while (true) {
                if (isWaiting) {
                    yield return duration;
                    isWaiting = false;
                }

                yield return null;
            }
        }

        IEnumerator LateFixedUpdate() {
            while (true) {
                yield return WaitFor.FixedUpdate;
                MovePlatform();
            }
        }

        void UpdateWayPoint() {
            currentWaypointIndex += reverseDirection ? -1 : 1;
            currentWaypointIndex = (currentWaypointIndex + waypoints.Count) % waypoints.Count;
            currentWaypoint = waypoints[currentWaypointIndex];
            isWaiting = true;
        }

        void MovePlatform() {
            if (waypoints.Count <= 0 || isWaiting)
                return;

            Vector3 toNextWaypoint = currentWaypoint.position - transform.position;
            Vector3 movement = toNextWaypoint.normalized * (movementSpeed * Time.deltaTime);

            if (movement.magnitude >= toNextWaypoint.magnitude || movement.magnitude == 0) {
                rb.transform.position = currentWaypoint.position;
                UpdateWayPoint();
            }
            else {
                rb.transform.position += movement;
            }

            foreach (var rb in triggerArea.Rigidbodies) {
                rb.MovePosition(rb.position + movement);
            }

            foreach (var motor in triggerArea.Motors) {
                motor.SetTransientPosition(motor.TransientPosition + movement);
            }
        }
    }
}