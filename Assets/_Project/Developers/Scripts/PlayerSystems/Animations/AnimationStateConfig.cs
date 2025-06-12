using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerSystems.Controls.Weapons.Animations {
    [Serializable]
    public struct AnimationStateConfig : IEquatable<AnimationStateConfig> {
        [SerializeField] AnimationClip clip;
        [SerializeField, Range(0f, 2f)] float animationSpeed;
        [SerializeField, Range(0f,2f)] float speedScaling;
        [Space]
        [SerializeField, Range(0f, 1f)] float blendInSeconds;
        [SerializeField] AnimationCurve blendInCurve;
        [Space]
        [SerializeField] List<AnimationEvent> animationEvents;
        
        public AnimationClip Clip => clip;
        public float AnimationSpeed => animationSpeed;
        public float SpeedScaling => speedScaling;
        public float BlendInSeconds => blendInSeconds;
        public AnimationCurve BlendInCurve => blendInCurve ??= AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        public List<AnimationEvent> AnimationEvents => animationEvents;
        
        public static AnimationStateConfig Default => new AnimationStateConfig {
            clip = null,
            animationSpeed = 1f,
            blendInSeconds = 0.2f,
            blendInCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f),
            speedScaling = 1f,
            animationEvents = new List<AnimationEvent>(),
        };
        
        public static AnimationStateConfig DefaultIgnoreSpeed => new AnimationStateConfig {
            clip = null,
            animationSpeed = 1f,
            blendInSeconds = 0.2f,
            blendInCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f),
            speedScaling = 0f,
            animationEvents = new List<AnimationEvent>(),
        };
        
        public bool Equals(AnimationStateConfig other) {
            return Equals(clip, other.clip) && animationSpeed.Equals(other.animationSpeed) && speedScaling.Equals(other.speedScaling) && blendInSeconds.Equals(other.blendInSeconds) && Equals(blendInCurve, other.blendInCurve) && Equals(animationEvents, other.animationEvents);
        }
        public override bool Equals(object obj) {
            return obj is AnimationStateConfig other && Equals(other);
        }
        public override int GetHashCode() {
            return HashCode.Combine(clip, animationSpeed, speedScaling, blendInSeconds, blendInCurve, animationEvents);
        }
        public static bool operator ==(AnimationStateConfig left, AnimationStateConfig right) {
            return left.Equals(right);
        }
        public static bool operator !=(AnimationStateConfig left, AnimationStateConfig right) {
            return !left.Equals(right);
        }
    }
}