using System;
using ImprovedTimers;
using KinematicCharacterController;
using PlayerSystems.Input;
using PlayerSystems.Modules.MovementModules;
using PlayerSystems.Movement.Debugging;
using UnityEngine;

namespace PlayerSystems.Movement {
    public delegate void VelocityUpdate(ref Vector3 velocity, float deltaTime);
    public delegate void RotationUpdate(ref Quaternion currentRotation, float deltaTime);

    public enum Stance {
        Stand, Crouch, Slide
    }

    public struct CharacterState {
        public bool Grounded;
        public Stance Stance;
        public Vector3 Velocity;
        public Vector3 Acceleration;
    }

    public class PlayerMovement : MonoBehaviour, ICharacterController {
        public event VelocityUpdate BaseModuleVelocityUpdate;
        public event VelocityUpdate VelocityUpdate;
        public event VelocityUpdate LateVelocityUpdate;
        public event VelocityUpdate ExternalVelocityUpdate;
        public event VelocityUpdate AfterExternalVelocityUpdate;
        
        public event Action BeforeVelocityUpdate;
        public event Action<Collider, Vector3, Vector3, HitStabilityReport> OnBecameGrounded;
        
        //public static event Action OnRotationUpdated;
        public event Action OnRotationUpdated;
        
        public event Action OnResetJumps;
        
        [SerializeField] float baseMovementSpeed = 8f;
        [Space]
        [SerializeField] KinematicCharacterMotor motor;
        [SerializeField] Transform root;
        [SerializeField] Transform cameraTarget;
        [Space]
        [SerializeField] float uncrouchInAirAfterSeconds = 0.3f;
        [Space]
        [SerializeField] float standHeight = 2f;
        [SerializeField] float crouchHeight = 1f;
        [SerializeField] float stanceHeightResponse = 15f;
        [Range(0, 1)]
        [SerializeField] float standCameraHeight = 0.9f;
        [Range(0, 1)]
        [SerializeField] float crouchCameraHeight = 0.7f;
        [Space]
        [SerializeField] float gravity = 50f;

        //private Stats _stats;
        
        CharacterState state;
        CharacterState lastState;
        CharacterState tempState;
        
        Vector3 requestedMovement;
        bool _requestedCrouch;
        bool _requireNewCrouchInput;

        bool _velocitySet;
        Vector3 _setVelocity;
        
        CountdownTimer stunTimer;
        
        //private bool MovementLocked => false /*_playerMediator.MovementLocked*/;
        
        private Collider[] heightChangeOverlapResults;
        
        private CountdownTimer _requireNewCrouchInputTimer;

        PlayerController player;
        
        // TODO: Crouch handling
        
        float targetCameraHeight;
        
        GroundedMovementModule groundedMovementModule;
        
        public float BaseSpeed => baseMovementSpeed;
        public float Speed => baseMovementSpeed;
        
        public bool Moving => new Vector3(player.GameplayInput.MoveInputDirection.x, 0, player.GameplayInput.MoveInputDirection.y).magnitude > 0f;
        public float TimeSinceUngrounded { get; private set; }
        public bool UngroundedDueToJump { get; set; }
        public bool RequestedCrouchInAir { get; set; }
        public bool OnWall { get; set; }
        public Vector3 RequestedMovementDirection => requestedMovement;
        public float Gravity => gravity;
        
        public Transform GetCameraTarget() => cameraTarget;
        public CharacterState GetState() => state;
        public CharacterState GetLastState() => lastState;
        public KinematicCharacterMotor GetMotor() => motor;

#if UNITY_EDITOR
        [Header("Debugging")]
        [SerializeField] private StateUI stateUI; // TODO: Move to playercontroller
#endif
        public void Initialize(PlayerController playerController) {
            player = playerController;
            targetCameraHeight = standCameraHeight;
            
            //_playerMediator = ServiceLocator.For(this).Get<PlayerMediator>();
            //_abilityManager = _playerMediator.PlayerAbilityManager; // TODO: Dash needed
            //_stats = _playerMediator.Stats;
            
            state.Stance = Stance.Stand;
            heightChangeOverlapResults = new Collider[8];

            // TODO ?
            _requireNewCrouchInputTimer = new CountdownTimer(uncrouchInAirAfterSeconds);
            _requireNewCrouchInputTimer.OnTimerStop += () => _requireNewCrouchInput = true;
            
            stunTimer = new CountdownTimer(0);
            
            motor.CharacterController = this;
        }
        
        public void UpdateMovement(float deltaTime) {
            //UpdateInput();
            UpdateBody(deltaTime);
        }
        
