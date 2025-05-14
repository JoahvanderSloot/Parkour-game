using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static PlayerInputActions;

namespace PlayerSystems.Input {
    public enum ButtonPhase {
        Pressed,
        Released
    }
    
    public interface IInputReader {
        void EnableActions();
    }
    
    [CreateAssetMenu(fileName = "new GameplayInputReader", menuName = "Player/Input/GameplayInputReader", order = 0)]
    public class GameplayInputReader : ScriptableObject, IInputReader, IGameplayActions, IDisposable {
        public event Action<Vector2> Look;
        public event Action<Vector2> Move; 
        public event Action<ButtonPhase> Jump;
        public event Action<ButtonPhase> Crouch;
        public event Action<ButtonPhase> MovementAbility;
        public event Action EquipMelee;
        public event Action EquipPrimary;
        public event Action EquipSecondary;
        public event Action EquipGadget;
        public event Action EquipPrevious;
        public event Action<ButtonPhase> PrimaryFire;
        public event Action<ButtonPhase> SecondaryFire;
        public event Action<ButtonPhase> WeaponAbility;
        public event Action<ButtonPhase> ThrowGrenade;
        public event Action<ButtonPhase> UltimateAbility;
        public event Action<ButtonPhase> Reload;
        public event Action Interact;
        
        public PlayerInputActions InputActions;
        
        public Vector2 LookDirectionDelta => InputActions.Gameplay.Look.ReadValue<Vector2>() + InputActions.Gameplay.LookController.ReadValue<Vector2>() * 50f;
        public Vector2 MoveInputDirection => InputActions.Gameplay.Move.ReadValue<Vector2>();
        
        public bool JumpPressed => InputActions.Gameplay.Jump.IsPressed();
        public bool CrouchPressed => InputActions.Gameplay.Crouch.IsPressed();
        public bool MovementAbilityPressed => InputActions.Gameplay.MovementAbility.IsPressed();
        public bool EquipMeleePressed => InputActions.Gameplay.EquipMelee.IsPressed();
        public bool EquipPrimaryPressed => InputActions.Gameplay.EquipPrimary.IsPressed();
        public bool EquipSecondaryPressed => InputActions.Gameplay.EquipSecondary.IsPressed();
        public bool EquipGadgetPressed => InputActions.Gameplay.EquipGadget.IsPressed();
        public bool EquipPreviousPressed => InputActions.Gameplay.EquipPrevious.IsPressed();
        public bool PrimaryFirePressed => InputActions.Gameplay.PrimaryFire.IsPressed();
        public bool SecondaryFirePressed => InputActions.Gameplay.SecondaryFire.IsPressed();
        public bool WeaponAbilityPressed => InputActions.Gameplay.WeaponAbility.IsPressed();
        public bool ThrowGrenadePressed => InputActions.Gameplay.ThrowGrenade.IsPressed();
        public bool UltimateAbilityPressed => InputActions.Gameplay.UltimateAbility.IsPressed();
        public bool ReloadPressed => InputActions.Gameplay.Reload.IsPressed();
        public bool InteractPressed => InputActions.Gameplay.Interact.IsPressed();
        
