using System.Collections.Generic;
using ImprovedTimers;
using PlayerSystems.Interaction;
using UnityEngine;

namespace PlayerSystems.EnvironmentalObjects {
    public class RopePart : MonoBehaviour, IInteractable {
        public RopeVerlet RopeVerlet { get; private set; }
        public int Index { get; private set; }

        public Vector3 CurrentPosition => transform.position;
        public Vector3 PreviousPosition { get; private set; }
        public Vector3 Velocity => (CurrentPosition - PreviousPosition) / TimeSinceLastUpdate;
        
        public float TimeSinceLastUpdate => Time.time - previousUpdateTime;
        float previousUpdateTime;

        List<(Vector3 totalForce, int updateSpread, int timesUpdated)> forcesToApply;
        
        public void Initialize(RopeVerlet ropeVerlet, int index) {
            RopeVerlet = ropeVerlet;
            Index = index;
            
            forcesToApply = new List<(Vector3 totalForce, int updateSpread, int timesUpdated)>();
        }
        
        public void UpdatePosition(Vector3 position) {
            previousUpdateTime = Time.time;
            
            PreviousPosition = transform.position;
            transform.position = position;
            
            ApplyForces();
        }

        void ApplyForces() {
            for (int i = forcesToApply.Count - 1; i >= 0; i--) {
                var force = forcesToApply[i];
                RopeVerlet.ApplyForceToSegment(Index, force.totalForce / force.updateSpread);
                
                if (forcesToApply[i].timesUpdated >= force.updateSpread)
                    forcesToApply.RemoveAt(i);
                else
                    forcesToApply[i] = (force.totalForce, force.updateSpread, force.timesUpdated + 1);;
            }
        }
        
        public void AddForce(Vector3 force, int updateSpread = 5) {
            forcesToApply.Add((force, updateSpread, 0));
        }

        public bool AllowInteraction { get; set; } = true;
        public bool RequireLook => false;
        public float MaxInteractionDistance => IInteractable.c_DefaultInteractionDistance;

        public bool CanInteract() => AllowInteraction;

        public bool OnInteract(PlayerController player, InteractionPhase phase) {
            Debug.Log("RopePart Interacted");
            if (!AllowInteraction)
                return false;
            
            if (phase is InteractionPhase.Released)
                return false;
            
            return true;
        }
        public void OnHoverEnter() {
            
        }
        public void OnHoverExit() {
            
        }
    }
}