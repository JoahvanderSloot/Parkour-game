using Extensions;
using PlayerSystems.Input;
using PlayerSystems.Modules;
using PrimeTween;
using UnityEngine;

namespace PlayerSystems.EnvironmentalObjects.Lache {
    [CreateAssetMenu(fileName = "LacheMovementModule", menuName = "Player/Modules/Environmental/LacheModule", order = 0)]
    public class LacheMovementModule : MovementModule {
        [Header("Interaction")]
        [SerializeField] float checkDistance = 2f;
        [SerializeField] float checkRadius = 0.5f;
        [SerializeField] LayerMask lacheLayerMask;
        
        [Header("Attachment")] 
        [SerializeField] float initialForceMultiplier = 25f;
        
        [Header("Detachment")]
        [SerializeField] float detachForceMultiplier = 100f;
        [SerializeField] float verticalJumpStrength = 10f;
        [SerializeField] float horizontalJumpStrength = 5f;
        
        [Header("Movement")]
        [SerializeField] float distanceToLache = 1.5f;
        [SerializeField] float acceleration = 120f;
        [SerializeField] float downwardsFriction = 0.05f;
        [SerializeField] float upwardsFriction = 1f;
        [SerializeField] float gravity = 989f;
        [Space]
        [SerializeField] float maximumAcceleration = 1000f;
        [SerializeField] float maximumVelocity = 500f;
        [SerializeField] float maximumAngle = 0f;
        [Space]
        [SerializeField] float swingPositionResponse = 20f;
        [Space]
        [SerializeField] TweenSettings<Vector3> attachmentOffsetTweenSettings;
        
#if UNITY_EDITOR
        [Header("Debugging")]
        [SerializeField] bool enableLogs = false;
        [SerializeField] bool drawSwing = false;
#endif

        Vector3 gravityDirection;

        bool interactWasPressed;
        RaycastHit latestHit;
        Lache attachedLache;
        Vector3 attachmentPoint;
        bool attached;

        Vector3 previousDirection;
        float accelerationBooster;
        float directionSimilarityFactor;

        bool interactIsPressed;
        bool jumpRequested;
        bool shouldDetach;

        float swingAngle;
        float swingVelocity;
        float swingAcceleration;

        Vector3 previousPosition;

        Vector3 positionOffset;

        public override ModuleLevel ModuleLevel => ModuleLevel.ManualActivationModule;
        public override bool ShouldActivate => TryingToGrabLache() && CheckForLache();

        public override void ModuleUpdate() { }

        protected override void Initialize() {
            Input.Interact += OnInteractPressed;
        }

        void OnDisable() {
            if (Application.isPlaying && Player != null) {
                Input.Interact -= OnInteractPressed;
            }
        }

        void OnInteractPressed() {
            if (attached)
                shouldDetach = true;
            else
                interactWasPressed = true;
        }

        void OnJump(ButtonPhase phase) {
            if (phase is ButtonPhase.Pressed && attached) {
                jumpRequested = true;
            }
        }

        bool TryingToGrabLache() {
            if (!interactWasPressed)
                return false;

            interactWasPressed = false;
            return true;
        }

        bool CheckForLache() {
            var gotHit = Physics.SphereCast(
                Player.MainCamera.transform.position,
                checkRadius,
                Player.MainCamera.transform.forward,
                out latestHit,
                checkDistance,
                lacheLayerMask,
                QueryTriggerInteraction.Collide
            );

            return gotHit && latestHit.collider.CompareTag(Lache.c_LacheTag);
        }

        public override void EnableModule() {
            base.EnableModule();

            shouldDetach = false;
            Player.Motor.SetGroundSolvingActivation(false);

            if (!latestHit.collider.TryGetComponent(out attachedLache)) {
                Debug.LogError("No lache found");
                DisableModule();
                return;
            }

            attachmentPoint = attachedLache.GetAttachmentPoint(latestHit.point);
            jumpRequested = false;

            Player.Movement.VelocityUpdate += MoveOnLache;
            Input.Jump += OnJump;
        }