        public bool JumpPressedThisFrame => InputActions.Gameplay.Jump.WasPressedThisFrame();
        public bool CrouchPressedThisFrame => InputActions.Gameplay.Crouch.WasPressedThisFrame();
        public bool MovementAbilityPressedThisFrame => InputActions.Gameplay.MovementAbility.WasPressedThisFrame();
        public bool EquipMeleePressedThisFrame => InputActions.Gameplay.EquipMelee.WasPressedThisFrame();
        public bool EquipPrimaryPressedThisFrame => InputActions.Gameplay.EquipPrimary.WasPressedThisFrame();
        public bool EquipSecondaryPressedThisFrame => InputActions.Gameplay.EquipSecondary.WasPressedThisFrame();
        public bool EquipGadgetPressedThisFrame => InputActions.Gameplay.EquipGadget.WasPressedThisFrame();
        public bool EquipPreviousPressedThisFrame => InputActions.Gameplay.EquipPrevious.WasPressedThisFrame();
        public bool PrimaryFirePressedThisFrame => InputActions.Gameplay.PrimaryFire.WasPressedThisFrame();
        public bool SecondaryFirePressedThisFrame => InputActions.Gameplay.SecondaryFire.WasPressedThisFrame();
        public bool WeaponAbilityPressedThisFrame => InputActions.Gameplay.WeaponAbility.WasPressedThisFrame();
        public bool ThrowGrenadePressedThisFrame => InputActions.Gameplay.ThrowGrenade.WasPressedThisFrame();
        public bool UltimateAbilityPressedThisFrame => InputActions.Gameplay.UltimateAbility.WasPressedThisFrame();
        public bool ReloadPressedThisFrame => InputActions.Gameplay.Reload.WasPressedThisFrame();
        public bool InteractPressedThisFrame => InputActions.Gameplay.Interact.WasPressedThisFrame();
            
        public bool JumpReleasedThisFrame => InputActions.Gameplay.Jump.WasReleasedThisFrame();
        public bool CrouchReleasedThisFrame => InputActions.Gameplay.Crouch.WasReleasedThisFrame();
        public bool MovementAbilityReleasedThisFrame => InputActions.Gameplay.MovementAbility.WasReleasedThisFrame();
        public bool EquipMeleeReleasedThisFrame => InputActions.Gameplay.EquipMelee.WasReleasedThisFrame();
        public bool EquipPrimaryReleasedThisFrame => InputActions.Gameplay.EquipPrimary.WasReleasedThisFrame();
        public bool EquipSecondaryReleasedThisFrame => InputActions.Gameplay.EquipSecondary.WasReleasedThisFrame();
        public bool EquipGadgetReleasedThisFrame => InputActions.Gameplay.EquipGadget.WasReleasedThisFrame();
        public bool EquipPreviousReleasedThisFrame => InputActions.Gameplay.EquipPrevious.WasReleasedThisFrame();
        public bool PrimaryFireReleasedThisFrame => InputActions.Gameplay.PrimaryFire.WasReleasedThisFrame();
        public bool SecondaryFireReleasedThisFrame => InputActions.Gameplay.SecondaryFire.WasReleasedThisFrame();
        public bool WeaponAbilityReleasedThisFrame => InputActions.Gameplay.WeaponAbility.WasReleasedThisFrame();
        public bool ThrowGrenadeReleasedThisFrame => InputActions.Gameplay.ThrowGrenade.WasReleasedThisFrame();
        public bool UltimateAbilityReleasedThisFrame => InputActions.Gameplay.UltimateAbility.WasReleasedThisFrame();
        public bool ReloadReleasedThisFrame => InputActions.Gameplay.Reload.WasReleasedThisFrame();
        public bool InteractReleasedThisFrame => InputActions.Gameplay.Interact.WasReleasedThisFrame();
        
        public void EnableActions() {
            if (InputActions == null) {
                InputActions = new PlayerInputActions();
                InputActions.Gameplay.SetCallbacks(this);
            }
            
            InputActions.Enable();
        }
        
        public void DisableActions() {
            InputActions?.Disable();
        }
        
        public void OnLook(InputAction.CallbackContext context) {
            Look?.Invoke(context.ReadValue<Vector2>());
        }
        public void OnLookController(InputAction.CallbackContext context) {
            Look?.Invoke(context.ReadValue<Vector2>());
        }
        public void OnMove(InputAction.CallbackContext context) {
            Move?.Invoke(context.ReadValue<Vector2>());
        }
        public void OnJump(InputAction.CallbackContext context) {
            switch (context.phase) {
                case InputActionPhase.Started:
                    Jump?.Invoke(ButtonPhase.Pressed);
                    break;
                case InputActionPhase.Canceled:
                    Jump?.Invoke(ButtonPhase.Released);
                    break;
            }
        }
        public void OnCrouch(InputAction.CallbackContext context) {
            switch (context.phase) {
                case InputActionPhase.Started:
                    Crouch?.Invoke(ButtonPhase.Pressed);
                    break;
                case InputActionPhase.Canceled:
                    Crouch?.Invoke(ButtonPhase.Released);
                    break;
            }
        }
        public void OnMovementAbility(InputAction.CallbackContext context) {
            switch (context.phase) {
                case InputActionPhase.Started:
                    MovementAbility?.Invoke(ButtonPhase.Pressed);
                    break;
                case InputActionPhase.Canceled:
                    MovementAbility?.Invoke(ButtonPhase.Released);
                    break;
            }
        }

