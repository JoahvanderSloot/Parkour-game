using UnityEngine;

namespace PlayerSystems.EnvironmentalObjects {
    public struct RopeSegment {
        public Vector3 Position;
        public Vector3 PreviousPosition;
            
        public RopeSegment(Vector3 position) {
            Position = position;
            PreviousPosition = position;
        }
    }
}