using System;
using Extensions;
using PlayerSystems.Input;
using PlayerSystems.Modules;
using Unity.VisualScripting;
using UnityEngine;

namespace PlayerSystems.EnvironmentalObjects {
    [CreateAssetMenu(fileName = "RopeMovementModule", menuName = "Player/Modules/Environmental/RopeModule", order = 0)]
    public class PlayerRopeMovementModule : MovementModule {
        [SerializeField] float ropeCheckDistance = 1f;
        [SerializeField] float ropeCheckRadius = 0.5f;
        [SerializeField] LayerMask ropeLayerMask;
        [Space]
        [SerializeField] float verticalJumpStrength = 10f;
        [SerializeField] float horizontalJumpStrength = 5f;
        [Space]
        [SerializeField] float initialForceMultiplier = 0.5f;
        [SerializeField] float acceleration = 0.5f;
        [SerializeField] float accelerationFactor = 0.1f;
        [Space]
        [SerializeField] float minimumAcceleration = 0.01f;
        [SerializeField] float maximumAcceleration = 1f;
        [Space]
        [SerializeField] int forceUpdateSpread = 5;
        [Space]
        [SerializeField] float ropeClingResponse = 10f;

        bool interactWasPressed;
        RaycastHit latestHit;
        RopePart ropePart;
        bool attached;

        Vector3 previousDirection;
        float accelerationBooster;
        float directionSimilarityFactor;

        bool interactIsPressed;
        bool jumpRequested;
        
        protected override void Initialize() {
           Input.Interact += OnInteractPressed;
        }

        void OnDisable() {
            if (Application.isPlaying && Player != null) {
                Input.Interact -= OnInteractPressed;
            }
        }

        public override ModuleLevel ModuleLevel => ModuleLevel.ManualActivationModule;
        public override bool ShouldActivate => TryingToGrabRope() && CheckForRope();

        bool CheckForRope() {
            var gotHit = Physics.SphereCast(
                Player.MainCamera.transform.position,
                ropeCheckRadius,
                Player.MainCamera.transform.forward,
                out latestHit,
                ropeCheckDistance,
                ropeLayerMask,
                QueryTriggerInteraction.Collide
            );

            // ReSharper disable once Unity.UnknownTag
            return gotHit && latestHit.collider.CompareTag(Rope.c_RopePartTag);
        }
        
        bool TryingToGrabRope() {
            if (!interactWasPressed)
                return false;
            
            interactWasPressed = false;
            return true;
        }

        void OnInteractPressed() {
            if (attached)
                DetachFromRope();
            else
                interactWasPressed = true;
        }

        void OnJump(ButtonPhase phase) {
            if (phase is ButtonPhase.Pressed && attached) {
                jumpRequested = true;
            }
        }
        
        public override void ModuleUpdate() {
            interactIsPressed = Input.InteractPressed;
        }

        public override void EnableModule() {
            base.EnableModule();
            
            Player.Motor.SetGroundSolvingActivation(false);
            
            if (!latestHit.collider.TryGetComponent(out ropePart)) {
                Debug.LogError("No rope part found");
                DisableModule();
                return;
            }
            
            jumpRequested = false;
            
            Player.Movement.VelocityUpdate += MoveOnRope;

            Input.Jump += OnJump;
        }
        
        public override void DisableModule() {
            base.DisableModule();
            
            Player.Motor.SetGroundSolvingActivation(true);
            
            Player.Movement.VelocityUpdate -= MoveOnRope;
            
            if (attached)
                DetachFromRope();
            
            ropePart = null;
        }

        void AttachToRope(RopePart ropePart, Vector3 currentVelocity) {
            if (attached)
                return;
            
            attached = true;
            ropePart.AddForce(currentVelocity * initialForceMultiplier);
        }
        
        void DetachFromRope() {
            attached = false;
            ropePart.RopeVerlet.ResetGravity();
            DisableModule();
        }

        void JumpOffRope(ref Vector3 currentVelocity) {
            DetachFromRope();
            
            jumpRequested = false;
            
            // Set minimum vertical speed to jump speed
            var currentVerticalSpeed = Vector3.Dot(currentVelocity, Player.Motor.CharacterUp);
            var targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, verticalJumpStrength);
            
            var flattenedMovement = Vector3.ProjectOnPlane(RequestedMovement, Player.Motor.CharacterUp);
            var horizontalForce = flattenedMovement * horizontalJumpStrength;
            
            // Add jump speed to the velocity
            var forceToAdd = Player.Motor.CharacterUp * (targetVerticalSpeed - currentVerticalSpeed) + horizontalForce;
            
            currentVelocity += forceToAdd;
        }
        
        public void MoveOnRope(ref Vector3 currentVelocity, float deltaTime) {
            if (!attached)
                AttachToRope(ropePart, currentVelocity);

            if (jumpRequested) {
                JumpOffRope(ref currentVelocity);
                return;
            }
            
            ropePart.RopeVerlet.SetGravityDirection(-Player.Motor.CharacterUp);
            
            var currentSpeed = currentVelocity.magnitude;
            var ropeVelocity = ropePart.Velocity;
            currentVelocity = Vector3.zero;
            
            // Add velocity to the rope part
            var directionToTopOfRope = (ropePart.RopeVerlet.StartPoint - ropePart.CurrentPosition).normalized;
            var movementDirection = Player.Motor.GetDirectionTangentToSurface(
                direction: RequestedMovement,
                surfaceNormal: directionToTopOfRope
            ) * RequestedMovement.magnitude;
            
            Debug.DrawRay(ropePart.CurrentPosition, directionToTopOfRope * 5, Color.red, 0.1f);

            if (movementDirection != Vector3.zero) {
                var upwardsDot = Vector3.Dot(ropeVelocity.normalized, Player.Motor.CharacterUp);
                var accelerationMultiplier = upwardsDot > 0f
                    ? upwardsDot.MapClamped(0f, 1f, accelerationFactor, 0f)
                    : upwardsDot.MapClamped(-1f, 0f, 1f, accelerationFactor);
                
                var directionDot = Vector3.Dot(ropeVelocity.normalized, movementDirection);
                accelerationBooster += accelerationFactor * directionDot;

                accelerationBooster = directionDot > 0f
                    ? accelerationBooster * directionDot.MapClamped(0f, 1f, accelerationFactor, 1f)
                    : directionDot.MapClamped(-1f, 0f, 0f, accelerationFactor);
                
                accelerationBooster = Mathf.Max(accelerationBooster, 0f);

                var accelerationToApply = Mathf.Max(Mathf.Min(acceleration * accelerationBooster, maximumAcceleration), minimumAcceleration);
                ropePart.AddForce(movementDirection * (accelerationToApply * accelerationMultiplier), forceUpdateSpread);
            }
            
            previousDirection = movementDirection;

            // Cling to the rope
            var targetPosition = ropePart.CurrentPosition;
            var lerpedPosition = Vector3.Lerp(
                Player.Motor.TransientPosition,
                targetPosition,
                1f - Mathf.Exp(-ropeClingResponse * deltaTime)
            );

            // calculate the velocity to apply
            var velocityToApply = (lerpedPosition - Player.Motor.TransientPosition) / deltaTime;
            // apply the velocity to the player
            currentVelocity += velocityToApply;
        }
    }
}