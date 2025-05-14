using PlayerSystems.Movement;
using UnityEngine;

namespace PlayerSystems.Modules.MovementModules {
    [CreateAssetMenu(fileName = "new GroundedMovementModule", menuName = "Player/Modules/Movement/GroundedMovement", order = 0)]
    public class GroundedMovementModule : MovementModule {
        [SerializeField, Range(0,2)] float walkSpeedScale = 1f;
        [SerializeField, Range(0,2)] float crouchSpeedScale = 0.44f;
        [Space]
        [SerializeField] float walkResponse = 25f;
        [SerializeField] float crouchResponse = 20f;
        
        float WalkSpeed => Player.Movement.Speed * walkSpeedScale;
        float CrouchSpeed => Player.Movement.Speed * crouchSpeedScale;
        
        protected override void Initialize() { }
        public override ModuleLevel ModuleLevel => ModuleLevel.BaseModule;

        public override bool ShouldActivate => Player.Motor.GroundingStatus.IsStableOnGround;

        public override void ModuleUpdate() {
            
        }

        public override void EnableModule() {
            base.EnableModule();

            var downwardVelocity = Player.Movement.GetState().Velocity.y * -1f;
            downwardVelocity = Mathf.Max(downwardVelocity, 0);

            Player.Movement.BaseModuleVelocityUpdate += Movement;
        }

        public override void DisableModule() {
            base.DisableModule();

            Player.Movement.BaseModuleVelocityUpdate -= Movement;
        }

        void Movement(ref Vector3 currentVelocity, float deltaTime) {
            var state = Player.Movement.GetState();
            
            if (state.Stance is Stance.Slide)
                return;
            
            var groundedMovement = Player.Motor.GetDirectionTangentToSurface(
                direction: RequestedMovement,
                surfaceNormal: Player.Motor.GroundingStatus.GroundNormal
            ) * RequestedMovement.magnitude;
            
            // Calculate movement speed and response based on stance
            var speed = state.Stance is Stance.Stand ? WalkSpeed : CrouchSpeed;
            var response = state.Stance is Stance.Stand ? walkResponse : crouchResponse;
            //speed *= 1 /*+ _stats.Speed * 0.01f*/;
            //speed *= SpeedMultiplier;

            var targetVelocity = groundedMovement * speed;
            var moveVelocity = Vector3.Lerp(
                a: currentVelocity,
                b: targetVelocity,
                t: 1f - Mathf.Exp(-response * deltaTime)
            );

            // Play footstep sound based on movement speed and time passed
            //var moveSpeed = moveVelocity.magnitude;
            // if (moveSpeed > 0f)
            // {
            //     timeSinceLastFootstep += deltaTime;
            //     if (timeSinceLastFootstep > footstepFrequency / moveSpeed)
            //     {
            //         timeSinceLastFootstep = 0f;
            //         PlayFootstepSound();
            //     }
            // }

            // Calculate acceleration TODO: Move to PlayerMovement
            // _state.Acceleration = (moveVelocity - currentVelocity) / deltaTime;

            currentVelocity = moveVelocity;
        }
    }
}
    
    