using PlayerSystems.Controls.Weapons.Animations;
using UnityEngine;

namespace PlayerSystems {
    public class PlayerHandsAnimations : MonoBehaviour {
        [SerializeField] PlayerAnimationConfig animationConfig;

        PlayerAnimationSystem animationSystem;

        public void Initialize(PlayerController playerController) {
            animationSystem = PlayerAnimationSystem.Create(animationConfig, playerController, nameof(PlayerHandsAnimations));
            animationSystem.Enable();
        }
        
        void FixedUpdate() {
            animationSystem?.FixedUpdate();
        }
    }
}