using System;
using System.Collections;
using System.Diagnostics;
using ImprovedTimers;
using PlayerSystems.Input;
using PlayerSystems.Movement;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace PlayerSystems.Modules.MovementModules {
    [CreateAssetMenu(fileName = "new WallRunModule", menuName = "Player/Modules/Movement/WallRun", order = 0)]
    public class WallRunModule : MovementModule {
        [SerializeField, Range(0, 2)] float wallRunSpeedScale = 1.25f;
        [SerializeField] float wallRunResponse = 10f;
        [SerializeField] float wallRunGravity = 1f;
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
        [Space] 
        [SerializeField] LayerMask wallLayers;

        [Header("Camera Tilt")]
        [SerializeField] float cameraTiltZ = 15f;
        [SerializeField] float tiltInResponse = 5f;
        [SerializeField] float tiltOutResponse = 5f;
        [Header("Camera Offset")]
        [SerializeField] float cameraOffsetX = 0.2f;
        [SerializeField] float cameraOffsetInResponse = 5f;
        [SerializeField] float cameraOffsetOutResponse = 5f;
        
        float WallRunSpeed => Player.Movement.Speed * wallRunSpeedScale;
        
        bool wallRight;
        bool wallLeft;
        
        RaycastHit rightWallHit;
        RaycastHit leftWallHit;
        
        RaycastHit attachedWallHit;
        bool attachedWallRight;
        
        CountdownTimer cooldownTimer;
        CountdownTimer jumpBufferTimer;

        bool previousWallRight;
        
        bool detachFromWall;
        
        bool jumpRequested;
        
        Vector3 WallCheckPosition => Player.Motor.TransientPosition + Player.Motor.CharacterUp * (Player.Height * wallCheckHeight);

        protected override void Initialize() {
            cooldownTimer = new CountdownTimer(cooldownAfterJump);
            jumpBufferTimer = new CountdownTimer(coyoteTime);
        }

        public override ModuleLevel ModuleLevel => ModuleLevel.AutomaticActivationModule;

        public override bool ShouldActivate => ShouldAttachToWall();

        public override void EnableModule() {
            base.EnableModule();

            detachFromWall = false;
            Player.Movement.SetStance(Stance.Stand, forceStance: true);
            Player.Movement.OnWall = true;
            
            jumpRequested = false;
            
            Input.Jump += OnJump;
            Player.Movement.VelocityUpdate += WallRun;
            
            Player.Movement.InvokeOnResetJumps();
            
            Player.Effects.CameraBob.Enable();
            
            var tiltZ = attachedWallRight ? cameraTiltZ : -cameraTiltZ;
            Player.Effects.CameraTilt.SetTilt(new Vector3(0f, 0f, tiltZ), tiltInResponse);
            
            var offsetX = attachedWallRight ? -cameraOffsetX : cameraOffsetX;
            Player.Effects.CameraOffset.SetOffset(new Vector3(offsetX, 0f, 0f), cameraOffsetInResponse);
        }
        public override void DisableModule() {
            base.DisableModule();
            
            if (!cooldownTimer.IsRunning) {
                cooldownTimer.Reset(cooldownAfterDetaching);
                cooldownTimer.Start();
            }
            
            Player.Movement.OnWall = false;
            previousWallRight = attachedWallRight;
            jumpRequested = false;
            
            Input.Jump -= OnJump;
            Player.Movement.VelocityUpdate -= WallRun;
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
            
            jumpBufferTimer.Reset(coyoteTime);
            jumpBufferTimer.Start();

            Player.Movement.OnWall = false;
            previousWallRight = attachedWallRight;
            
            Player.Movement.VelocityUpdate -= WallRun;
            Player.Movement.LateVelocityUpdate += BufferJump;

            Player.Effects.CameraBob.Disable();
            Player.Effects.CameraTilt.ResetTilt(tiltOutResponse);
            Player.Effects.CameraOffset.ResetOffset(cameraOffsetOutResponse);
        }
        
        public override void ModuleUpdate() {

        }
        
        void OnJump(ButtonPhase phase) {
            if (phase == ButtonPhase.Pressed)
                jumpRequested = true;
        }

        bool ShouldAttachToWall() {
            if (!AboveGround())
                return false;
            
            if (!CheckForWall())
                return false;

            var notPressingForward = Input.MoveInputDirection.y <= 0;
            if (notPressingForward)
                return false;
            
            if (wallRight && StrafingTowardWall(rightWallHit))
                SetAttachedWall(rightWallHit);
            else if (wallLeft && StrafingTowardWall(leftWallHit))
                SetAttachedWall(leftWallHit);
            else
                return false;
            
            if (cooldownTimer.IsRunning && attachedWallRight == previousWallRight)
                return false;
            
            if (!IsValidWall(attachedWallHit))
                return false;
            
            return true;
        }
        
        bool CheckForWall() {
            wallRight = Physics.Raycast(WallCheckPosition, Player.Motor.CharacterRight, out rightWallHit, wallCheckDistance, wallLayers);
            wallLeft = Physics.Raycast(WallCheckPosition, -Player.Motor.CharacterRight, out leftWallHit, wallCheckDistance, wallLayers);
            
            DrawDebugLines();
            
            return wallRight || wallLeft;
        }
        
        
        bool StrafingTowardWall(RaycastHit wallHit) {
            var isRight = Vector3.Dot(Player.Motor.CharacterRight, wallHit.normal) < 0;
            
            switch (isRight) {
                case true when Input.MoveInputDirection.x > 0:
                    attachedWallRight = true;
                    attachedWallHit = wallHit;
                    return true;
                case false when Input.MoveInputDirection.x < 0:
                    attachedWallRight = false;
                    attachedWallHit = wallHit;
                    return true;
                default:
                    return false;
            }
        }
        
        bool AboveGround() {
            return !Physics.Raycast(Player.Motor.TransientPosition, -Player.Motor.CharacterUp, out RaycastHit hit, minDistanceFromGround);
        }
        
        bool IsValidWall(RaycastHit wallHit) {
            // Check bank angle
            var angle = Vector3.Angle(Player.Motor.CharacterUp, wallHit.normal);
            var angleIsValid = !(angle < minMaxBankAngle.x) && !(angle > minMaxBankAngle.y);
            if (!angleIsValid)
                return false;
            
            return true;
        }

        bool ShouldDetachFromWall() {
            if (detachFromWall)
                return true;

            var lookingAway = attachedWallRight 
                ? Vector3.Dot(Player.Motor.CharacterForward, -rightWallHit.normal) < -0.9f 
                : Vector3.Dot(Player.Motor.CharacterForward, -leftWallHit.normal) < -0.9f;
            if (lookingAway)
                return true;
            
            var strafingAway = attachedWallRight 
                ? Input.MoveInputDirection.x < 0 
                : Input.MoveInputDirection.x > 0;
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
            
            if (!IsValidWall(attachedWallHit))
                return true;
            
            return false;
        }

        void SetAttachedWall(RaycastHit wallHit) {
            var isRight = Vector3.Dot(Player.Motor.CharacterRight, wallHit.normal) < 0;
            
            attachedWallRight = isRight;
            attachedWallHit = wallHit;
        }
        
        void WallRun(ref Vector3 currentVelocity, float deltaTime) {
            Vector3 wallNormal = attachedWallHit.normal;
            Vector3 wallForward = Vector3.Cross(wallNormal, Player.Motor.CharacterUp);
            
            if ((Player.Motor.CharacterForward - wallForward).magnitude > (Player.Motor.CharacterForward - -wallForward).magnitude)
                wallForward = -wallForward;
            
            var gravity = wallRunGravity * deltaTime;
            var newVerticalVelocity = Mathf.Max(currentVelocity.y, 0f) + gravity;
            
            currentVelocity = new Vector3(currentVelocity.x, 0f, wallForward.z);
            
            var targetVelocity = wallForward * WallRunSpeed;
            var moveVelocity = Vector3.Lerp(
                a: currentVelocity,
                b: targetVelocity,
                t: 1f - Mathf.Exp(-wallRunResponse * deltaTime)
            );
            
            currentVelocity = moveVelocity + Player.Motor.CharacterUp * newVerticalVelocity;
            
            // Pull player toward wall & apply gravity
            currentVelocity += -wallNormal * (100 * deltaTime);
            //currentVelocity += Player.Motor.CharacterUp * (gravity * deltaTime);
            
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
            Vector3 forceToApply = attachedWallHit.normal * jumpOutForce + Player.Motor.CharacterUp * jumpUpForce;
            currentVelocity += forceToApply;
            
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