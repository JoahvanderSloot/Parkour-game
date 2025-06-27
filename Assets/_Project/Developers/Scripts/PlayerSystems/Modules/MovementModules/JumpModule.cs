using ImprovedTimers;
using PlayerSystems.Input;
using Unity.VisualScripting;
using UnityEngine;

namespace PlayerSystems.Modules.MovementModules {
    [CreateAssetMenu(fileName = "new JumpModule", menuName = "Player/Modules/Movement/Jump", order = 0)]
    public class JumpModule : MovementModule {
        [SerializeField] float jumpStrength = 10f;
        [SerializeField] float coyoteTime = 0.15f;
        [SerializeField] float jumpBuffer = 0.15f;
        [SerializeField] int airJumps = 1;
        [Range(0, 1)] [SerializeField] float jumpSustainGravity = 0.5f;

        public float JumpSustainGravity => jumpSustainGravity;

        int airJumpsLeft;
        bool requestedJump;
        bool allowJump = true;

        CountdownTimer jumpBufferTimer;

        protected override void Initialize() {
            allowJump = true;
            Input.Jump += HandleJumpInput;
            Player.Movement.OnResetJumps += ResetJumps;

            jumpBufferTimer = new CountdownTimer(jumpBuffer);

            jumpBufferTimer.OnTimerStop += () => requestedJump = false;
        }

        void OnDisable() {
            if (Application.isPlaying && Player != null) {
                Input.Jump -= HandleJumpInput;
                Player.Movement.OnResetJumps -= ResetJumps;

                jumpBufferTimer.Dispose();
            }
        }

        void HandleJumpInput(ButtonPhase phase) {
            if (phase is not ButtonPhase.Pressed)
                return;

            if (!CanRequestJump())
                return;

            requestedJump = true;
            jumpBufferTimer.Reset(jumpBuffer);
            jumpBufferTimer.Start();
        }

        bool CanRequestJump() {
            if (Player.Modules.ActiveModule is null)
                return true;

            return Player.Modules.ActiveModule is MovementModule activeMovementModule
                   && CanOverride(activeMovementModule);
        }

        public override ModuleLevel ModuleLevel => ModuleLevel.ManualActivationModule;
        public override bool ShouldActivate => ShouldJump();

        bool ShouldJump() {
            return requestedJump && CanJump();
        }

        bool CanJump() {
            var grounded = Player.Motor.GroundingStatus.IsStableOnGround;
            var canCoyoteJump = Player.Movement.TimeSinceUngrounded < coyoteTime &&
                                !Player.Movement.UngroundedDueToJump;
            return airJumpsLeft > 0 || grounded || canCoyoteJump;
        }

        public override void ModuleUpdate() { }

        public override void EnableModule() {
            base.EnableModule();
            Player.Movement.VelocityUpdate += Jump;
        }

        public override void DisableModule() {
            base.DisableModule();

            Player.Movement.VelocityUpdate -= Jump;
        }

        void ResetJumps() {
            airJumpsLeft = airJumps;
        }

        // ReSharper disable Unity.PerformanceAnalysis
        void Jump(ref Vector3 currentVelocity, float deltaTime) {
            var grounded = Player.Motor.GroundingStatus.IsStableOnGround;
            var canCoyoteJump = Player.Movement.TimeSinceUngrounded < coyoteTime &&
                                !Player.Movement.UngroundedDueToJump;

            if (grounded || canCoyoteJump) {
                airJumpsLeft = airJumps;
                Player.Motor.ForceUnground(0f);
                Player.Movement.UngroundedDueToJump = true;
                DoJump(ref currentVelocity, true);
            }
            else if (airJumpsLeft > 0) {
                airJumpsLeft--;
                DoJump(ref currentVelocity, false);
            }
            else {
                Debug.LogError("Jump activated but cannot jump");
                requestedJump = false;
                jumpBufferTimer.Stop();
                DisableModule();
            }

            return;

            void DoJump(ref Vector3 currentVelocity, bool wasGrounded) {
                AudioManager.Instance.Play("Jump");
                requestedJump = false;
                jumpBufferTimer.Stop();

                // requestedCrouch = false;
                // _requestedCrouchInAir = false;
                // _requireNewCrouchInput = true;

                // Set minimum vertical speed to jump speed
                var currentVerticalSpeed = Vector3.Dot(currentVelocity, Player.Motor.CharacterUp);
                var targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, jumpStrength);
                // Add jump speed to the velocity
                currentVelocity += Player.Motor.CharacterUp * (targetVerticalSpeed - currentVerticalSpeed);


                DisableModule();
            }
        }
    }
}