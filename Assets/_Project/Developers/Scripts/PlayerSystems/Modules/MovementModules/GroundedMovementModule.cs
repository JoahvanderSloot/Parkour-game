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
            
            Player.Effects.CameraBob.Enable();
        }

        public override void DisableModule() {
            base.DisableModule();

            Player.Movement.BaseModuleVelocityUpdate -= Movement;
            
            Player.Effects.CameraBob.Disable();
        }

        void UpdateCameraBobState(Stance stance, float velocity) {
            var cameraBob = Player.Effects.CameraBob;
            
            switch (cameraBob.IsEnabled) {
                case false when stance is Stance.Stand or Stance.Crouch:
                    Player.Effects.CameraBob.Enable();
                    break;
                case true when stance is not Stance.Stand and not Stance.Crouch:
                    Player.Effects.CameraBob.Disable();
                    break;
            }

            switch (velocity) {
                case <= 0.1f when cameraBob.IsEnabled:
                    cameraBob.Disable();
                    break;
                case > 0.1f when !cameraBob.IsEnabled:
                    cameraBob.Enable();
                    break;
            }
        }

        void Movement(ref Vector3 currentVelocity, float deltaTime)
        {
            var state = Player.Movement.GetState();

            UpdateCameraBobState(state.Stance, state.Velocity.magnitude);

            // Check if the player is not grounded
            if (!Player.Motor.GroundingStatus.IsStableOnGround)
            {
                AudioManager.Instance.Stop("Run");
                return;
            }

            if (state.Stance is Stance.Slide)
                return;

            var groundedMovement = Player.Motor.GetDirectionTangentToSurface(
                direction: RequestedMovement,
                surfaceNormal: Player.Motor.GroundingStatus.GroundNormal
            ) * RequestedMovement.magnitude;

            // Calculate movement speed and response based on stance
            var speed = state.Stance is Stance.Stand ? WalkSpeed : CrouchSpeed;
            var response = state.Stance is Stance.Stand ? walkResponse : crouchResponse;

            var targetVelocity = groundedMovement * speed;
            var moveVelocity = Vector3.Lerp(
                a: currentVelocity,
                b: targetVelocity,
                t: 1f - Mathf.Exp(-response * deltaTime)
            );

            // Play or stop footstep sound based on movement speed and time passed
            var moveSpeed = moveVelocity.magnitude;
            if (moveSpeed > 0f)
            {
                if (!AudioManager.Instance.IsPlaying("Run"))
                {
                    AudioManager.Instance.Play("Run");
                }
            }
            else
            {
                AudioManager.Instance.Stop("Run");
            }

            currentVelocity = moveVelocity;
        }
    }
}
    
    