        public override void DisableModule() {
            base.DisableModule();

            shouldDetach = false;
            Player.Motor.SetGroundSolvingActivation(true);

            Player.Movement.VelocityUpdate -= MoveOnLache;
            Input.Jump -= OnJump;

            if (attached)
                DetachFromLache();

            attachedLache = null;
        }

        void AttachToLache(Lache lache, Vector3 currentVelocity, out float initialForce) {
            initialForce = 0f;

            if (attached)
                return;

            attached = true;

            var barDirection = lache.BarDirection;
            var forwardDirection = Vector3.Cross(barDirection, Player.Motor.CharacterUp);

            swingVelocity = 0f;
            swingAcceleration = 0f;
            // Find the closest angle to the position of player
            swingAngle = GetBestFittingSwingAngle(
                barDirection,
                -Player.Motor.CharacterUp,
                Player.TransientTopPosition,
                distanceToLache
            );
            
            var circlePoint = MathPlus.GetPointAroundAxis(
                barDirection,
                attachmentPoint,
                distanceToLache,
                -Player.Motor.CharacterUp,
                swingAngle
            );

            positionOffset = Player.TransientTopPosition - circlePoint;
            attachmentOffsetTweenSettings.startValue = positionOffset;
            Tween.Custom(attachmentOffsetTweenSettings, value => positionOffset = value);
            
            // apply initial force to swing
            var currentVelocityDot = Vector3.Dot(currentVelocity.normalized, forwardDirection);
            swingVelocity += -currentVelocityDot * currentVelocity.magnitude * initialForceMultiplier;
            
#if UNITY_EDITOR
            if (enableLogs)
                Debug.Log($"attaching to lache at angle {swingAngle} and force {swingVelocity}");
#endif
        }

        float GetBestFittingSwingAngle(
            Vector3 barDirection,
            Vector3 referenceDirection,
            Vector3 targetPosition,
            float swingRadius
        ) {
            // Vector from attachment to target, projected onto swing plane
            Vector3 toTarget = targetPosition - attachmentPoint;
            Vector3 projected = Vector3.ProjectOnPlane(toTarget, barDirection);

            if (projected == Vector3.zero)
                return 0f;

            // Calculate angle between reference and projected vector
            float angle = Vector3.SignedAngle(referenceDirection, projected, barDirection);

            // Optionally, clamp based on swing limits
            // angle = Mathf.Clamp(angle, -maximumAngle, maximumAngle);

            return angle;
        }

        void DetachFromLache() {
            attached = false;
            DisableModule();
        }

        void JumpOffLache(ref Vector3 currentVelocity) {
            DetachFromLache();

            jumpRequested = false;

            // Set minimum vertical speed to jump speed
            var currentVerticalSpeed = Vector3.Dot(currentVelocity, Player.Motor.CharacterUp);
            var targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, verticalJumpStrength);
            
            var flattenedMovement = Vector3.ProjectOnPlane(RequestedMovement, Player.Motor.CharacterUp);
            var horizontalForce = flattenedMovement * horizontalJumpStrength;
            
            // Add jump speed to the velocity
            var forceToAdd = Player.Motor.CharacterUp * (targetVerticalSpeed - currentVerticalSpeed) + horizontalForce;

            currentVelocity += forceToAdd;
            
#if UNITY_EDITOR
            if (enableLogs)
                Debug.Log($"Jumping out with horizontal force {horizontalForce} and vertical speed {targetVerticalSpeed}, Total force {forceToAdd}");
#endif
        }

