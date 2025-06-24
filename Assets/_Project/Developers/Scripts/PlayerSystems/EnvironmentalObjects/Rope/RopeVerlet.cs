using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerSystems.EnvironmentalObjects {
    public class RopeVerlet {
        readonly RopeConfig config;
        public readonly RopeSegment[] Segments;
        
        public Vector3 StartPoint { get; private set; }

        readonly Collider[] collisionCheckArray;
        
        readonly Dictionary<int, Vector3> forceToAdd;
        readonly Dictionary<int, Vector3> setPositions;
        
        public int SegmentCount => config.segmentCount;
        public float SegmentLength => config.segmentLength;
        public float Width => config.width;
        public float Length => config.segmentCount * config.segmentLength;
        
        public LayerMask CollisionMask => config.collisionMask;
        public float BounceFactor => config.bounceFactor;
        public float Damping => config.damping;
        
        Vector3 externallySetGravity = Vector3.zero;
        public Vector3 Gravity => externallySetGravity == Vector3.zero
                ? config.gravityForce
                : externallySetGravity;

        public int PhysicsIterations => config.physicsIterations;
        public int CollisionIterationInterval => config.collisionIterationInterval;

        readonly bool collisionCheck;
        
        public RopeVerlet(RopeConfig config, Vector3 startPoint, bool collisionCheck = true) {
            this.config = config;
            StartPoint = startPoint;
            this.collisionCheck = collisionCheck;
            
            Segments = new RopeSegment[SegmentCount];
            for (int i = 0; i < SegmentCount; i++) {
                var yPos = StartPoint.y - SegmentLength * i;
                Segments[i] = new RopeSegment(new Vector3(StartPoint.x, yPos, StartPoint.z));
            }
            
            collisionCheckArray = new Collider[this.config.maxCollisionCount];
            
            forceToAdd = new Dictionary<int, Vector3>();
            setPositions = new Dictionary<int, Vector3>();
            
            for (int i = 0; i < SegmentCount; i++) {
                forceToAdd[i] = Vector3.zero;
            }
        }

        public void SetGravityDirection(Vector3 gravityDirection) {
            var normalizedDirection = gravityDirection.normalized;
            externallySetGravity = normalizedDirection * config.gravityForce.magnitude;
        }
        
        public void ResetGravity() {
            externallySetGravity = Vector3.zero;
        }

        public void SetStartPoint(Vector3 startPoint) => StartPoint = startPoint;
        
        // ReSharper disable Unity.PerformanceAnalysis
        public void ApplyForceToSegment(int index, Vector3 force) {
            if (index < 0 || index >= Segments.Length) {
                Debug.LogError($"Index {index} is out of bounds for the rope segments.");
                return;
            }
            
            forceToAdd[index] += force;
            //Segments[index].PreviousPosition += force;
        }
        
        public void SetSegmentPosition(int index, Vector3 position) {
            if (index < 0 || index >= Segments.Length) {
                Debug.LogError($"Index {index} is out of bounds for the rope segments.");
                return;
            }
            
            setPositions[index] = position;
            //Segments[index].Position = position;
        }

        public void Simulate() {
            SimulatePhysics();
            
            for (int i = 0; i < PhysicsIterations; i++) {
                ApplyConstraints();
                
                if (collisionCheck && i % CollisionIterationInterval == 0) {
                    HandleCollisions();
                }
            }
        }
        
        void SimulatePhysics() {
            for (int i = 0; i < Segments.Length; i++) {
                RopeSegment newSegment = Segments[i];
                
                if (setPositions.TryGetValue(i, out var position)) {
                    newSegment.Position = position;
                    newSegment.PreviousPosition = position;
                    setPositions.Remove(i);
                }
                
                Vector3 velocity = (newSegment.Position - newSegment.PreviousPosition + forceToAdd[i]) * (1f - Damping);
                forceToAdd[i] = Vector3.zero;
                
                newSegment.PreviousPosition = newSegment.Position;
                newSegment.Position += velocity;
                newSegment.Position += Gravity * Time.fixedDeltaTime;
                Segments[i] = newSegment;
            }
        }

        void ApplyConstraints() {
            RopeSegment firstSegment = Segments[0];
            firstSegment.PreviousPosition = firstSegment.Position;
            firstSegment.Position = StartPoint;
            Segments[0] = firstSegment;
            
            for (int i = 0; i < Segments.Length - 1; i++) {
                RopeSegment segmentA = Segments[i];
                RopeSegment segmentB = Segments[i + 1];

                Vector3 delta = segmentA.Position - segmentB.Position;
                float distance = delta.magnitude;
                float difference = distance - SegmentLength;
                 
                Vector3 changeDir = delta.normalized;
                Vector3 changeAmount = changeDir * (difference * 0.5f);

                if (i != 0) {
                    segmentA.Position -= changeAmount;
                }

                segmentB.Position += changeAmount;

                Segments[i] = segmentA;
                Segments[i + 1] = segmentB;
            }
        }

        void HandleCollisions() {
            for (int i = 0; i < Segments.Length; i++) {
                var segment = Segments[i];
                Vector3 velocity = segment.Position - segment.PreviousPosition;
                var size = Physics.OverlapSphereNonAlloc(segment.Position, 0.1f, collisionCheckArray, CollisionMask);

                if (size == 0)
                    continue;
                
                for (var index = 0; index < size; index++) {
                    var collider = collisionCheckArray[index];
                    
                    if (collider.gameObject.CompareTag("RopePart"))
                        continue;
                    
                    var closestPoint = collider.ClosestPoint(segment.Position);
                    var distance = Vector3.Distance(segment.Position, closestPoint);

                    if (distance < Width) {
                        var delta = segment.Position - closestPoint;
                        if (delta.normalized == Vector3.zero) {
                            delta = segment.Position - collider.bounds.center;
                        }

                        var depth = Width - distance;
                        segment.Position += delta.normalized * depth;
                        velocity = Vector3.Reflect(velocity, delta.normalized) * BounceFactor;
                    }
                }

                segment.PreviousPosition = segment.Position - velocity;
                Segments[i] = segment;
                
                Array.Clear(collisionCheckArray, 0, size);
            }
        }
    }
}