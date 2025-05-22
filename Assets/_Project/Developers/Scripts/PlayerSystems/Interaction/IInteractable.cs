using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace PlayerSystems.Interaction {
    public enum InteractionPhase {
        Pressed,
        Held,
        Released
    }
    
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public interface IInteractable {
        public const string c_InteractableLayerName = "Interactable";
        public static readonly int InteractableLayerIndex = LayerMask.NameToLayer(c_InteractableLayerName);
        public static readonly int InteractableLayerMask = 1 << InteractableLayerIndex;
        
        bool MustBeLookedAt { get; }
        
        bool OnInteract(PlayerController player, InteractionPhase phase);
        void OnHoverEnter();
        void OnHoverExit();
    }
}