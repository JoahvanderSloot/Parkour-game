using Extensions;
using PlayerSystems.Movement;
using UnityEngine;

namespace PlayerSystems.EnvironmentalObjects {
    [SelectionBase]
    public class BouncePad : MonoBehaviour {
        [SerializeField] Vector3 bounceForce = new(0f, 20f, 0f);
        
        void OnTriggerEnter(Collider other) {
            if (!other.CompareTag("Player"))
                return;
            
            if (!other.TryGetComponent(out PlayerMovement playerMovement))
                return;
            
            var motor = playerMovement.GetMotor();
            motor.ForceUnground();
            motor.BaseVelocity = motor.BaseVelocity.With(y: Mathf.Max(0f, motor.BaseVelocity.y));
            playerMovement.ApplyForce(bounceForce);
            playerMovement.InvokeOnResetJumps();
        }
    }
}