        public void UpdateInput(MovementInput input)
        {
            // Take the 2D input vector and convert it into 3D movement vector on the XZ plane
            // Then Clamp the magnitude to 1 and rotate it relative to the direction player is facing
            requestedMovement = new Vector3(
                player.GameplayInput.MoveInputDirection.x, 
                0, 
                player.GameplayInput.MoveInputDirection.y
            );
            requestedMovement = Vector3.ClampMagnitude(requestedMovement, 1);
            requestedMovement = player.CameraTransform.rotation * requestedMovement;

            var wasRequestingCrouch = _requestedCrouch;

            switch (state.Grounded) {
                case false when lastState.Grounded && _requestedCrouch:
                    _requireNewCrouchInputTimer.Reset(uncrouchInAirAfterSeconds);
                    _requireNewCrouchInputTimer.Start();
                    break;
                case true when _requireNewCrouchInputTimer.IsRunning:
                    _requireNewCrouchInputTimer.Pause();
                    break;
            }

            // Old crouch input handling
            {
                // if (false /*Settings.CrouchToggle*/) {
                //     switch (input.Crouch) {
                //         case CrouchInput.Toggle when _requireNewCrouchInputTimer.IsRunning:
                //             _requireNewCrouchInputTimer.Pause();
                //             _requestedCrouch = true;
                //             break;
                //         case CrouchInput.Toggle:
                //             _requestedCrouch = !_requestedCrouch;
                //             _requireNewCrouchInput = false;
                //             break;
                //         case CrouchInput.None when _requireNewCrouchInput:
                //             _requestedCrouch = false;
                //             break;
                //         case CrouchInput.None:
                //         case CrouchInput.Held:
                //         default:
                //             break;
                //     }
                // }
                // else {
                switch (input.Crouch) {
                    case CrouchInput.Held when !_requireNewCrouchInput:
                        _requestedCrouch = true;
                        break;
                    case CrouchInput.Held when _requireNewCrouchInput:
                        _requestedCrouch = false;
                        break;
                    case CrouchInput.None:
                        _requestedCrouch = false;
                        _requireNewCrouchInput = false;
                        if (_requireNewCrouchInputTimer.IsRunning)
                            _requireNewCrouchInputTimer.Pause();
                        break;
                    case CrouchInput.Toggle:
                        //DBug.LogWarning("Sending crouch toggle when toggle is not enabled");
                        break;
                    default:
                        _requestedCrouch = false;
                        break;
                }
                // }
            }
            
            
            // New crouch input handling TODO: Implement
            // {
            //     if (false /*Settings.CrouchToggle*/) {
            //         switch (input.Crouch) {
            //             case CrouchInput.Toggle when _requireNewCrouchInputTimer.IsRunning:
            //                 _requireNewCrouchInputTimer.Pause();
            //                 _requestedCrouch = true;
            //                 break;
            //             case CrouchInput.Toggle:
            //                 _requestedCrouch = !_requestedCrouch;
            //                 _requireNewCrouchInput = false;
            //                 break;
            //             case CrouchInput.None when _requireNewCrouchInput:
            //                 _requestedCrouch = false;
            //                 break;
            //             case CrouchInput.None:
            //             case CrouchInput.Held:
            //             default:
            //                 break;
            //         }
            //     }
            //     else {
            //         switch (input.Crouch) {
            //             case CrouchInput.Held when !_requireNewCrouchInput:
            //                 _requestedCrouch = true;
            //                 break;
            //             case CrouchInput.Held when _requireNewCrouchInput:
            //                 _requestedCrouch = false;
            //                 break;
            //             case CrouchInput.None:
            //                 _requestedCrouch = false;
            //                 _requireNewCrouchInput = false;
            //                 if (_requireNewCrouchInputTimer.IsRunning)
            //                     _requireNewCrouchInputTimer.Pause();
            //                 break;
            //             case CrouchInput.Toggle:
            //                 //DBug.LogWarning("Sending crouch toggle when toggle is not enabled");
            //                 break;
            //             default:
            //                 _requestedCrouch = false;
            //                 break;
            //         }
            //     }
            // }

            RequestedCrouchInAir = _requestedCrouch switch {
                true when !wasRequestingCrouch => !state.Grounded,
                false when wasRequestingCrouch => false,
                _ => RequestedCrouchInAir
            };
        }

