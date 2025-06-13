using System;
using UnityEngine;

namespace PlayerSystems.Attributes {
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class MinMaxSliderAttribute : PropertyAttribute {
        public readonly float Min;
        public readonly float Max;

        public MinMaxSliderAttribute(float min, float max) {
            Min = min;
            Max = max;
        }
    }
}