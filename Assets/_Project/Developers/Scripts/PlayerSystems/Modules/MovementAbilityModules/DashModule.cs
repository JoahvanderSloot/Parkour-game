using ImprovedTimers;
using PlayerSystems.Movement;
using UnityEngine;

namespace PlayerSystems.Modules.MovementAbilityModules {
    [CreateAssetMenu(fileName = "new Dash_Module", menuName = "Player/Modules/MovementAbility/Dash", order = 0)]
    public class DashModule : MovementAbilityModule {
        [Space]
        [SerializeField] float dashSpeedScale = 2.65f; 
        [SerializeField] float maxDashSpeed = 50f;
        [Space]
        //[SerializeField] float smoothing = 0.33f;
        [SerializeField] float dashDuration = 0.15f;
        [SerializeField, Range(0,1)] float retainedSpeed = 0.33f;
        [Space]
        [Tooltip("How much does vertical speed count towards dash speed calculation")]
        [SerializeField] float verticalSpeedFactor = 0.25f;
        
        float DashSpeed => Mathf.Min(Player.Movement.Speed * dashSpeedScale, maxDashSpeed);
        
        protected override void Initialize() {
            CannotBeOverridden = true;
            dashTimer = new CountdownTimer(dashDuration);
        }

        public override ModuleLevel ModuleLevel => ModuleLevel.ManualActivationModule;
        //public override bool ShouldActivate { get; }
        public override void ModuleUpdate() {
            
        }
        
        float speedBeforeDash;
        Vector3 lastVelocity;
        bool dashing;
        
        CountdownTimer dashTimer;
        
        // 13.5 was the original speed value
        float ScaledSpeed => DashSpeed * GetDashVelocity01();

        float GetDashVelocity01() {
            return 1f;
            //if (smoothing <= 0f) return 1f;
            
            // TODO check if this is correct until then return 1
            
            // var smoothUntil = smoothing / 2f;
            // return dashTimer.Progress < 0.5f
            //     ? Mathf.SmoothStep(0f, 1f, dashTimer.Progress / smoothUntil)
            //     : Mathf.SmoothStep(1f, 0f, Mathf.Clamp01((dashTimer.Progress - (1f - smoothUntil)) / smoothUntil));
        }

        public override void EnableModule() {
            base.EnableModule();

            Player.Movement.VelocityUpdate += PlayerVelocityUpdate;
        }
        
        public override void DisableModule() {
            base.DisableModule();
            dashing = false;
            Player.Movement.VelocityUpdate -= PlayerVelocityUpdate;
        }

        public void PlayerVelocityUpdate(ref Vector3 currentVelocity, float deltaTime) {
            WhileDashing(ref currentVelocity);
            lastVelocity = currentVelocity;
        }

        // protected void StartAbility() {
        //     // SoundManager.Instance.CreateSound()
        //     //     .WithPosition(playerMediator.PlayerTransform.position)
        //     //     .WithSoundData(dashConfig.DashSound)
        //     //     .Play();
        //     
        //     speedBeforeDash = Player.Movement.GetState().Velocity.magnitude;
        //     //playerMediator.PlayerWeaponManager.CurrentWeapon.Dash();
        //     StartDash();
        //     dashing = true;
        //     
        //     //Combo.Increase(this, 0);
        // }

        void StartDash(ref Vector3 currentVelocity) {
            dashing = true;
            dashTimer.Reset(dashDuration);
            dashTimer.Start();

            // Get the current horizontal speed
            var velocity = Player.Movement.GetState().Velocity;
            var horizontalSpeed = Mathf.Sqrt(velocity.x * velocity.x + velocity.z * velocity.z);
            var verticalSpeed = velocity.y;
            speedBeforeDash = horizontalSpeed + verticalSpeed * verticalSpeedFactor;
            
            var moveDirection = new Vector3(Input.MoveInputDirection.x, 0, Input.MoveInputDirection.y);
            Vector3.ClampMagnitude(moveDirection, 1);
            var inputDirection = Player.CameraTransform.rotation * moveDirection; // TODO: This is not rotation independent (always returns direction on a flat plane in world space)
        
            var planarInputDirection = Vector3.ProjectOnPlane (
                vector: inputDirection,
                planeNormal: Player.Motor.CharacterUp
            ) * inputDirection.magnitude;

            // Set dash direction to movement direction if moving, otherwise to character forward
            var dashDirection = planarInputDirection.sqrMagnitude == 0f ? Player.Motor.CharacterForward : planarInputDirection;

            // Check the ground angle and unground if going downhill
            var groundAngle = Vector3.Angle(dashDirection, Player.Motor.GroundingStatus.GroundNormal);
            if (groundAngle < 90f) {
                Player.Motor.ForceUnground(0f);
            }
            else {
                // If not going downhill calculate dash direction along the ground
                dashDirection = Player.Motor.GetDirectionTangentToSurface(
                    direction: dashDirection,
                    surfaceNormal: Player.Motor.GroundingStatus.GroundNormal
                );
            }
            
            var speed = Mathf.Max(speedBeforeDash, ScaledSpeed);
            var dashVelocity = dashDirection * Mathf.Max(speed, 1);
            lastVelocity = dashVelocity;
            currentVelocity = dashVelocity;
            
            //Player.Events.Publish(new OnDash(currentVelocity));
        }

        void WhileDashing(ref Vector3 currentVelocity) {
            if (!dashing) {
                StartDash(ref currentVelocity);
                return;
            }
            
            if (dashTimer.IsFinished) {
                EndDash(ref currentVelocity);
                return;
            }
            
            var dashDirection = Vector3.ProjectOnPlane(currentVelocity, Player.Motor.CharacterUp).normalized;
        
            // Check the ground angle and unground if going downhill
            var groundAngle = Vector3.Angle(dashDirection, Player.Motor.GroundingStatus.GroundNormal);
            switch (Player.Motor.GroundingStatus.IsStableOnGround) {
                case true when groundAngle < 90f:
                    Player.Motor.ForceUnground(0f);
                    break;
                case true:
                    // If not going downhill calculate dash direction along the ground
                    dashDirection = Player.Motor.GetDirectionTangentToSurface
                    (
                        direction: dashDirection,
                        surfaceNormal: Player.Motor.GroundingStatus.GroundNormal
                    );
                    break;
            }

            var speed = Mathf.Max(speedBeforeDash, ScaledSpeed);
            var dashVelocity = dashDirection * speed;
            currentVelocity = dashVelocity;
        }
    
        void EndDash(ref Vector3 currentVelocity) {
            //_state.MovementAbilityActive = false;
            //if (_state.Stance is not Stance.Stand) UnCrouch();

            // If we are grounded set velocity to walk speed else to air speed in direction of current velocity
            if (Player.Motor.GroundingStatus.IsStableOnGround) {
                Vector3 velocityToSet = Vector3.ProjectOnPlane(lastVelocity, Player.Motor.GroundingStatus.GroundNormal).normalized * Mathf.Max(lastVelocity.magnitude * retainedSpeed, speedBeforeDash);
                currentVelocity = velocityToSet;
            }
            else {
                Vector3 velocityToSet = Vector3.ProjectOnPlane(lastVelocity, Player.Motor.CharacterUp).normalized * Mathf.Max(lastVelocity.magnitude * retainedSpeed, speedBeforeDash);
                currentVelocity = velocityToSet;
            }

            Player.Movement.SetStance(Stance.Stand);
            DisableModule();
        }
    }
}