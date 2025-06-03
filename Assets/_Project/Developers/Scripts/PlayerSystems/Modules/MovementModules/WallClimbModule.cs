using System.Diagnostics;
using ImprovedTimers;
using PlayerSystems.Input;
using PlayerSystems.Movement;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace PlayerSystems.Modules.MovementModules {
    [CreateAssetMenu(fileName = "WallClimbModule", menuName = "Player/Modules/Movement/WallClimb", order = 0)]
    public class WallClimbModule : MovementModule {
        [SerializeField] float wallClimbSpeedScale = 1f;
        [SerializeField] float wallRunResponse = 10f;
        [SerializeField] float wallClimbDuration = 2f;
        [Space]
        [SerializeField] float jumpOutForce = 10f;
        [SerializeField] float jumpUpForce = 10f;
        [SerializeField] float coyoteTime = 0.2f;
        [Space]
        [SerializeField] Vector2 minMaxBankAngle = new (75, 100);
        [SerializeField] float wallCheckDistance = 1f;
        [SerializeField, Range(0,1)] float wallCheckHeight = 0.25f;
        [SerializeField] float minDistanceFromGround = 1f;
        [Space]
        [SerializeField] float cooldownAfterDetaching = 0.1f;
        [SerializeField] float cooldownAfterJump = 0.2f;
        [SerializeField] float cooldownAfterStanceChange = 0.4f;
        [SerializeField] float cooldownAfterTimeOut = 0.7f;
        [SerializeField] float cooldownOnSimilarWall = 1f;
        [Space] 
        [SerializeField] LayerMask wallLayers;
        
        [Header("Camera Tilt")]
        [SerializeField] Vector3 cameraTilt = new (10f, 0f, 0f);
        [SerializeField] float tiltInResponse = 5f;
        [SerializeField] float tiltOutResponse = 5f;
        [Header("Camera Offset")]
        [SerializeField] Vector3 cameraOffset = new (0f, 0f, -0.1f);
        [SerializeField] float cameraOffsetInResponse = 5f;
        [SerializeField] float cameraOffsetOutResponse = 5f;
        
        float WallClimbSpeed => Player.Movement.Speed * wallClimbSpeedScale;
        
        bool hasWall;
        RaycastHit wallHit;
        RaycastHit previousHit;

        bool uniqueWall;

        CountdownTimer unUniqueWallAttachmentTimer;
        CountdownTimer cooldownTimer;
        CountdownTimer jumpBufferTimer;
        CountdownTimer wallClimbTimer;

        bool detachFromWall;
        bool jumpRequested;
        
        Vector3 WallCheckPosition => Player.Motor.TransientPosition + Player.Motor.CharacterUp * (Player.Height * wallCheckHeight);

        protected override void Initialize() {
            unUniqueWallAttachmentTimer = new CountdownTimer(cooldownOnSimilarWall);
            cooldownTimer = new CountdownTimer(cooldownAfterJump);
            jumpBufferTimer = new CountdownTimer(coyoteTime);
            
            wallClimbTimer = new CountdownTimer(wallClimbDuration);
            wallClimbTimer.OnTimerStop += () => {
                if (!Enabled)
                    return;
                
                cooldownTimer.Reset(cooldownAfterTimeOut);
                cooldownTimer.Start();
                DetachFromWall();
            };
        }
        
        public override void ModuleUpdate() {
            
        }

        public override ModuleLevel ModuleLevel => ModuleLevel.AutomaticActivationModule;
        public override bool AllowBaseModuleActivation => true;

        public override bool ShouldActivate => ShouldAttachToWall();

        public override void EnableModule() {
            base.EnableModule();
            
            wallClimbTimer.Reset(wallClimbDuration);
            wallClimbTimer.Start();

            detachFromWall = false;
            Player.Movement.SetStance(Stance.Stand, forceStance: true);
            Player.Movement.OnWall = true;
            
            jumpRequested = false;
            
            Input.Jump += OnJump;
            Player.Movement.VelocityUpdate += WallClimb;
            
            Player.Movement.InvokeOnResetJumps();
            
            Player.Effects.CameraBob.Enable();
            
            Player.Effects.CameraTilt.SetTilt(cameraTilt, tiltInResponse);
            Player.Effects.CameraOffset.SetOffset(cameraOffset, cameraOffsetInResponse);
        }
        public override void DisableModule() {
            base.DisableModule();
            
            if (!cooldownTimer.IsRunning) {
                cooldownTimer.Reset(cooldownAfterDetaching);
                cooldownTimer.Start();
            }
            
            unUniqueWallAttachmentTimer.Reset(cooldownOnSimilarWall);
            unUniqueWallAttachmentTimer.Start();
            
            Player.Movement.OnWall = false;
            jumpRequested = false;
            
            Input.Jump -= OnJump;
            Player.Movement.VelocityUpdate -= WallClimb;
            Player.Movement.LateVelocityUpdate -= BufferJump;
            
            Player.Effects.CameraBob.Disable();
            Player.Effects.CameraTilt.ResetTilt(tiltOutResponse);
            Player.Effects.CameraOffset.ResetOffset(cameraOffsetOutResponse);
        }

        void DetachFromWall() {
            if (!cooldownTimer.IsRunning) {
                cooldownTimer.Reset(cooldownAfterDetaching);
                cooldownTimer.Start();
            }
            
            unUniqueWallAttachmentTimer.Reset(cooldownOnSimilarWall);
            unUniqueWallAttachmentTimer.Start();
            
            jumpBufferTimer.Reset(coyoteTime);
            jumpBufferTimer.Start();

            Player.Movement.OnWall = false;

            Player.Movement.VelocityUpdate -= WallClimb;
            Player.Movement.LateVelocityUpdate += BufferJump;

            Player.Effects.CameraBob.Disable();
            Player.Effects.CameraTilt.ResetTilt(tiltOutResponse);
            Player.Effects.CameraOffset.ResetOffset(cameraOffsetOutResponse);
        }
        
        void OnJump(ButtonPhase phase) {
            if (phase == ButtonPhase.Pressed)
                jumpRequested = true;
        }

        bool ShouldAttachToWall() {
            if (cooldownTimer.IsRunning)
                return false;
            
            if (!AboveGround())
                return false;
            
            if (!CheckForWall())
                return false;

            if (!uniqueWall && unUniqueWallAttachmentTimer.IsRunning)
                return false;
            
            var notPressingForward = Input.MoveInputDirection.y <= 0;
            if (notPressingForward)
                return false;
            
            if (!IsValidWall(wallHit))
                return false;
            
            return true;
        }
        
        bool CheckForWall() {
            hasWall = Physics.Raycast(WallCheckPosition, Player.Motor.CharacterForward, out wallHit, wallCheckDistance, wallLayers);
            
            uniqueWall = previousHit.normal != wallHit.normal || !hasWall;
            previousHit = wallHit;
            
            DrawDebugLines();
            return hasWall;
        }
        
        bool AboveGround() {
            return !Physics.Raycast(Player.Motor.TransientPosition, -Player.Motor.CharacterUp, minDistanceFromGround);
        }
        
        bool IsValidWall(RaycastHit wallHit) {
            // Check bank angle
            var angle = Vector3.Angle(Player.Motor.CharacterUp, wallHit.normal);
            var angleIsValid = !(angle < minMaxBankAngle.x) && !(angle > minMaxBankAngle.y);
            return angleIsValid;
        }

        bool ShouldDetachFromWall() {
            if (detachFromWall)
                return true;

            var lookingAway = Vector3.Dot(Player.Motor.CharacterForward, wallHit.normal) > -0.75f;
            if (lookingAway)
                return true;

            var strafingAway = Input.MoveInputDirection.y < 0f;
            if (strafingAway)
                return true;

            var notPressingForward = Input.MoveInputDirection.y <= 0;
            if (notPressingForward)
                return true;
            
            var wrongStance = Player.Movement.GetState().Stance is not Stance.Stand;
            if (wrongStance) {
                cooldownTimer.Reset(cooldownAfterStanceChange);
                cooldownTimer.Start();
                return true;
            }

            if (Player.Movement.GetState().Velocity.magnitude < 0.1f)
                return true;

            if (!CheckForWall())
                return true;
            
            if (!IsValidWall(wallHit))
                return true;
            
            return false;
        }
        
        void WallClimb(ref Vector3 currentVelocity, float deltaTime) {
            Vector3 wallNormal = wallHit.normal;
            var wallUpwards = Vector3.ProjectOnPlane(Player.Motor.CharacterUp, wallNormal).normalized;

            currentVelocity = new Vector3(currentVelocity.x, wallUpwards.y * WallClimbSpeed, currentVelocity.z);
            
            var targetVelocity = Player.Motor.GetDirectionTangentToSurface(
                direction: currentVelocity,
                surfaceNormal: wallNormal
            ) * WallClimbSpeed;
            
            var moveVelocity = Vector3.Lerp(
                a: currentVelocity,
                b: targetVelocity,
                t: 1f - Mathf.Exp(-wallRunResponse * deltaTime)
            );
            
            currentVelocity = moveVelocity;
            
            // Pull player toward wall
            currentVelocity += -wallNormal * (100 * deltaTime);
            
            if (jumpRequested) {
                JumpOut(ref currentVelocity);
                DisableModule();
            }
            
            if (ShouldDetachFromWall()) {
                DetachFromWall();
            }
        }

        void BufferJump(ref Vector3 currentVelocity, float deltaTime) {
            if (jumpBufferTimer.IsFinished)
                DisableModule();
            
            if (!jumpRequested)
                return;
            
            jumpRequested = false;
            JumpOut(ref currentVelocity);
            DisableModule();
        }
        
        void JumpOut(ref Vector3 currentVelocity) {
            var currentVerticalSpeed = Vector3.Dot(currentVelocity, Player.Motor.CharacterUp);
            var targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, jumpUpForce);
            var verticalForce = Player.Motor.CharacterUp * (targetVerticalSpeed - currentVerticalSpeed);
            
            var horizontalForce = wallHit.normal * jumpOutForce;
            
            currentVelocity += verticalForce + horizontalForce;
            
            cooldownTimer.Reset(cooldownAfterJump);
            cooldownTimer.Start();
            
            detachFromWall = true;
            jumpRequested = false;
        }
        
        [Conditional("UNITY_EDITOR")]
        void DrawDebugLines() {
            Debug.DrawLine(WallCheckPosition, Player.Motor.CharacterRight * wallCheckDistance, Color.blue);
            Debug.DrawLine(WallCheckPosition, -Player.Motor.CharacterRight * wallCheckDistance, Color.blue);
        }
    }
}