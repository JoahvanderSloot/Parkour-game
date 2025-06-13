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
        public const float c_DefaultInteractionDistance = 2.75f;
        public const string c_InteractableLayerName = "Interactable";
        public static readonly int InteractableLayer = LayerMask.NameToLayer(c_InteractableLayerName);
        public static readonly LayerMask InteractableLayerMask = 1 << InteractableLayer;
        
        /// <summary>
        /// Whether interaction can be performed without looking at the object. (Around the player)
        /// </summary>
        bool RequireLook { get; }
        
        /// <summary>
        /// Maximum distance at which the player can interact with the object.
        /// </summary>
        float MaxInteractionDistance { get; }
        
        /// <summary>
        /// Checks if the player can interact with the object.
        /// </summary>
        bool CanInteract();
        
        /// <summary>
        /// Called when the player interacts with the object.
        /// </summary>
        /// <returns>Success of the interaction (used to invoke OnInteract event)</returns>
        bool OnInteract(PlayerController player, InteractionPhase phase);
        
        /// <summary>
        /// Called when current interaction target.
        /// </summary>
        void OnHoverEnter();
        
        /// <summary>
        /// Called when no longer current interaction target.
        /// </summary>
        void OnHoverExit();
    }
}