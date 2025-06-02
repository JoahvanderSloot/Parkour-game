using UnityEngine;

namespace Extensions {
    public static class MathExtensions {
        public static float Map(this float value, float fromMin, float fromMax, float toMin, float toMax) {
            return (value - fromMin) / (fromMax - fromMin) * (toMax - toMin) + toMin;
        }
        public static float Map01(this float value, float fromMin, float fromMax) {
            return (value - fromMin) / (fromMax - fromMin) * (1f - 0f) + 0f;
        }
        
        public static float MapClamped(this float value, float fromMin, float fromMax, float toMin, float toMax) {
            return Mathf.Clamp(value.Map(fromMin, fromMax, toMin, toMax), toMin, toMax);
        }
        public static float MapClamped01(this float value, float fromMin, float fromMax) {
            return Mathf.Clamp01(value.Map(fromMin, fromMax, 0f, 1f));
        }

        public static bool Approx(this float value, float otherValue) {
            return Mathf.Approximately(value, otherValue);
        }
    }
}