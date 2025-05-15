using System;
using UnityEngine;

namespace PlayerSystems.EnvironmentalObjects {
    [Serializable]
    public struct RopeConfig : IEquatable<RopeConfig> {
        [Header("Rope")]
        public int segmentCount;
        public float segmentLength;
        public float width;
        
        [Header("Physics")]
        public LayerMask collisionMask;
        public Vector3 gravityForce;
        [Range(0f,1f)] public float damping;
        [Range(0f,1f)] public float bounceFactor;
        [Space]
        public int physicsIterations;
        public int collisionIterationInterval;
        public int maxCollisionCount;

        public RopeConfig(int segmentCount, float segmentLength, float width, LayerMask collisionMask, Vector3 gravityForce, float damping, float bounceFactor, int physicsIterations, int collisionIterationInterval, int maxCollisionCount) {
            this.segmentCount = segmentCount;
            this.segmentLength = segmentLength;
            this.width = width;
            
            this.collisionMask = collisionMask;
            this.gravityForce = gravityForce;
            this.damping = damping;
            this.bounceFactor = bounceFactor;
            
            this.physicsIterations = physicsIterations;
            this.collisionIterationInterval = collisionIterationInterval;
            this.maxCollisionCount = maxCollisionCount;
        }

        public static RopeConfig DefaultConfig() =>
            new(50, 0.25f, 0.2f, 0, Vector3.up * -2f, 0.02f, 0.1f, 50, 2, 5);
        
        #region Equatable

        public bool Equals(RopeConfig other) {
            return segmentCount == other.segmentCount && segmentLength.Equals(other.segmentLength) && width.Equals(other.width) && collisionMask == other.collisionMask && gravityForce.Equals(other.gravityForce) && damping.Equals(other.damping) && bounceFactor.Equals(other.bounceFactor) && physicsIterations == other.physicsIterations && collisionIterationInterval == other.collisionIterationInterval && maxCollisionCount == other.maxCollisionCount;
        }

        public override bool Equals(object obj) {
            return obj is RopeConfig other && Equals(other);
        }

        public override int GetHashCode() {
            var hashCode = new HashCode();
            hashCode.Add(segmentCount);
            hashCode.Add(segmentLength);
            hashCode.Add(width);
            hashCode.Add(gravityForce);
            hashCode.Add(damping);
            hashCode.Add(bounceFactor);
            hashCode.Add(physicsIterations);
            hashCode.Add(collisionIterationInterval);
            hashCode.Add(maxCollisionCount);
            return hashCode.ToHashCode();
        }
        
        public static bool operator ==(RopeConfig left, RopeConfig right) {
            return left.Equals(right);
        }
        public static bool operator !=(RopeConfig left, RopeConfig right) {
            return !left.Equals(right);
        }

        #endregion
    }
}