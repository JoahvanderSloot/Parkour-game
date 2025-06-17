using KinematicCharacterController;
using UnityEngine;

namespace PlayerSystems.EnvironmentalObjects {
    [RequireComponent(typeof(LineRenderer))]
    public class VisualRope : MonoBehaviour {
        [SerializeField] Transform startPointTransform;
        [SerializeField] RopeConfig ropeConfig = RopeConfig.DefaultConfig();
        [Space]
        [ReadOnly, SerializeField] LineRenderer lineRenderer;
        
        RopeVerlet ropeVerlet;
        Vector3[] segmentPositions;
        
        public static implicit operator RopeVerlet(VisualRope visualRope) => visualRope.ropeVerlet;
        
        public void CreateRope(RopeConfig config, Vector3 startPosition) {
            lineRenderer = GetComponent<LineRenderer>() ?? gameObject.AddComponent<LineRenderer>();
            ropeConfig = config;
            startPointTransform = transform;
            startPointTransform.position = startPosition;
            CreateRope();
        }
        public void CreateRope() {
            ropeVerlet = new RopeVerlet(ropeConfig, startPointTransform.position);
            segmentPositions = new Vector3[ropeVerlet.Segments.Length];
            lineRenderer.positionCount = segmentPositions.Length;
        }
        
        void FixedUpdate() {
            ropeVerlet.SetStartPoint(startPointTransform.position);
            ropeVerlet.Simulate();
            DrawRope();
        }

        void DrawRope() {
            for (var i = 0; i < ropeVerlet.Segments.Length; i++) {
                segmentPositions[i] = ropeVerlet.Segments[i].Position;
            }

            lineRenderer.SetPositions(segmentPositions);
        }
        
        public void SetStartPoint(Vector3 startPoint) {
            startPointTransform.position = startPoint;
        }

        public void SetEndPoint(Vector3 endPoint) {
            ropeVerlet.SetSegmentPosition(0, endPoint);
        }
    }
}