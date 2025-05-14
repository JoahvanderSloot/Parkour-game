using System;
using UnityEngine;

namespace PlayerSystems.Input
{ 
    [Obsolete]
    public enum CrouchInput
    {
        None, Toggle, Held,// TODO Add Hold 
    }
    [Obsolete]
    public struct MovementInput
    {
        public Quaternion Rotation;
        public Vector2 Move;
        public bool Jump;
        public bool JumpSustain;
        public CrouchInput Crouch;
    }
    [Obsolete]
    public struct CombatInput
    {
        public bool EquipPrimary;
        public bool EquipSecondary;
        public bool EquipPrevious;
        
        public bool Reload;
        
        public bool PrimaryFirePressed;
        public bool SecondaryFirePressed;
        public bool WeaponAbilityPressed;
        
        public bool PrimaryFireReleased;
        public bool SecondaryFireReleased;
        public bool WeaponAbilityReleased;
        
        public bool MovementAbility;
    }
    [Obsolete]
    public struct CameraInput
    {
        public Vector2 Look;
    }
    
    [Obsolete]
    public class PlayerInput : IDisposable
    {
        private readonly PlayerInputActions inputActions;
        private readonly Transform cameraTransform;
    
        public static PlayerInput CreateAndInitialize(Transform playerCameraTransform)
        {
            var inputActions = new PlayerInputActions();
            inputActions.Enable();
            var playerInput = new PlayerInput(inputActions, playerCameraTransform);
            return playerInput;
        }
    
        private PlayerInput(PlayerInputActions inputActions, Transform playerCameraTransform)
        {
            this.inputActions = inputActions;
            cameraTransform = playerCameraTransform;
        }
        
        public Vector3 GetInputDirection() // TODO: This is not rotation independent (always returns direction on a flat plane in world space)
        {
            var input = inputActions.Gameplay;
            var move = input.Move.ReadValue<Vector2>();
            var moveDirection = new Vector3(move.x, 0, move.y);
            Vector3.ClampMagnitude(moveDirection, 1);
            return cameraTransform.rotation * moveDirection;
        }
        
        public MovementInput GetEmptyMovementInput()
        {
            return new MovementInput
            {
                Rotation = cameraTransform.rotation,
                Move = Vector2.zero,
                Jump = false,
                JumpSustain = false,
                Crouch = CrouchInput.None,
            };
        }
    
        public MovementInput GetMovementInput()
        {
            var input = inputActions.Gameplay;
            return new MovementInput
            {
                Rotation        = cameraTransform.rotation,
                Move            = input.Move.ReadValue<Vector2>(),
                Jump            = input.Jump.WasPressedThisFrame(),
                JumpSustain     = input.Jump.IsPressed(),
                Crouch          = false /*Settings.CrouchToggle*/
                    ? input.Crouch.WasPressedThisFrame() ? CrouchInput.Toggle : CrouchInput.None
                    : input.Crouch.IsPressed() ? CrouchInput.Held : CrouchInput.None
            };
        }

        public CombatInput GetCombatInput()
        {
            var input = inputActions.Gameplay;
            return new CombatInput
            {
                EquipPrimary             = input.EquipPrimary.WasPressedThisFrame(),
                EquipSecondary           = input.EquipSecondary.WasPressedThisFrame(),
                EquipPrevious            = input.EquipPrevious.WasPressedThisFrame(),
                
                Reload                   = input.Reload.WasPressedThisFrame(),
                
                PrimaryFirePressed       = input.PrimaryFire.WasPressedThisFrame(),
                SecondaryFirePressed     = input.SecondaryFire.WasPressedThisFrame(),
                WeaponAbilityPressed     = input.WeaponAbility.WasPressedThisFrame(),
                
                PrimaryFireReleased      = input.PrimaryFire.WasReleasedThisFrame(),
                SecondaryFireReleased    = input.SecondaryFire.WasReleasedThisFrame(),
                WeaponAbilityReleased    = input.WeaponAbility.WasReleasedThisFrame(),
                
                MovementAbility          = input.MovementAbility.WasPressedThisFrame()
            };
        }
    
        public CameraInput GetCameraInput()
        {
            var input = inputActions.Gameplay;
            
            var look = input.Look.ReadValue<Vector2>();
            if (look == Vector2.zero)
                look = input.LookController.ReadValue<Vector2>() * 50f;
            
            return new CameraInput { Look = look };
        }
        
        public bool InteractPressed()
        {
            return inputActions.Gameplay.Interact.WasPressedThisFrame();
        }
        
        public bool GetCancelInput()
        {
            MovementInput movementInput = GetMovementInput();
            CombatInput combatInput = GetCombatInput();
            bool movementCancel = movementInput.Jump || movementInput.Crouch != CrouchInput.None;
            bool combatCancel = combatInput.PrimaryFirePressed || combatInput.SecondaryFirePressed || combatInput.WeaponAbilityPressed || combatInput.MovementAbility;
            return movementCancel || combatCancel;
        }

        public void Dispose()
        {
            inputActions?.Dispose();
        }
    }
}