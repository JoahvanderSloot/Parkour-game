using System;
using PlayerSystems.Interaction;
using PrimeTween;
using UnityEngine;

namespace PlayerSystems.EnvironmentalObjects.Lache {
    public class Lache : MonoBehaviour, IInteractable {
        public const string c_LacheTag = "Lache";
        
        [SerializeField] Transform barStart;
        [SerializeField] Transform barEnd;
        
        public Vector3 BarDirection => (barEnd.position - barStart.position).normalized;

        public bool AllowInteraction = true;
        
        void Start() {
            gameObject.tag = c_LacheTag;
            gameObject.layer = IInteractable.InteractableLayerIndex;
        }
        
        public Vector3 GetAttachmentPoint(Vector3 position) {
            Vector3 start = barStart.position;
            Vector3 end = barEnd.position;
            Vector3 point = position;

            Vector3 barDirection = end - start;
            float barLength = barDirection.magnitude;
            if (barLength == 0f) return start;

            Vector3 barDirNormalized = barDirection / barLength;
            float projectedLength = Vector3.Dot(point - start, barDirNormalized);
            float clampedLength = Mathf.Clamp(projectedLength, 0f, barLength);

            return start + barDirNormalized * clampedLength;
        }

        public bool MustBeLookedAt => false;
        public bool OnInteract(PlayerController player, InteractionPhase phase) {
            if (!AllowInteraction)
                return false;
            
            if (phase is InteractionPhase.Released)
                return false;
            
            Debug.Log("Interacting with Lache");
            return true;
        }
        public void OnHoverEnter() {
            Debug.Log("Hovering with Lache");
            Tween.Scale(transform, Vector3.one * 1.1f, 0.2f);
        }
        public void OnHoverExit() {
            Debug.Log("Hovering out with Lache");
            Tween.Scale(transform, Vector3.one, 0.2f);
        }
    }
}