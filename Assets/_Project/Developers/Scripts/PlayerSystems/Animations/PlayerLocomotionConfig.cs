using System;
using UnityEngine;

namespace PlayerSystems.Controls.Weapons.Animations {
    [Serializable]
    public struct PlayerLocomotionConfig : IEquatable<PlayerLocomotionConfig> {
        [SerializeField] AnimationStateConfig idle;
        [SerializeField] AnimationStateConfig run;
        [SerializeField] AnimationStateConfig slide;
        [SerializeField] AnimationStateConfig airborne;
        [SerializeField] AnimationStateConfig wallClimb;
        
        public AnimationStateConfig IdleStateConfig => idle;
        public AnimationStateConfig RunStateConfig => run;
        public AnimationStateConfig SlideStateConfig => slide;
        public AnimationStateConfig AirborneStateConfig => airborne;
        public AnimationStateConfig WallClimbStateConfig => wallClimb;

        public static PlayerLocomotionConfig Default => new PlayerLocomotionConfig {
            idle = AnimationStateConfig.DefaultIgnoreSpeed,
            run = AnimationStateConfig.Default,
            slide = AnimationStateConfig.Default,
            airborne = AnimationStateConfig.Default,
            wallClimb = AnimationStateConfig.Default
        };

        public bool Equals(PlayerLocomotionConfig other) {
            return idle.Equals(other.idle) && run.Equals(other.run) && slide.Equals(other.slide) && airborne.Equals(other.airborne);
        }
        public override bool Equals(object obj) {
            return obj is PlayerLocomotionConfig other && Equals(other);
        }
        public override int GetHashCode() {
            return HashCode.Combine(idle, run, slide, airborne);
        }
        public static bool operator ==(PlayerLocomotionConfig left, PlayerLocomotionConfig right) {
            return left.Equals(right);
        }
        public static bool operator !=(PlayerLocomotionConfig left, PlayerLocomotionConfig right) {
            return !left.Equals(right);
        }
    }
}