using System;
using KinematicCharacterController;
using PlayerSystems.Input;
using PlayerSystems.Interaction;
using PlayerSystems.Modules;
using PlayerSystems.Movement;
using PlayerSystems.Movement.CameraEffects;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using PlayerInput = PlayerSystems.Input.PlayerInput;

namespace PlayerSystems {
    public class PlayerController : MonoBehaviour {
        public static PlayerController s_ActivePlayer;
        
        [SerializeField] private Transform playerHeadTransform;

        [SerializeField] private PlayerMovement playerMovement;

        //[FormerlySerializedAs("player")] [SerializeField] private PlayerUnit playerUnit;
        [SerializeField] private PlayerCamera playerCamera;

        [SerializeField] private Camera mainCamera;

        //[SerializeField] private PlayerAbilityManager playerAbilityManager;
        //[SerializeField] private PlayerWeaponManager playerWeaponManager;
        [FormerlySerializedAs("playerCameraEffects")] [SerializeField]
        private PlayerFX playerFX;
        public PlayerFX Effects => playerFX;

        //private InteractionSystem.InteractionHandler interactionHandler;
        private PlayerInput oldInput;
        
        [SerializeField] InteractionHandler interactionHandler;
        public InteractionHandler InteractionHandler => interactionHandler;
        
        [SerializeField] GameplayInputReader gameplayInputReader;
        public GameplayInputReader GameplayInput => gameplayInputReader;
        [Space] [SerializeField] PlayerModules playerModules;
        public PlayerModules Modules => playerModules;
        
        [Space]
        [SerializeField] PlayerHandsAnimations playerHandsAnimations;
        
        public KinematicCharacterMotor Motor => playerMovement.GetMotor();
        public PlayerMovement Movement => playerMovement;

        public Camera MainCamera => mainCamera;

        public Transform CameraTransform => playerCamera.transform;
        public Vector3 CameraPosition => playerCamera.transform.position;

        public Transform Transform => Movement.transform;
        public float Height => Motor.Capsule.height;
        public Vector3 CenterPosition => Motor.Capsule.center;
        public Vector3 BottomPosition => Transform.position;
        public Vector3 TopPosition => Motor.TransientPosition + Motor.CharacterUp * Height;
        public Vector3 HeadPosition => playerHeadTransform.position;
        public Vector3 TransientCenterPosition => Motor.TransientPosition + Motor.CharacterUp * Height * 0.5f;
        public Vector3 TransientBottomPosition => Motor.TransientPosition;
        public Vector3 TransientTopPosition => Motor.TransientPosition + Motor.CharacterUp * Height;
        
        void Start() {
            Initialize();
        }

        public void Initialize() {
            Cursor.lockState = CursorLockMode.Locked;

            gameplayInputReader.EnableActions();
            
            playerMovement.Initialize(this);
            playerCamera.Initialize(playerMovement.GetCameraTarget(), this);
            playerFX.Initialize(this, mainCamera);
            interactionHandler.Initialize(this);

            MainCamera.gameObject.SetActive(true);
            oldInput = PlayerInput.CreateAndInitialize(playerCamera.transform);

            playerModules.InitializeModules(this);

            playerHandsAnimations.Initialize(this);
            
            s_ActivePlayer = this;
        }

        void OnDestroy() {
            oldInput.Dispose();
            gameplayInputReader.Dispose();
        }

        void Teleport(Vector3 position) {
            playerMovement.SetPosition(position);
        }

        public void Update() {
            var deltaTime = Time.deltaTime;

            // playerUnit.UpdateUnit(deltaTime);
            //
            // interactionHandler.CheckForInteractables();
            // if (input.InteractPressed())
            //     interactionHandler.Interact();

            playerCamera.UpdateRotation(gameplayInputReader);

            var combatInput = oldInput.GetCombatInput();
            var movementInput = false /*PlayerMediator.MovementLocked*/
                ? oldInput.GetEmptyMovementInput()
                : oldInput.GetMovementInput();

            //if (input.GetCancelInput())
            //playerAbilityManager.CancelAbilities(); TODO: Call Dash Directly

            //if (!PlayerMediator.LookLocked)

            playerMovement.UpdateInput(movementInput);
            playerMovement.UpdateBody(deltaTime);

            // if (!PlayerMediator.CombatLocked)
            // {
            //     playerWeaponManager.UpdateInput(combatInput);
            //
            //     if (combatInput.MovementAbility)
            //         playerAbilityManager.ExecuteMovementAbility();
            // }

            //playerAbilityManager.UpdateAbilities(deltaTime); TODO: Call Dash Directly

            //var weapon = playerWeaponManager.CurrentWeapon;
            // weapon.SetGrounded(playerMovement.GetMotor().GroundingStatus.IsStableOnGround);
            // weapon.SetSliding(playerMovement.GetState().Stance is Stance.Slide);
            // var speed = playerMovement.GetState().Velocity.magnitude;
            // var clampedSpeed = Mathf.Clamp(speed, 1, 100);
            // weapon.SetSpeed(clampedSpeed);

            playerModules.UpdateModules();

#if UNITY_EDITOR
            if (Keyboard.current.tKey.wasPressedThisFrame) {
                var ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
                if (Physics.Raycast(ray, out var hit)) {
                    Teleport(hit.point);
                }
            }
#endif
        }

        public void LateUpdate() {
            var deltaTime = Time.deltaTime;

            playerCamera.UpdatePosition();
            playerFX.UpdateEffects(deltaTime);
            //playerAbilityManager.LateUpdateAbilities(deltaTime); TODO: Call Dash Directly
        }
    }
}