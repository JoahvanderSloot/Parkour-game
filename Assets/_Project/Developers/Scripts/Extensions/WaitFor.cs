using System.Collections.Generic;
using UnityEngine;

namespace Extensions {
    public static class WaitFor {
        public static WaitForEndOfFrame EndOfFrame { get; } = new WaitForEndOfFrame();
        public static WaitForFixedUpdate FixedUpdate { get; } = new WaitForFixedUpdate();
        
        static readonly Dictionary<float, WaitForSeconds> s_waitForSecondsCache = new (100, new FloatComparer());

        public static WaitForSeconds Seconds(float seconds) {
            if (seconds < 1f / Application.targetFrameRate)
                return null;
            
            if (s_waitForSecondsCache.TryGetValue(seconds, out var wait))
                return wait;
            
            wait = new WaitForSeconds(seconds);
            s_waitForSecondsCache[seconds] = wait;
            return wait;
        }
        
        class FloatComparer : IEqualityComparer<float> {
            public bool Equals(float x, float y) => Mathf.Abs(x - y) <= Mathf.Epsilon;
            public int GetHashCode(float obj) => obj.GetHashCode();
        }
    }
}