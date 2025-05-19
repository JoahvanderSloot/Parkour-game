using System;
using UnityEngine;

namespace PlayerSystems.EnvironmentalObjects.Lache {
    public class Lache : MonoBehaviour {
        public const string c_LacheTag = "Lache";
        
        [SerializeField] Transform barStart;
        [SerializeField] Transform barEnd;
        
        public Vector3 BarDirection => (barEnd.position - barStart.position).normalized;

        void Start() {
            gameObject.tag = c_LacheTag;
        }

        public Vector3 GetAttachmentPoint(in RaycastHit rayHit) {
            Vector3 start = barStart.position;
            Vector3 end = barEnd.position;
            Vector3 point = rayHit.point;

            Vector3 barDirection = end - start;
            float barLength = barDirection.magnitude;
            if (barLength == 0f) return start;

            Vector3 barDirNormalized = barDirection / barLength;
            float projectedLength = Vector3.Dot(point - start, barDirNormalized);
            float clampedLength = Mathf.Clamp(projectedLength, 0f, barLength);

            return start + barDirNormalized * clampedLength;
        }
    }
}