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

        void Start() {
            gameObject.tag = c_LacheTag;
            gameObject.layer = IInteractable.InteractableLayer;
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
        public void Interact(PlayerController player) {
            Debug.Log("Interacting with Lache");
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