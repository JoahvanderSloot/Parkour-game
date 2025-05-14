using UnityEngine;

namespace PlayerSystems.Modules.MovementModules {
    [CreateAssetMenu(fileName = "new AirMovementModule", menuName = "Player/Modules/Movement/AirMovement", order = 0)]
    public class AirMovementModule : MovementModule {
        [SerializeField, Range(0, 2)] float airSpeedScale = 0.86f;
        [SerializeField] float airAcceleration = 100f;
        [SerializeField, Range(0f, 5f)] float crouchHeldGravityScale = 1.8f;
        
        float AirSpeed => Player.Movement.BaseSpeed * airSpeedScale;
        float Gravity => Player.Movement.Gravity;

        float JumpSustainGravity => jumpModule != null ? jumpModule.JumpSustainGravity : 1f;
        
        JumpModule jumpModule;

        protected override void Initialize() {
            if (Player.Modules.TryGetModule<JumpModule>(out var module)) {
                jumpModule = module;
            }
            
            Player.Modules.OnModuleAdded += ModuleAdded;
        }
        public override ModuleLevel ModuleLevel => ModuleLevel.BaseModule;

        public override bool ShouldActivate => !Player.Motor.GroundingStatus.IsStableOnGround;
        public override void ModuleUpdate() {

        }

        public override void EnableModule() {
            base.EnableModule();
            
            Player.Movement.BaseModuleVelocityUpdate += Movement;
        }

        public override void DisableModule() {
            base.DisableModule();
            
            Player.Movement.BaseModuleVelocityUpdate -= Movement;
        }
        
        void ModuleAdded(IPlayerModule addedModule) {
            if (addedModule is JumpModule module) {
                jumpModule = module;
            }
        }
        
        void Movement(ref Vector3 currentVelocity, float deltaTime) {
            // If requesting movement in air, move in air
            if (RequestedMovement.sqrMagnitude > 0f) {
                MoveInAir(ref currentVelocity, deltaTime);
            }

            ApplyGravity(ref currentVelocity, deltaTime);
        }
        
        void MoveInAir(ref Vector3 currentVelocity, float deltaTime) {
            var planarMovement = Vector3.ProjectOnPlane (
                vector: RequestedMovement,
                planeNormal: Player.Motor.CharacterUp
            ) * RequestedMovement.magnitude;

            // Current velocity on movement plane
            var currentPlanarVelocity = Vector3.ProjectOnPlane (
                vector: currentVelocity,
                planeNormal: Player.Motor.CharacterUp
            );

            // Calculate movement force
            var movementForce = planarMovement * (airAcceleration * deltaTime);

            var calculatedAirSpeed = AirSpeed * (1 /*+ 1_stats.Speed * 0.01f*/);
            //calculatedAirSpeed *= SpeedMultiplier;
            // If moving slower than max air speed, treat movement force as steering force
            if (currentPlanarVelocity.magnitude < calculatedAirSpeed) {
                var targetPlanarVelocity = currentPlanarVelocity + movementForce;

                targetPlanarVelocity = Vector3.ClampMagnitude(targetPlanarVelocity, calculatedAirSpeed);
                movementForce = targetPlanarVelocity - currentPlanarVelocity;
            }
            // Otherwise, nerf the movement force when it's in the direction of the current planar velocity
            // to prevent accelerating beyond max air speed
            else if (Vector3.Dot(currentPlanarVelocity, movementForce) > 0f) {
                var constrainedMovementForce = Vector3.ProjectOnPlane (
                    vector: movementForce,
                    planeNormal: currentPlanarVelocity.normalized
                );

                movementForce = constrainedMovementForce;
            }

            // Prevent air-climbing steep slopes
            if (Player.Motor.GroundingStatus.FoundAnyGround) {
                if (Vector3.Dot(movementForce, currentVelocity + movementForce) > 0f) {
                    var obstructionNormal = Vector3.Cross (
                        Player.Motor.CharacterUp,
                        Vector3.Cross(Player.Motor.CharacterUp, Player.Motor.GroundingStatus.GroundNormal)
                    ).normalized;

                    movementForce = Vector3.ProjectOnPlane(movementForce, obstructionNormal);
                }
            }

            currentVelocity += movementForce;
        }
        
        void ApplyGravity(ref Vector3 currentVelocity, float deltaTime) {
            var effectiveGravity = -Gravity;
            
            if (Input.CrouchPressed)
                effectiveGravity *= crouchHeldGravityScale;
            
            var verticalSpeed = Vector3.Dot(currentVelocity, Player.Motor.CharacterUp);
            
            if (Input.JumpPressed && verticalSpeed > 0f) {
                effectiveGravity *= JumpSustainGravity;
            }

            currentVelocity += Player.Motor.CharacterUp * (effectiveGravity * deltaTime);
        }
    }
}