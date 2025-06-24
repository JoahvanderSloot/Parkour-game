using PlayerSystems.Controls.Weapons.Animations;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerSystems {
    public class PlayerHandsAnimations : MonoBehaviour {
        [SerializeField] PlayerAnimationConfig animationConfig;
        [SerializeField] OneShotAnimationConfig navigatorAnimationConfig = OneShotAnimationConfig.Default;
        
        PlayerAnimationSystem animationSystem;

        public void Initialize(PlayerController playerController) {
            animationSystem = PlayerAnimationSystem.Create(animationConfig, playerController, nameof(PlayerHandsAnimations));
            animationSystem.Enable();
        }
        
        void FixedUpdate() {
            animationSystem?.FixedUpdate();
        }

        void OnDestroy() {
            animationSystem?.Dispose();
        }

        void Update() {
            if (Keyboard.current.tabKey.wasPressedThisFrame) {
                animationSystem.PlayOneShot(navigatorAnimationConfig);
            }
        }
    }
}