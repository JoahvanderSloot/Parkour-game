using System;
using PlayerSystems.Interaction;
using PlayerSystems.Modules;
using UnityEngine;

namespace PlayerSystems.EnvironmentalObjects.GrapplePoints {
    [CreateAssetMenu(fileName = "GrapplePoint_Module", menuName = "Player/Modules/Environmental/GrapplePointModule", order = 0)]
    public class GrapplePointMovementModule : MovementModule {
        [SerializeField] float cooldownTime = 1f;
        [SerializeField] float grappleSpeed = 10f;
        [SerializeField] float grappleDistanceMultiplier = 1.5f;
        [Space]
        [SerializeField] float ropeSpeed = 10f;
        [SerializeField] Vector3 ropeOffset;
        [Space]
        [SerializeField] RopeConfig ropeConfig = RopeConfig.DefaultConfig();
        
        bool interactedWithGrapplePoint = false;
        GrapplePoint grapplePoint;

        VisualRope ropeVisual;
        
        protected override void Initialize() {
            Player.InteractionHandler.OnInteract += HandleInteract;
            ropeVisual = new GameObject().AddComponent<VisualRope>();
            ropeVisual.CreateRope(ropeConfig, Vector3.zero);
        }

        void OnDisable() {
            if (ropeVisual) {
                Destroy(ropeVisual.gameObject);
                ropeVisual = null;
            }
            
            if (!Player)
                return;
            
            Player.InteractionHandler.OnInteract -= HandleInteract;
        }

        void HandleInteract(IInteractable interactable, Vector3 point) {
            if (interactable is not GrapplePoint grapplePoint)
                return;
            
            interactedWithGrapplePoint = true;
            this.grapplePoint = grapplePoint;
        }
        
        public override ModuleLevel ModuleLevel => ModuleLevel.ManualActivationModule;

        public override bool ShouldActivate {
            get {
                var activate = interactedWithGrapplePoint;
                interactedWithGrapplePoint = false;
                return activate;
            }
        }

        public override void ModuleUpdate() {
            
        }

        public override void EnableModule() {
            base.EnableModule();
            
            Debug.Log("GrapplePointMovementModule Enable");
            
            ropeVisual.gameObject.SetActive(true);
            ShootGrapple();
        }

        public override void DisableModule() {
            base.DisableModule();
            grapplePoint = null;
            interactedWithGrapplePoint = false;
            CannotBeOverridden = false;
            ropeVisual.gameObject.SetActive(false);
            Player.Movement.VelocityUpdate -= VelocityUpdate;
        }
        
        void UpdateRopeStartPoint() {
            if (!grapplePoint)
                return;
            
            var startPoint = Player.CameraPosition + ropeOffset;
            ropeVisual.SetStartPoint(startPoint);
        }

        async void ShootGrapple() {
            try {
                CannotBeOverridden = true;
                
                var ropeEndPoint = grapplePoint.transform.position;
                while (Vector3.Distance(ropeEndPoint, grapplePoint.transform.position) < 0.1f) {
                    ropeEndPoint = Vector3.Lerp(
                        ropeEndPoint,
                        grapplePoint.transform.position,
                        1f - Mathf.Exp(-ropeSpeed * Time.deltaTime)
                    );
                
                    ropeVisual.SetEndPoint(ropeEndPoint);
                    UpdateRopeStartPoint();
                
                    Debug.Log("GrapplePointMovementModule Shooting grapple to point: " + grapplePoint.name);
                    
                    await Awaitable.NextFrameAsync();
                }
                
                ropeVisual.SetEndPoint(grapplePoint.transform.position);
                UpdateRopeStartPoint();
                Player.Movement.VelocityUpdate += VelocityUpdate;
                
                CannotBeOverridden = false;
            }
            catch (Exception e) {
                throw; // TODO handle exception
            }
        }

        void VelocityUpdate(ref Vector3 currentVelocity, float deltaTime) {
            GrappleToPoint(grapplePoint, ref currentVelocity);
        }
        
        void GrappleToPoint(GrapplePoint point, ref Vector3 currentVelocity) {
            if (!point)
                return;
            
            Debug.Log("GrapplePointMovementModule Grappling to point: " + point.name);
            
            var grapplePosition = grapplePoint.transform.position;
            var direction = (grapplePosition - Player.TransientBottomPosition).normalized;
            var distance = Vector3.Distance(Player.transform.position, grapplePosition);
            
            var desiredVelocity = direction * (grappleSpeed * (distance * grappleDistanceMultiplier));
            
            Player.Motor.ForceUnground();
            currentVelocity = desiredVelocity;
            
            GrapplePointManager.StartCooldownForAll(cooldownTime);
            DisableModule();
        }
    }
}