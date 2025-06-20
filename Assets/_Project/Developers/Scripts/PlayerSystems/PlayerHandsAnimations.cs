using System;
using PlayerSystems.Controls.Weapons.Animations;
using PlayerSystems.Modules;
using PlayerSystems.Modules.MovementModules;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerSystems {
    public class PlayerHandsAnimations : MonoBehaviour {
        [SerializeField] PlayerAnimationConfig animationConfig;
        [SerializeField] OneShotAnimationConfig navigatorAnimationConfig = OneShotAnimationConfig.Default;

        PlayerAnimationSystem animationSystem;

        PlayerController player;

        public void Initialize(PlayerController playerController) {
            player = playerController;
            
            animationSystem =
                PlayerAnimationSystem.Create(animationConfig, player, nameof(PlayerHandsAnimations));
            animationSystem.Enable();

            player.Modules.OnModuleEnabled += OnModuleEnabled;
        }

        void OnModuleEnabled(IPlayerModule module) {
            if (module is JumpModule movementModule) {
                PlayJumpAnimation();
            }
        }

        void FixedUpdate() {
            animationSystem?.FixedUpdate();
        }

        void OnDestroy() {
            animationSystem?.Dispose();
            animationSystem = null;
            
            player.Modules.OnModuleEnabled -= OnModuleEnabled;
        }

        void Update() {
            if (Keyboard.current.tabKey.wasPressedThisFrame) {
                animationSystem.PlayOneShot(navigatorAnimationConfig);
            }
        }

        void PlayJumpAnimation() {
            if (animationSystem == null)
                return;

            if (animationConfig.JumpConfig.HasValue) {
                animationSystem.PlayOneShot(animationConfig.JumpConfig.Value);
            }
        }
    }
}