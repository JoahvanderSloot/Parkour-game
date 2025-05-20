using System.Collections;
using Extensions;
using KinematicCharacterController;
using UnityEngine;

namespace PlayerSystems.EnvironmentalObjects.Gravitational {
     [RequireComponent(typeof(TriggerArea))]
    public class GravityWell : MonoBehaviour {
        TriggerArea triggerArea;
        
        void Start() {
            triggerArea = GetComponent<TriggerArea>();
        }

        // TODO: Use ManagedUpdate
        void Update() {
            foreach (var motor in triggerArea.Motors) {
                Vector3 directionToMotor = motor.TransientPosition - transform.position;
                Vector3 projection = Vector3.Project(directionToMotor, transform.forward);
                
                Vector3 center = projection + transform.position;
                
                Vector3 directionToCenter = center - motor.TransientPosition;
                
                RotateCharacterMotor(motor, directionToCenter, Time.deltaTime);
            }
        }

        void FixedUpdate() {
            foreach (var rb in triggerArea.Rigidbodies) {
                Vector3 directionToRb = rb.transform.position - transform.position;
                Vector3 projection = Vector3.Project(directionToRb, transform.forward);

                Vector3 center = projection + transform.position;

                Vector3 directionToCenter = center - rb.transform.position;
                
                RotateRigidBody(rb, directionToCenter);
            }
        }

        void OnTriggerExit(Collider other) {
            if (other.TryGetComponent(out KinematicCharacterMotor motor)) {
                StartCoroutine(FlipMotor(motor, Vector3.up, 3f));
            }
            else if (other.TryGetComponent(out Rigidbody rb)) {
                RotateRigidBody(rb, Vector3.up);
                Vector3 eulerAngles = rb.rotation.eulerAngles.With(x: 0f, z: 0f);
                rb.MoveRotation(Quaternion.Euler(eulerAngles));
            }
        }

        void RotateRigidBody(Rigidbody rb, Vector3 targetDirection) {
            targetDirection.Normalize();
            
            Quaternion rotationDifference = Quaternion.FromToRotation(rb.transform.up, targetDirection);
            Quaternion finalRotation = rotationDifference * rb.transform.rotation;
            
            rb.MoveRotation(finalRotation);
        }
        
        void RotateCharacterMotor(KinematicCharacterMotor motor, Vector3 targetDirection, float deltaTime) {
            targetDirection.Normalize();
            
            Quaternion rotationDifference = Quaternion.FromToRotation(motor.CharacterUp, targetDirection);
            Quaternion finalRotation = rotationDifference *  motor.TransientRotation;
            
            finalRotation = Quaternion.Slerp(motor.TransientRotation, finalRotation, Mathf.Min(30f * deltaTime, 1f));
            
            motor.SetRotation(finalRotation);
        }
        
        IEnumerator FlipMotor(KinematicCharacterMotor motor, Vector3 newUpDirection, float t) {
            float angle = Mathf.Infinity;
            float previousAngle = angle;
            float elapsedTime = 0f;
            
            while (elapsedTime < t * 5f) {
                var deltaTime = Time.deltaTime;
                var targetRotation = Quaternion.FromToRotation(motor.CharacterUp, newUpDirection) * motor.TransientRotation;
                var finalRotation = Quaternion.Slerp(motor.TransientRotation, targetRotation, t * Time.deltaTime);
                angle = Vector3.Angle(newUpDirection, finalRotation * Vector3.up);
                
                if (previousAngle < angle)
                    yield break;
                
                motor.SetRotation(finalRotation);
                
                previousAngle = angle;
                elapsedTime += deltaTime;
                
                yield return null;
            }

            motor.SetRotation(Quaternion.FromToRotation(motor.CharacterUp, newUpDirection) * motor.TransientRotation);
            Vector3 eulerAngles = motor.TransientRotation.eulerAngles.With(x: 0f, z: 0f);
            motor.SetRotation(Quaternion.Euler(eulerAngles));
        }
    }
}