        void MoveOnLache(ref Vector3 currentVelocity, float deltaTime) {
            var forceToAdd = 0f;

            if (!attached) {
                AttachToLache(attachedLache, currentVelocity, out var initialForce);
                forceToAdd += initialForce;
            }

            currentVelocity = Vector3.zero;

            // Get correct forward direction based on bar direction
            var barDirection = attachedLache.BarDirection;

            var forwardDirection = Vector3.Cross(barDirection, Player.Motor.CharacterUp);
            if (forwardDirection == Vector3.zero)
                forwardDirection = Vector3.Cross(barDirection, Player.Motor.CharacterRight);

            var flattenedMovement = Vector3.ProjectOnPlane(RequestedMovement, Player.Motor.CharacterUp);
            var requestedMovementDot = Vector3.Dot(flattenedMovement, forwardDirection);

            forceToAdd += -requestedMovementDot * acceleration;

            SimulateSwing(deltaTime, forceToAdd);
            
            // Modulate angle so that 0 points downwards
            var angle = swingAngle + 90f;

            var circlePoint = MathPlus.GetPointAroundAxis(
                barDirection,
                attachmentPoint,
                distanceToLache,
                forwardDirection,
                angle
            );

            var differenceOfTopToTransientPosition = Player.Motor.TransientPosition - Player.TransientTopPosition;
            var lerpedPosition = Vector3.Lerp(
                Player.TransientTopPosition,
                circlePoint,
                1f - Mathf.Exp(-swingPositionResponse * deltaTime)
            );

            if (jumpRequested) {
                currentVelocity += (circlePoint - previousPosition) * detachForceMultiplier;
                JumpOffLache(ref currentVelocity);
                return;
            }

            if (shouldDetach) {
                currentVelocity += (circlePoint - previousPosition) * detachForceMultiplier;
                DetachFromLache();
                return;
            }

            previousPosition = circlePoint;

            var desiredPosition = lerpedPosition + differenceOfTopToTransientPosition;
            Player.Motor.SetTransientPosition(desiredPosition + positionOffset);
#if UNITY_EDITOR
            if (enableLogs)
                Debug.Log($"Angle: {swingAngle}, Velocity: {swingVelocity}, Acceleration: {swingAcceleration}");
            
            if (!drawSwing)
                return;
            
            BDebug.DrawSphere(desiredPosition + positionOffset, 0.15f, 0.02f, Color.magenta, 60f, 10f);
            BDebug.DrawSphere(desiredPosition, 0.15f, 0.02f, Color.cyan, 60f, 10f);
            BDebug.DrawSphere(lerpedPosition, 0.15f, 0.02f, Color.green, 60f, 10f);
            BDebug.DrawSphere(circlePoint, 0.15f, 0.02f, Color.red, 60f, 10f);
            BDebug.DrawSphere(attachmentPoint, 0.15f, 0.02f, Color.blue, 60f, 10f);
            Debug.DrawLine(attachmentPoint, circlePoint, Color.yellow, 0.02f);
#endif
        }

        void SimulateSwing(float deltaTime, float additionalForces = 0f) {
            var newSwingAngle = swingAngle + swingVelocity * deltaTime + swingAcceleration * (deltaTime * deltaTime * 0.5f);
            var newSwingAcceleration = ApplyForces(additionalForces);
            var newSwingVelocity = swingVelocity + (swingAcceleration + newSwingAcceleration) * (deltaTime * 0.5f);

            swingAngle = newSwingAngle;
            swingAcceleration = Mathf.Clamp(newSwingAcceleration, -maximumAcceleration, maximumAcceleration);
            swingVelocity = Mathf.Clamp(newSwingVelocity, -maximumVelocity, maximumVelocity);

            if (maximumAngle.Approx(0f))
                return;

            swingAngle = Mathf.Clamp(newSwingAngle, -maximumAngle, maximumAngle);

            if (!Mathf.Abs(swingAngle).Approx(maximumAngle))
                return;

            swingAcceleration = 0f;
            swingVelocity = 0f;

            swingAngle = Mathf.Clamp(newSwingAngle, -maximumAngle * 0.99f, maximumAngle * 0.99f);
        }

        float ApplyForces(float forces = 0f) {
            // Calculate friction based on current velocity and direction
            // whether it's going away from 0 or towards 0
            var directionToZero = Mathf.Sign(swingAngle) * -1f;
            var velocityDirectionToZero = Mathf.Sign(swingVelocity) * -1f;
            var directionSimilarity = Mathf.Clamp01(Mathf.Abs(velocityDirectionToZero - directionToZero));
            var calculatedFriction = Mathf.Lerp(downwardsFriction, upwardsFriction, directionSimilarity);

            // apply constant friction pulling velocity towards 0
            forces -= calculatedFriction * swingVelocity;

            // apply gravity pulling down towards swingAngle 0
            forces -= gravity * Mathf.Sin(swingAngle * Mathf.Deg2Rad);

            return forces;
        }
    }
}