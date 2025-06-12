using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerSystems.Controls.Weapons.Animations {
    public readonly struct AnimationState : IEquatable<AnimationState> {
        public AnimationClip Clip { get; }
        public float AnimationSpeed { get; }
        public float SpeedScaling { get; }
        public float BlendInSeconds { get; }
        public AnimationCurve BlendInCurve { get; }
        public List<AnimationEvent> Events { get; }
        public int ID { get; }
        
        
        static int s_nextId = 0;
        static int GetNextID() {
            if (s_nextId + 1 >= int.MaxValue)
                s_nextId = 0;
            
            return s_nextId++;
        }
        
        public static AnimationState Create(AnimationStateConfig config) {
            return new AnimationState(config);
        }
        
        AnimationState(AnimationStateConfig config) {
            Clip = config.Clip;
            AnimationSpeed = config.AnimationSpeed;
            SpeedScaling = config.SpeedScaling;
            BlendInSeconds = config.BlendInSeconds;
            BlendInCurve = config.BlendInCurve;
            Events = config.AnimationEvents;
            ID = GetNextID();
        }

        public bool Equals(AnimationState other) {
            return ID == other.ID;
        }
        public override bool Equals(object obj) {
            return obj is AnimationState other && Equals(other);
        }
        public override int GetHashCode() {
            return ID;
        }
        public static bool operator ==(AnimationState left, AnimationState right) {
            return left.Equals(right);
        }
        public static bool operator !=(AnimationState left, AnimationState right) {
            return !left.Equals(right);
        }
    }
}