        public void UpdateBody(float deltaTime) {
            // TODO Update height and camera target based target height and cameraHeight
            
            if (state.Stance is Stance.Crouch && !Mathf.Approximately(motor.Capsule.height, crouchHeight)) {
                SetHeight(crouchHeight);
                SetCameraHeight(crouchCameraHeight);
            }
            else if (state.Stance is Stance.Stand && !Mathf.Approximately(motor.Capsule.height, standHeight)) {
                SetHeight(standHeight);
                SetCameraHeight(standCameraHeight);
            }
            
            var currentHeight = motor.Capsule.height;
            var normalizedHeight = currentHeight / standHeight;

            var cameraTargetHeight = currentHeight * targetCameraHeight;
            var rootTargetScale = new Vector3(1, normalizedHeight, 1);

            cameraTarget.localPosition = Vector3.Lerp (
                a: cameraTarget.localPosition,
                b: Vector3.up * cameraTargetHeight,
                t: 1f - Mathf.Exp(-stanceHeightResponse * deltaTime)
            );

            root.localScale = Vector3.Lerp (
                a: root.localScale,
                b: rootTargetScale,
                t: 1f - Mathf.Exp(-stanceHeightResponse * deltaTime)
            );
        }

        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime) {
            if (stunTimer.IsRunning) {
                WhileStunned(ref currentVelocity, deltaTime);
                return;
            }

            BeforeVelocityUpdate?.Invoke();
            state.Acceleration = Vector3.zero;
            
            var startingVelocity = currentVelocity;
            
            // If the velocity is set by something, update current velocity and return
            if (_velocitySet) {
                _velocitySet = false;
                state.Acceleration = (_setVelocity - currentVelocity) / deltaTime;
                currentVelocity = _setVelocity;
                return;
            }

            if (motor.GroundingStatus.IsStableOnGround) {
                TimeSinceUngrounded = 0f;
                UngroundedDueToJump = false;
            }
            else {
                TimeSinceUngrounded += deltaTime;
            }
            
            BaseModuleVelocityUpdate?.Invoke(ref currentVelocity, deltaTime);
            state.Acceleration = (currentVelocity - startingVelocity) / deltaTime;
            VelocityUpdate?.Invoke(ref currentVelocity, deltaTime);
            state.Acceleration = (currentVelocity - startingVelocity) / deltaTime;
            LateVelocityUpdate?.Invoke(ref currentVelocity, deltaTime);
            state.Acceleration = (currentVelocity - startingVelocity) / deltaTime;
            
            ExternalVelocityUpdate?.Invoke(ref currentVelocity, deltaTime);
            state.Acceleration = (currentVelocity - startingVelocity) / deltaTime;
            AfterExternalVelocityUpdate?.Invoke(ref currentVelocity, deltaTime);
            state.Acceleration = (currentVelocity - startingVelocity) / deltaTime;
            
            // Update active ability
            //_abilityManager.PlayerVelocityUpdate(ref currentVelocity, deltaTime); TODO: Dash needed
            
            state.Acceleration = (currentVelocity - startingVelocity) / deltaTime;
        }
        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime) {
            // Update the character's rotation to face in the same direction as the requested rotation (camera rotation)
            // Then flatten the rotation so that the character only rotates on the Y axis
            var newForward = Vector3.ProjectOnPlane(player.CameraTransform.rotation * Vector3.forward, motor.CharacterUp);
            var difference = Vector3.SignedAngle(motor.CharacterForward, newForward, motor.CharacterUp);
            
            if (newForward != Vector3.zero)
                currentRotation = Quaternion.AngleAxis(difference, motor.CharacterUp) * currentRotation;
            
            OnRotationUpdated?.Invoke();
        }

        public void BeforeCharacterUpdate(float deltaTime) {
            // if (_lastState.Stance is Stance.Slide && _state.Stance is not Stance.Slide && slideSoundEmitter)
            // {
            //     slideSoundEmitter.Stop();
            //     slideSoundEmitter = null;
            // }
            
            tempState = state;

            //if (MovementLocked) return;
            
            // Crouch
            if (_requestedCrouch && state.Stance is Stance.Stand) {
                SetStance(Stance.Crouch);
            }
        }
        public void PostGroundingUpdate(float deltaTime) {
            if (!motor.GroundingStatus.IsStableOnGround && state.Stance is Stance.Slide) {
                state.Stance = Stance.Crouch;
                //_requestedCrouch = false;
                //motor.SetCapsuleDimensions(motor.Capsule.radius, standHeight, standHeight * 0.5f);
            }
        }
        public void AfterCharacterUpdate(float deltaTime) {
            // If we are not requesting to crouch, and we are not standing, un-crouch
            if (!_requestedCrouch && state.Stance is not Stance.Stand) {
                SetStance(Stance.Stand);
            }
            
            // Calculate state acceleration based on velocity change since last frame and time passed
            var totalAcceleration = (state.Velocity - lastState.Velocity) / deltaTime;
            state.Acceleration = Vector3.ClampMagnitude(state.Acceleration, totalAcceleration.magnitude);

            // Update state after all calculations
            state.Grounded = motor.GroundingStatus.IsStableOnGround;
            state.Velocity = motor.Velocity;
            lastState = tempState;

#if UNITY_EDITOR
            if (stateUI) stateUI.UpdateStateUI(state);
#endif
        }

        // Gets called every update if stable on ground
        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) {
            // if (hitCollider.TryGetComponent(out KillBox killBox)) {
            //     motor.SetPosition(killBox.RespawnPoint);
            // }
            
            if (!motor.LastGroundingStatus.IsStableOnGround && motor.GroundingStatus.IsStableOnGround) {
                // Became Grounded
                
                // Play landing sound
                
                OnBecameGrounded?.Invoke(hitCollider, hitNormal, hitPoint, hitStabilityReport);
                InvokeOnResetJumps();
            }
        }
        // Called when the motor detects a hit for example walking in to a wall
        // (keeps getting called if player keeps walking towards a wall)
        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) {
            
        }

        public bool IsColliderValidForCollisions(Collider coll) {
            return true;
        }
        public void OnDiscreteCollisionDetected(Collider hitCollider) {
            Debug.LogWarning("Discrete Collision Detected. (PLEASE TELL LASSI IF YOU SEE THIS MESSAGE and try to figure out when it happens)");
        }
        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport) {

        }
        
        public void SetVelocityForNextUpdate(Vector3 velocity) {
            _velocitySet = true;
            _setVelocity = velocity;
        }
        public void AddVelocity(Vector3 velocity) {
            motor.BaseVelocity += velocity;
        }
        
        public bool SetStance(Stance stance, float height = 0, float cameraHeight = 0, bool forceStance = false) {
            if (forceStance)
                state.Stance = stance;
            
            if (height == 0) {
                height = stance switch {
                    Stance.Stand => standHeight,
                    Stance.Crouch => crouchHeight,
                    Stance.Slide => crouchHeight,
                    _ => standHeight
                };
            }
            
            if (cameraHeight == 0) {
                cameraHeight = stance switch {
                    Stance.Stand => standCameraHeight,
                    Stance.Crouch => crouchCameraHeight,
                    Stance.Slide => crouchCameraHeight,
                    _ => standCameraHeight
                };
            }

            if (SetHeight(height)) {
                SetCameraHeight(cameraHeight);
                state.Stance = stance;
                return true;
            }
            
            return false;
        }
        
        // ReSharper disable Unity.PerformanceAnalysis
        public bool SetHeight(float height) {
            if (height < 1f) {
                Debug.LogError("Heights lower than 1 are not supported");
                height = 1f;
            }
            
            if (height > motor.Capsule.height) {
                var originalHeight = motor.Capsule.height;
                
                // Tentatively "standup"
                motor.SetCapsuleDimensions(motor.Capsule.radius, height, height * 0.5f);

                // Then see if we can actually stand up without hitting our head
                // If we can't, stay crouching
                var pos = motor.TransientPosition;
                var rot = motor.TransientRotation;
                var mask = motor.CollidableLayers;
                if (motor.CharacterOverlap(pos, rot, heightChangeOverlapResults, mask, QueryTriggerInteraction.Ignore) > 0) {
                    motor.SetCapsuleDimensions(motor.Capsule.radius, originalHeight, originalHeight * 0.5f);
                    return false;
                }
            }
            else {
                motor.SetCapsuleDimensions(motor.Capsule.radius, height, height * 0.5f);
            }

            return true;
        }
        
        public void SetCameraHeight(float height) {
            targetCameraHeight = height;
        }

        public void SetPosition(Vector3 position, bool killVelocity = true) {
            motor.SetPosition(position);
            if (killVelocity)
                motor.BaseVelocity = Vector3.zero;
        }
        
        // TODO Move to Unit script
        public void Stun(float duration) {
            stunTimer.Reset(duration);
            stunTimer.Start();
            
            if (state.Stance is Stance.Slide) {
                SetStance(Stance.Crouch);
            }
        }

        void WhileStunned(ref Vector3 currentVelocity, float deltaTime) {
            // TODO: Grab friction from ground surface
            
            if (motor.GroundingStatus.IsStableOnGround) {
                // Apply friction
                currentVelocity = Vector3.Lerp(
                    a: currentVelocity,
                    b: Vector3.zero,
                    t: 1f - Mathf.Exp(-10f * deltaTime)
                );
            }
            else {
                // Apply gravity and slight friction
                currentVelocity += motor.CharacterUp * (-gravity * deltaTime);
                
                currentVelocity = Vector3.Lerp(
                    a: currentVelocity,
                    b: Vector3.zero,
                    t: 1f - Mathf.Exp(-1f * deltaTime)
                );
            }
        }
        
        // TODO: this is very naive implementation maybe change later to also be able to handle different force modes
        public void ApplyForce(Vector3 direction, float force) {
            ApplyForce(direction * force);
        }
        public void ApplyForce(Vector3 force) {
            motor.BaseVelocity += force;
        }
        
        public void InvokeOnResetJumps() {
            OnResetJumps?.Invoke();
        }
    }
}