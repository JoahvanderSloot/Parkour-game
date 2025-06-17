using ImprovedTimers;
using PlayerSystems.Interaction;
using PrimeTween;
using UnityEngine;

namespace PlayerSystems.EnvironmentalObjects.GrapplePoints {
    public class GrapplePoint : MonoBehaviour, IInteractable {
        [SerializeField] float maxInteractionDistance = 25f;
        
        public bool RequireLook => true;
        public float MaxInteractionDistance => maxInteractionDistance;
        
        CountdownTimer cooldownTimer;

        Vector3 defaultScale;
        
        void Start() {
            gameObject.layer = IInteractable.InteractableLayer;
            cooldownTimer = new CountdownTimer(1f);
            GrapplePointManager.RegisterGrapplePoint(this);
            defaultScale = transform.localScale;
        }

        void OnDestroy() {
            GrapplePointManager.UnregisterGrapplePoint(this);
        }

        public void StartCooldown(float durationSeconds) {
            cooldownTimer.Reset(durationSeconds);
            cooldownTimer.Start();
        }
        
        public bool CanInteract() => cooldownTimer.IsFinished;

        public bool OnInteract(PlayerController player, InteractionPhase phase) {
            return phase is InteractionPhase.Pressed;
        }
        public void OnHoverEnter() {
            Tween.Scale(transform, defaultScale * 1.5f, 0.2f);
        }
        public void OnHoverExit() {
            Tween.Scale(transform, defaultScale, 0.2f);
        }
    }
}