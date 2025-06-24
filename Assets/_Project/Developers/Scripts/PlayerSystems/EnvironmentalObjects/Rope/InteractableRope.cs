using System;
using System.Linq;
using KinematicCharacterController;
using PlayerSystems.Interaction;
using UnityEngine;

namespace PlayerSystems.EnvironmentalObjects {
    [RequireComponent(typeof(LineRenderer))]
    [ExecuteAlways]
    public class InteractableRope : MonoBehaviour {
        public const string c_RopePartTag = "RopePart";
        
        [SerializeField] Transform startPointTransform;
        [SerializeField] RopeConfig ropeConfig = RopeConfig.DefaultConfig();
        [Space]
        [ReadOnly, SerializeField] LineRenderer lineRenderer;
        
        RopeVerlet ropeVerlet;
        Vector3[] segmentPositions;
        
        [SerializeField, ReadOnly] RopePart[] ropeParts;

        public static implicit operator RopeVerlet(InteractableRope interactableRope) => interactableRope.ropeVerlet;
        
        void Start() {
            if (!Application.isPlaying)
                return;
            
            CreateRope();
        }
        
        void CreateRope() {
            ropeVerlet = new RopeVerlet(ropeConfig, startPointTransform.position);
            segmentPositions = new Vector3[ropeVerlet.Segments.Length];
            lineRenderer.positionCount = segmentPositions.Length;
            
            CreateRopeParts();
        }

        void CreateRopeParts() {
            if (ropeParts != null)
                DestroyRopeParts();
            
            ropeParts = new RopePart[ropeVerlet.Segments.Length];
            for (var i = 0; i < ropeVerlet.Segments.Length; i++) {
                var colliderObject = new GameObject($"RopeCollider_{i}") {
                    transform = {
                        parent = transform
                    },
                    tag = c_RopePartTag,
                    layer = IInteractable.InteractableLayer
                };
                
                var ropePart = colliderObject.AddComponent<RopePart>();
                ropePart.Initialize(ropeVerlet, i);
                
                var col = colliderObject.AddComponent<SphereCollider>();
                col.isTrigger = true;
                col.radius = ropeVerlet.Width;
                
                ropeParts[i] = ropePart;
            }
            
            UpdateRopeParts();
        }

        void UpdateRopeParts() {
            for (var i = 0; i < ropeVerlet.Segments.Length; i++) {
                ropeParts[i].UpdatePosition(ropeVerlet.Segments[i].Position);
            }
        }

        void DestroyRopeParts() {
            foreach (var ropePart in ropeParts.ToList()) {
                if (ropePart == null)
                    continue;
                
                if (Application.isPlaying) {
                    Destroy(ropePart.gameObject);
                } 
                else {
                    DestroyImmediate(ropePart.gameObject);
                }
            }
            
            Array.Clear(ropeParts, 0, ropeParts.Length);
            ropeParts = null;
        }
        
#if UNITY_EDITOR
        void Update() {
            if (Application.isPlaying)
                return;
            
            var ropeValid = ValidatePosition() && ValidateConfig();
            if (ropeValid)
                return;
            
            DrawRopeInEditor();
        }
#endif
        
        void FixedUpdate() {
            ropeVerlet.SetStartPoint(startPointTransform.position);
            ropeVerlet.Simulate();

            DrawRope();
            UpdateRopeParts();
        }

        void DrawRope() {
            for (var i = 0; i < ropeVerlet.Segments.Length; i++) {
                segmentPositions[i] = ropeVerlet.Segments[i].Position;
            }

            lineRenderer.SetPositions(segmentPositions);
        }

        void OnDestroy() {
            DestroyRopeParts();
        }

#if UNITY_EDITOR
        
        RopeConfig previousConfig = RopeConfig.DefaultConfig();
        Vector3 previousPosition;
        
        void OnValidate() {
            lineRenderer ??= GetComponent<LineRenderer>();
            startPointTransform ??= transform;
            
            lineRenderer.widthMultiplier = ropeConfig.width;

            var ropeValid = ValidatePosition() && ValidateConfig();
            
            if (ropeValid)
                return;
            
            if (Application.isPlaying)
                CreateRope();
            else
                DrawRopeInEditor();
        }

        bool ValidatePosition() {
            var valid= previousPosition == transform.position;
            previousPosition = transform.position;
            return valid;
        }

        bool ValidateConfig() {
            var valid = previousConfig == ropeConfig;
            previousConfig = ropeConfig;
            return valid;
        }

        void DrawRopeInEditor() {
            var count = ropeConfig.segmentCount;
            var segmentLength = ropeConfig.segmentLength;
            
            lineRenderer.positionCount = count;
            lineRenderer.widthMultiplier = ropeConfig.width;

            for (var i = 0; i < count; i++) {
                var pos = startPointTransform.position + Vector3.down * (i * segmentLength);
                lineRenderer.SetPosition(i, pos);
            }
        }
        
#endif
    }
}