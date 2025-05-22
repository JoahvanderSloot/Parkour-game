using UnityEngine;

namespace PlayerSystems.Interaction {
    public class InteractionHandler : MonoBehaviour {
        [SerializeField] float rayDistance = 3f;
        [SerializeField] float headProbeRadius = 2f;
        [SerializeField] LayerMask interactableLayer;

        PlayerController player;
        
        IInteractable currentInteractable;

        public void Initialize(PlayerController playerController) {
            player = playerController;
        }

        void Update() {
            HandleInteractionProbing();
            
            if (player.GameplayInput.InteractPressed)
                HandleInteraction(currentInteractable);
        }

        void HandleInteractionProbing() {
            var ray = player.MainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            if (!InteractableProber.ProbeCrosshair(ray, rayDistance, interactableLayer, out var interactable))
                InteractableProber.ProbeAroundPoint(player.MainCamera.transform.position, headProbeRadius, interactableLayer, out interactable);

            HandleInteractableHit(interactable);
        }

        void HandleInteraction(IInteractable interactable) {
            interactable?.Interact(player);
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