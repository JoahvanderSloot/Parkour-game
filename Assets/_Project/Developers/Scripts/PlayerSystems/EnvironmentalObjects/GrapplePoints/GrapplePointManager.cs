using System.Collections.Generic;

namespace PlayerSystems.EnvironmentalObjects.GrapplePoints {
    public static class GrapplePointManager {
        static List<GrapplePoint> s_grapplePoints;
        
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Setup() {
            s_grapplePoints = new List<GrapplePoint>();
        }
        
        public static void RegisterGrapplePoint(GrapplePoint point) {
            if (!s_grapplePoints.Contains(point)) {
                s_grapplePoints.Add(point);
            }
        }
        
        public static void UnregisterGrapplePoint(GrapplePoint point) {
            if (s_grapplePoints.Contains(point)) {
                s_grapplePoints.Remove(point);
            }
        }
        
        public static void StartCooldownForAll(float durationSeconds) {
            foreach (var point in s_grapplePoints) {
                point.StartCooldown(durationSeconds);
            }
        }
    }
}