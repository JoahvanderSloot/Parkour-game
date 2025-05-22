using UnityEngine;

namespace PlayerSystems.Interaction {
    public interface IInteractable {
        public const string c_InteractableLayerName = "Interactable";
        public static int InteractableLayer => LayerMask.NameToLayer(c_InteractableLayerName);
        
        bool MustBeLookedAt { get; }
        void Interact(PlayerController player);
        void OnHoverEnter();
        void OnHoverExit();
    }
}