using System.Collections;
using KinematicCharacterController;
using PrimeTween;
using UnityEngine;

namespace PlayerSystems.EnvironmentalObjects.Gravitational {
    public class RightAngleFlip : MonoBehaviour {
        [SerializeField] bool returnToNormalGravity = false;
        Coroutine motorRoutine;
        
        void OnTriggerEnter(Collider col) {
            if (col.TryGetComponent<KinematicCharacterMotor>(out var motor)) {
                FlipDirection(transform.up, motor);
            }
            else if (col.attachedRigidbody != null) {
                FlipDirection(transform.up, col.attachedRigidbody.transform);
            }
        }
        
        void OnTriggerExit(Collider col) {
            if (!returnToNormalGravity)
                return;
            
            if (col.TryGetComponent<KinematicCharacterMotor>(out var motor)) {
                FlipDirection(Vector3.up, motor);
            }
            else if (col.attachedRigidbody != null) {
                FlipDirection(Vector3.up, col.attachedRigidbody.transform);
            }
        }
        
        void FlipDirection(Vector3 newUpDirection, Transform tr) {
            float angleBetweenUpDirections = Vector3.Angle(newUpDirection, tr.up);
            float angleThreshold = 0.001f;
            
            if (angleBetweenUpDirections < angleThreshold)
                return;
            
            var rotationDifference = Quaternion.FromToRotation(tr.up, newUpDirection);
            Tween.Rotation(tr, rotationDifference * tr.rotation, 0.2f);
        }

        void FlipDirection(Vector3 newUpDirection, KinematicCharacterMotor motor) {
            float angleBetweenUpDirections = Vector3.Angle(newUpDirection, motor.CharacterUp);
            float angleThreshold = 0.001f;

            if (angleBetweenUpDirections < angleThreshold)
                return;

            if (motorRoutine != null)
                StopCoroutine(motorRoutine);
            
            motorRoutine = StartCoroutine(FlipMotor(motor, newUpDirection, 7f));
        }
        
        IEnumerator FlipMotor(KinematicCharacterMotor motor, Vector3 newUpDirection, float t) {
            float angle = Mathf.Infinity;
            float previousAngle = angle;
            float elapsedTime = 0f;
            while (elapsedTime < t * 0.1f) {

                var deltaTime = Time.deltaTime;
                var targetRotation = Quaternion.FromToRotation(motor.CharacterUp, newUpDirection) * motor.TransientRotation;
                var finalRotation = Quaternion.Slerp(motor.TransientRotation, targetRotation, elapsedTime / t);
                angle = Vector3.Angle(newUpDirection, finalRotation * Vector3.up);
                
                if (previousAngle < angle) 
                    yield break;
                
                motor.SetRotation(finalRotation);
                
                previousAngle = angle;
                elapsedTime += deltaTime;
                
                yield return null;
            }

            motor.SetRotation(Quaternion.FromToRotation(motor.CharacterUp, newUpDirection) * motor.TransientRotation);
        }
    }
}