        public void OnEquipMelee(InputAction.CallbackContext context) {
            if (context.phase != InputActionPhase.Started)
                return;
                
            EquipMelee?.Invoke();
        }

        public void OnEquipPrimary(InputAction.CallbackContext context) {
            if (context.phase != InputActionPhase.Started)
                return;

            EquipPrimary?.Invoke();
        }
        public void OnEquipSecondary(InputAction.CallbackContext context) {
            if (context.phase != InputActionPhase.Started)
                return;

            EquipSecondary?.Invoke();
        }

        public void OnEquipGadget(InputAction.CallbackContext context) {
            if (context.phase != InputActionPhase.Started)
                return;

            EquipGadget?.Invoke();
        }

        public void OnEquipPrevious(InputAction.CallbackContext context) {
            if (context.phase != InputActionPhase.Started)
                return;

            EquipPrevious?.Invoke();
        }
        public void OnPrimaryFire(InputAction.CallbackContext context) {
            switch (context.phase) {
                case InputActionPhase.Started:
                    PrimaryFire?.Invoke(ButtonPhase.Pressed);
                    break;
                case InputActionPhase.Canceled:
                    PrimaryFire?.Invoke(ButtonPhase.Released);
                    break;
            }
        }
        public void OnSecondaryFire(InputAction.CallbackContext context) {
            switch (context.phase) {
                case InputActionPhase.Started:
                    SecondaryFire?.Invoke(ButtonPhase.Pressed);
                    break;
                case InputActionPhase.Canceled:
                    SecondaryFire?.Invoke(ButtonPhase.Released);
                    break;
            }
        }
        public void OnWeaponAbility(InputAction.CallbackContext context) {
            switch (context.phase) {
                case InputActionPhase.Started:
                    WeaponAbility?.Invoke(ButtonPhase.Pressed);
                    break;
                case InputActionPhase.Canceled:
                    WeaponAbility?.Invoke(ButtonPhase.Released);
                    break;
            }
        }

        public void OnThrowGrenade(InputAction.CallbackContext context) {
            switch (context.phase) {
                case InputActionPhase.Started:
                    ThrowGrenade?.Invoke(ButtonPhase.Pressed);
                    break;
                case InputActionPhase.Canceled:
                    ThrowGrenade?.Invoke(ButtonPhase.Released);
                    break;
            }
        }
        public void OnUltimateAbility(InputAction.CallbackContext context) {
            switch (context.phase) {
                case InputActionPhase.Started:
                    UltimateAbility?.Invoke(ButtonPhase.Pressed);
                    break;
                case InputActionPhase.Canceled:
                    UltimateAbility?.Invoke(ButtonPhase.Released);
                    break;
            }
        }

        public void OnReload(InputAction.CallbackContext context) {
            switch (context.phase) {
                case InputActionPhase.Started:
                    Reload?.Invoke(ButtonPhase.Pressed);
                    break;
                case InputActionPhase.Canceled:
                    Reload?.Invoke(ButtonPhase.Released);
                    break;
            }
        }
        public void OnInteract(InputAction.CallbackContext context) {
            if (context.phase != InputActionPhase.Started)
                return;

            Interact?.Invoke();;
        }

        public void Dispose() {
            DisableActions();
            InputActions?.Dispose();
        }
    }
}