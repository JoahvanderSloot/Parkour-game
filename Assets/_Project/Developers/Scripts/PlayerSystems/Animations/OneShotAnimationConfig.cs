using System;
using System.Collections.Generic;
using PlayerSystems.Attributes;
using UnityEngine;

namespace PlayerSystems.Controls.Weapons.Animations {
    [Serializable]
    public struct OneShotAnimationConfig : IEquatable<OneShotAnimationConfig> {
        [SerializeField] AnimationClip clip;
        [SerializeField, Range(0f,2f)] float animationSpeed;
        [SerializeField, Range(0f, 1f)] float addedSeconds;
        [Space]
        [SerializeField, MinMaxSlider(0f, 1f)] Vector2 blend;
        [SerializeField] AnimationCurve blendInCurve;
        [SerializeField] AnimationCurve blendOutCurve;
        [Space]
        [SerializeField] bool allowReplay;
        [SerializeField] bool allowInterrupt;
        [Space]
        [SerializeField] List<AnimationEvent> animationEvents;
        
        public AnimationClip Clip => clip;
        public float AnimationSpeed => animationSpeed;
        public Vector2 Blend => blend;
        public float AddedSeconds => addedSeconds;
        public AnimationCurve BlendInCurve => blendInCurve;
        public AnimationCurve BlendOutCurve => blendOutCurve;
        public bool AllowReplay => allowReplay;
        public bool AllowInterrupt => allowInterrupt;
        public List<AnimationEvent> AnimationEvents => animationEvents;
        
        public static OneShotAnimationConfig Default => new OneShotAnimationConfig {
            clip = null,
            animationSpeed = 1f,
            blend = new Vector2(0.1f, 0.9f),
            blendInCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f),
            blendOutCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f),
            addedSeconds = 0f,
            allowReplay = false,
            allowInterrupt = true,
            animationEvents = new List<AnimationEvent>()
        };

        public OneShotAnimationConfig OverrideAllowReplay(bool value) {
            return new OneShotAnimationConfig {
                clip = clip,
                animationSpeed = animationSpeed,
                blend = blend,
                addedSeconds = addedSeconds,
                blendInCurve = blendInCurve,
                blendOutCurve = blendOutCurve,
                allowReplay = value,
                allowInterrupt = allowInterrupt
            };
        }
        public OneShotAnimationConfig OverrideInterrupt(bool value) {
            return new OneShotAnimationConfig {
                clip = clip,
                animationSpeed = animationSpeed,
                blend = blend,
                addedSeconds = addedSeconds,
                blendInCurve = blendInCurve,
                blendOutCurve = blendOutCurve,
                allowReplay = allowReplay,
                allowInterrupt = value
            };
        }
        
        public bool Equals(OneShotAnimationConfig other) {
            return Equals(clip, other.clip) && animationSpeed.Equals(other.animationSpeed) && blend.Equals(other.blend) && addedSeconds.Equals(other.addedSeconds) && Equals(blendInCurve, other.blendInCurve) && Equals(blendOutCurve, other.blendOutCurve);
        }
        public override bool Equals(object obj) {
            return obj is OneShotAnimationConfig other && Equals(other);
        }
        public override int GetHashCode() {
            return HashCode.Combine(clip, animationSpeed, blend, addedSeconds, blendInCurve, blendOutCurve);
        }
        public static bool operator ==(OneShotAnimationConfig left, OneShotAnimationConfig right) {
            return left.Equals(right);
        }
        public static bool operator !=(OneShotAnimationConfig left, OneShotAnimationConfig right) {
            return !left.Equals(right);
        }
    }
}