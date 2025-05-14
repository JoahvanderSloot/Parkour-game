using UnityEngine;

namespace PlayerSystems.Movement.CameraEffects {
    public class CameraOffset : MonoBehaviour {
        [SerializeField] float defaultResponse = 5f;
        float response;

        public Vector3 TargetOffset { get; private set; }
        public Vector3 CurrentOffset => transform.localPosition;

        public void Initialize() {
            ResetOffset();
        }

        public void SetResponse(float newResponse) =>
            response = newResponse > 0
                ? newResponse
                : defaultResponse;

        public void SetOffset(Vector3 offset, float response = 0f) {
            SetResponse(response);
            TargetOffset = offset;
        }

        public void ResetOffset(float response = 0f) {
            SetResponse(response);
            TargetOffset = Vector3.zero;
        }

        public void UpdateOffset(float deltaTime) {
            transform.localPosition = Vector3.Lerp(
                transform.localPosition,
                TargetOffset,
                1f - Mathf.Exp(-response * deltaTime)
            );
        }
    }
}