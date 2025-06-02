using System;
using System.Collections.Generic;
using KinematicCharacterController;
using UnityEngine;
using UnityEngine.Events;

namespace PlayerSystems.EnvironmentalObjects {
    public class TriggerArea : MonoBehaviour {
        public event Action<KinematicCharacterMotor> OnMotorEnter;
        public event Action<KinematicCharacterMotor> OnMotorExit;
        
        public event Action<Rigidbody> OnRigidbodyEnter;
        public event Action<Rigidbody> OnRigidbodyExit;

        [SerializeField] public UnityEvent onPlayerEnter;
        [SerializeField] public UnityEvent onPlayerExit;
        
        readonly List<Rigidbody> rigidbodies = new();
        public IReadOnlyList<Rigidbody> Rigidbodies => rigidbodies;
        
        readonly List<KinematicCharacterMotor> motors = new();
        public IReadOnlyList<KinematicCharacterMotor> Motors => motors;
        
        
        void OnTriggerEnter(Collider other) {
            if (other.TryGetComponent<KinematicCharacterMotor>(out var motorEnter) && !motors.Contains(motorEnter)) {
                OnMotorEnter?.Invoke(motorEnter);
                onPlayerEnter?.Invoke();
            }
            else if (other.attachedRigidbody is { } rbEnter && !rigidbodies.Contains(rbEnter)) {
                OnRigidbodyEnter?.Invoke(rbEnter);
            }
            
            if (other.TryGetComponent<KinematicCharacterMotor>(out var motor) && !motors.Contains(motor))
                motors.Add(motor);
            else if (other.attachedRigidbody is { } rb && !rigidbodies.Contains(rb))
                rigidbodies.Add(rb);
        }
        
        void OnTriggerExit(Collider other) {
            if (other.TryGetComponent<KinematicCharacterMotor>(out var motorExit)) {
                OnMotorExit?.Invoke(motorExit);
                onPlayerExit?.Invoke();
            }
            else if (other.attachedRigidbody is { } rbExit) {
                OnRigidbodyExit?.Invoke(rbExit);
            }
            
            if (other.TryGetComponent<KinematicCharacterMotor>(out var motor))
                motors.Remove(motor);
            else if (other.attachedRigidbody is { } rb)
                rigidbodies.Remove(rb);
        }
    }
}