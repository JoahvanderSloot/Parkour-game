using PlayerSystems.Input;
using PlayerSystems.Modules;
using UnityEngine;

namespace PlayerSystems.EnvironmentalObjects.Lache {
    [CreateAssetMenu(fileName = "LacheMovementModule", menuName = "Player/Modules/Environmental/LacheModule", order = 0)]
    public class LacheMovementModule : MovementModule {
        [SerializeField] float checkDistance = 1f;
        [SerializeField] float checkRadius = 0.5f;
        [SerializeField] LayerMask lacheLayerMask;
        [Space]
        [SerializeField] float verticalJumpStrength = 10f;
        [SerializeField] float horizontalJumpStrength = 5f;
        [Space]
        [SerializeField] float distanceToLache = 2f;
        [SerializeField] float initialForceMultiplier = 0.5f;
        [SerializeField] float acceleration = 0.5f;
        [SerializeField] float accelerationFactor = 0.1f;
        [Space]
        [SerializeField] float minimumAcceleration = 0.01f;
        [SerializeField] float maximumAcceleration = 1f;
        [Space]
        [SerializeField] float ropeClingResponse = 10f;

        bool interactWasPressed;
        RaycastHit latestHit;
        Lache lache;
        Vector3 attachmentPoint;
        bool attached;

        Vector3 previousDirection;
        float accelerationBooster;
        float directionSimilarityFactor;

        bool interactIsPressed;
        bool jumpRequested;

        float swingAngle;
        float swingVelocity;
        float swingAcceleration;
        
        public override ModuleLevel ModuleLevel => ModuleLevel.ManualActivationModule;
        public override bool ShouldActivate => TryingToGrabLache() && CheckForLache();
        
        public override void ModuleUpdate() {
            
        }
        
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
                DetachFromLache();
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

            // ReSharper disable once Unity.UnknownTag
            return gotHit && latestHit.collider.CompareTag(Lache.c_LacheTag);
        }
        
        public override void EnableModule() {
            base.EnableModule();
            
            if (!latestHit.collider.TryGetComponent(out lache)) {
                Debug.LogError("No lache found");
                DisableModule();
                return;
            }
            
            attachmentPoint = lache.GetAttachmentPoint(latestHit);
            jumpRequested = false;
            
            Player.Movement.VelocityUpdate += MoveOnLache;
            Input.Jump += OnJump;
        }
        
        public override void DisableModule() {
            base.DisableModule();
            
            Player.Movement.VelocityUpdate -= MoveOnLache;
            Input.Jump -= OnJump;
            
            if (attached)
                DetachFromLache();
            
            lache = null;
        }

        void AttachToLache(Lache lache, Vector3 currentVelocity) {
            if (attached)
                return;
            
            attached = true;
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
            var horizontalForce = RequestedMovement * horizontalJumpStrength;
            
            // Add jump speed to the velocity
            var forceToAdd = Player.Motor.CharacterUp * (targetVerticalSpeed - currentVerticalSpeed) + horizontalForce;
            
            currentVelocity += forceToAdd;
        }

        void MoveOnLache(ref Vector3 currentVelocity, float deltaTime) {
            if (!attached)
                AttachToLache(lache, currentVelocity);
            
            if (jumpRequested) {
                JumpOffLache(ref currentVelocity);
                return;
            }
            
            // TODO: Apply gravity to acceleration
            // TODO: Apply player input to acceleration

            swingVelocity += swingAcceleration;
            var desiredAngle = swingAngle + swingVelocity;
            
            Vector3 barDirection = lache.BarDirection;
            Vector3 reference = Vector3.Cross(barDirection, Player.Motor.CharacterUp);
            if (reference == Vector3.zero)
                reference = Vector3.Cross(barDirection, Player.Motor.CharacterRight);
            
            Vector3 circlePoint = attachmentPoint + 
                Quaternion.AngleAxis(desiredAngle, barDirection) * (reference.normalized * distanceToLache);
            
            currentVelocity += circlePoint - Player.Motor.TransientPosition;
        }
    }
}