using Extensions;
using PlayerSystems.Input;
using PlayerSystems.Interaction;
using PlayerSystems.Modules;
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
        
        bool attached;
        RopePart attachedRopePart;

        float accelerationBooster;
        float directionSimilarityFactor;

        bool interactIsPressed;
        bool jumpRequested;
        
        bool interactReleasedAfterDetaching;
        
        bool tryingToGrabRopePart;
        RopePart ropePartToGrab;
        
        protected override void Initialize() {
            Player.InteractionHandler.OnInteract += OnInteract;
            Input.Interact += OnInteractInput;
        }

        void OnDisable() {
            if (Application.isPlaying && Player != null) {
                Player.InteractionHandler.OnInteract -= OnInteract;
                Input.Interact -= OnInteractInput;
            }
        }

        void OnInteract(IInteractable interactable, Vector3 point) {
            Debug.Log($"interactable {interactable} point {point}");
            if (interactable is RopePart ropePart) {
                OnInteractWithRopePart(ropePart);
            }
        }
        public void OnInteractWithRopePart(RopePart ropePart) {
            Debug.Log($"on interact with {ropePart}");
            if (!interactReleasedAfterDetaching)
                return;
            
            tryingToGrabRopePart = true;
            ropePartToGrab = ropePart;
            
            Debug.Log($"Trying to grab {ropePart.name}");
        }

        bool CheckForActivation() {
            var activate = tryingToGrabRopePart;
            tryingToGrabRopePart = false;
            return activate;
        }

        void OnInteractInput(ButtonPhase phase) {
            switch (phase) {
                case ButtonPhase.Pressed:
                    interactIsPressed = true;
                    if (attached) DetachFromRope();
                    break;
                case ButtonPhase.Released:
                    interactIsPressed = false;
                    interactReleasedAfterDetaching = true;
                    break;
                default:
                    break;
            }
        }

        public override ModuleLevel ModuleLevel => ModuleLevel.ManualActivationModule;
        public override bool ShouldActivate => CheckForActivation();

        void OnJump(ButtonPhase phase) {
            if (phase is ButtonPhase.Pressed && attached) {
                jumpRequested = true;
            }
        }
        
        public override void ModuleUpdate() {
            //interactIsPressed = Input.InteractPressed;
        }

        public override void EnableModule() {
            base.EnableModule();
            
            Player.Motor.SetGroundSolvingActivation(false);
            
            if (!ropePartToGrab) {
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
            
            Player.Movement.InvokeOnResetJumps();
            
            Player.Motor.SetGroundSolvingActivation(true);
            
            Player.Movement.VelocityUpdate -= MoveOnRope;
            
            if (attached)
                DetachFromRope();
        }

        void AttachToRope(RopePart ropePart, Vector3 currentVelocity) {
            if (attached)
                return;
            
            attached = true;
            attachedRopePart = ropePart;
            ropePart.AddForce(currentVelocity * initialForceMultiplier);
        }
        
        void DetachFromRope() {
            attached = false;
            
            if (interactIsPressed)
                interactReleasedAfterDetaching = false;
            
            attachedRopePart.RopeVerlet.ResetGravity();
            attachedRopePart = null;
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
                AttachToRope(ropePartToGrab, currentVelocity);

            if (jumpRequested) {
                JumpOffRope(ref currentVelocity);
                return;
            }
            
            attachedRopePart.RopeVerlet.SetGravityDirection(-Player.Motor.CharacterUp);
            
            var currentSpeed = currentVelocity.magnitude;
            var ropeVelocity = attachedRopePart.Velocity;
            currentVelocity = Vector3.zero;
            
            // Add velocity to the rope part
            var directionToTopOfRope = (attachedRopePart.RopeVerlet.StartPoint - attachedRopePart.CurrentPosition).normalized;
            var movementDirection = Player.Motor.GetDirectionTangentToSurface(
                direction: RequestedMovement,
                surfaceNormal: directionToTopOfRope
            ) * RequestedMovement.magnitude;
            
            Debug.DrawRay(attachedRopePart.CurrentPosition, directionToTopOfRope * 5, Color.red, 0.1f);

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
                attachedRopePart.AddForce(movementDirection * (accelerationToApply * accelerationMultiplier), forceUpdateSpread);
            }

            // Cling to the rope
            var targetPosition = attachedRopePart.CurrentPosition;
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