using PlayerSystems.Movement;
using UnityEngine;

namespace PlayerSystems.Modules.MovementModules {
    [CreateAssetMenu(fileName = "new SlideModule", menuName = "Player/Modules/Movement/Slide", order = 0)]
    public class SlideModule : MovementModule {
        [SerializeField, Range(0, 3)] float slideStartSpeedScale = 1.87f;
        [SerializeField, Range(0, 3)] float slideEndSpeedScale = 0.67f;
        [Space]
        [SerializeField] float slideFriction = 0.8f;
        [SerializeField] float slideSteerAcceleration = 5f;
        [SerializeField] float slideGravity = 50f;
        [Space]
        [SerializeField] float slideHeight = 1f;
        [SerializeField, Range(0,1)] float slideCameraHeight = 0.8f;

        float SlideStartSpeed => Player.Movement.Speed * slideStartSpeedScale;
        float SlideEndSpeed => Player.Movement.Speed * slideEndSpeedScale;

        protected override void Initialize() { }
        public override ModuleLevel ModuleLevel => ModuleLevel.AutomaticActivationModule;

        public override bool ShouldActivate => ShouldStartSlide();
        
        bool sliding;
        
        bool ShouldStartSlide() {
            if (!Player.Motor.GroundingStatus.IsStableOnGround)
                return false;
            
            var state = Player.Movement.GetState();
            var lastState = Player.Movement.GetLastState();
            
            return Player.Movement.Moving && state.Stance is Stance.Crouch && (lastState.Stance is Stance.Stand || !lastState.Grounded);
        }
        public override void ModuleUpdate() {

        }

        public override void EnableModule() {
            base.EnableModule();
            
            Player.Movement.VelocityUpdate += Slide;
        }

        public override void DisableModule() {
            base.DisableModule();

            Player.Movement.VelocityUpdate -= Slide;
            sliding = false;
        }
        
        void StartSlide(ref Vector3 currentVelocity, bool wasInAir) {
            sliding = true;
            // slideSoundEmitter = SoundManager.Instance.Get();
            // slideSoundEmitter.Initialize(slideSoundData);
            // slideSoundEmitter.Play();
            //
            // _playerMediator.PlayerWeaponManager.CurrentWeapon.StartSlide();
            Player.Movement.SetStance(Stance.Slide, slideHeight, slideCameraHeight);
            
            var lastState = Player.Movement.GetLastState();

            // When landing on stable ground the character motor projects the velocity onto a flat plane
            // See: KinematicCharacterMotor.HandleVelocityProjection()
            // This is normally good for preventing the character from sliding down slopes when landing
            // But in this case we want the player to slide
            // Re project last frames (falling) velocity onto the current ground normal
            if (wasInAir) {
                currentVelocity = Vector3.ProjectOnPlane(
                    lastState.Velocity,
                    Player.Motor.GroundingStatus.GroundNormal
                );
            }

            var effectiveSlideStartSpeed = SlideStartSpeed /* * stats.speed*/;
            
            // Null slideStartspeed and use current velocity instead if player was in air and requested crouch
            if (!lastState.Grounded && !Player.Movement.RequestedCrouchInAir) {
                effectiveSlideStartSpeed *= 0f;
                Player.Movement.RequestedCrouchInAir = false;
            }

            var slideSpeed = Mathf.Max(effectiveSlideStartSpeed, currentVelocity.magnitude);
            currentVelocity = Player.Motor.GetDirectionTangentToSurface(
                direction: currentVelocity,
                surfaceNormal: Player.Motor.GroundingStatus.GroundNormal
            ) * slideSpeed;
        }
        void Slide(ref Vector3 currentVelocity, float deltaTime) {
            if (!sliding)
                StartSlide(ref currentVelocity, !Player.Movement.GetLastState().Grounded);
            
            //slideSoundEmitter.transform.position = transform.position;
            
            var groundedMovement = Player.Motor.GetDirectionTangentToSurface(
                direction: RequestedMovement,
                surfaceNormal: Player.Motor.GroundingStatus.GroundNormal
            ) * RequestedMovement.magnitude;
            
            // Friction
            currentVelocity -= currentVelocity * (slideFriction * deltaTime);

            // Slope
            {
                var force = Vector3.ProjectOnPlane(
                    vector: -Player.Motor.CharacterUp,
                    planeNormal: Player.Motor.GroundingStatus.GroundNormal
                ) * -slideGravity;

                currentVelocity -= force * deltaTime;
            }

            // Steer
            {
                // Target velocity is player's movement direction * current speed
                var currentSpeed = currentVelocity.magnitude;
                var targetVelocity = groundedMovement * currentVelocity.magnitude;
                var steerVelocity = currentVelocity;
                var steerForce = (targetVelocity - currentVelocity) * (slideSteerAcceleration * deltaTime);
                // Add steer force and clamp the velocity to current speed so slide speed doesn't increase with steering
                steerVelocity += steerForce;
                steerVelocity = Vector3.ClampMagnitude(steerVelocity, currentSpeed);

                // _state.Acceleration = (steerVelocity - currentVelocity) / deltaTime;
                currentVelocity = steerVelocity;
            }

            // End sliding
            if (currentVelocity.magnitude < SlideEndSpeed || Player.Movement.GetState().Stance is Stance.Stand or Stance.Crouch) {
                if (Player.Movement.GetState().Stance is Stance.Slide)
                    Player.Movement.SetStance(Stance.Crouch, forceStance: true);
                
                DisableModule();
            }
        }
    }
}