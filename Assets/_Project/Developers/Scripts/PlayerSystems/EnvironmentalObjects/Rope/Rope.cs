using System;
using System.Linq;
using KinematicCharacterController;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerSystems.EnvironmentalObjects {
    [RequireComponent(typeof(LineRenderer))]
    [ExecuteInEditMode]
    public class Rope : MonoBehaviour {
        [SerializeField] Transform startPointTransform;
        [SerializeField] RopeConfig ropeConfig = RopeConfig.DefaultConfig();
        [Space]
        [ReadOnly, SerializeField] LineRenderer lineRenderer;
        
        RopeVerlet ropeVerlet;
        Vector3[] segmentPositions;
        
        [SerializeField, ReadOnly] RopePart[] ropeParts;

        public static implicit operator RopeVerlet(Rope rope) => rope.ropeVerlet;
        
        void Start() {
            if (!Application.isPlaying)
                return;
            
            CreateRope();
        }
        
        void CreateRope() {
            ropeVerlet = new RopeVerlet(ropeConfig, startPointTransform.position);
            segmentPositions = new Vector3[ropeVerlet.Segments.Length];
            lineRenderer.positionCount = segmentPositions.Length;
            
            CreateColliders();
        }

        void CreateColliders() {
            if (ropeParts != null)
                DestroyRopeParts();
            
            ropeParts = new RopePart[ropeVerlet.Segments.Length];
            for (var i = 0; i < ropeVerlet.Segments.Length; i++) {
                var colliderObject = new GameObject($"RopeCollider_{i}") {
                    transform = {
                        parent = transform
                    },
                    tag = "RopePart"
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
            Debug.Log("Destroying colliders");
            
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
        
        void Update() {
            if (Keyboard.current.kKey.isPressed) {
                ropeVerlet.ApplyForceToSegment(23, Vector3.forward * 0.1f);
            }
            
            if (Keyboard.current.lKey.isPressed) {
                ropeVerlet.SetSegmentPosition(23, segmentPositions[23] + Vector3.forward * 0.1f);
            }
            
// #if UNITY_EDITOR
//             if (Application.isPlaying)
// #endif
//                 //DrawRope();
// #if UNITY_EDITOR
//             else
//                 DrawRopeInEditor();
// #endif
        }
        
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
            Debug.Log("Destroyed");
            DestroyRopeParts();
        }

#if UNITY_EDITOR
        
        RopeConfig previousConfig = RopeConfig.DefaultConfig();
        
        void OnValidate() {
            lineRenderer ??= GetComponent<LineRenderer>();
            startPointTransform ??= transform;
            
            lineRenderer.widthMultiplier = ropeConfig.width;
            
            if (previousConfig == ropeConfig)
                return;
            
            previousConfig = ropeConfig;
            
            if (Application.isPlaying) {
                CreateRope();
            }
            else
                DrawRopeInEditor();
        }

        void DrawRopeInEditor() {
            lineRenderer.positionCount = 2;
            lineRenderer.widthMultiplier = ropeConfig.width;
            
            var length = ropeConfig.segmentLength * ropeConfig.segmentCount;
            var startPosition = startPointTransform.position;
            var endPosition = startPointTransform.position - startPointTransform.up * length;
            
            lineRenderer.SetPosition(0, startPosition);
            lineRenderer.SetPosition(1, endPosition);
        }
        
#endif
    }
}