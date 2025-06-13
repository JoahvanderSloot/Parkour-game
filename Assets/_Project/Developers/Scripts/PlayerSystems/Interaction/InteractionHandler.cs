using System;
using PlayerSystems.Input;
using UnityEngine;

namespace PlayerSystems.Interaction {
    public class InteractionHandler : MonoBehaviour {
        public event Action<IInteractable, Vector3> OnInteract;
        
        [SerializeField] float probeDistance = 2.5f;
        [SerializeField] float headProbeRadius = 1.75f;
        
        PlayerController player;
        GameplayInputReader Input => player.GameplayInput;
        
        IInteractable currentInteractable;

        public void Initialize(PlayerController playerController) {
            player = playerController;
        }

        void Update() {
            HandleInteractionProbing(out var hitPoint);
            HandleInteraction(currentInteractable, hitPoint);
        }

        void HandleInteractionProbing(out Vector3 interactionPoint) {
            var ray = player.MainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            if (!InteractableProber.ProbeRay(ray, probeDistance, out var interactable, out interactionPoint))
                InteractableProber.ProbeAroundPoint(player.MainCamera.transform.position, headProbeRadius, out interactable, out interactionPoint);

            HandleInteractableHit(interactable);
        }

        void HandleInteraction(IInteractable interactable, Vector3 hitPoint) {
            if (interactable == null)
                return;

            if (Input.InteractPressedThisFrame) {
                if (interactable.OnInteract(player, InteractionPhase.Pressed))
                    OnInteract?.Invoke(interactable, hitPoint);
                    
                return;
            }
            
            if (Input.InteractPressed) {
                if (interactable.OnInteract(player, InteractionPhase.Held))
                    OnInteract?.Invoke(interactable, hitPoint);
                
                return;
            }
            
            if (Input.InteractReleasedThisFrame) {
                if (interactable.OnInteract(player, InteractionPhase.Released))
                    OnInteract?.Invoke(interactable, hitPoint);
            }
        }

        void HandleInteractableHit(IInteractable interactable) {
            if (currentInteractable == interactable)
                return;

            currentInteractable?.OnHoverExit();
            currentInteractable = interactable;
            currentInteractable?.OnHoverEnter();
        }
    }
}