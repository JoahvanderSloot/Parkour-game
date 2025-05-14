using UnityEngine;

namespace PlayerSystems.Movement.CameraEffects {
    public class CameraTilt : MonoBehaviour {
        [SerializeField] float defaultTiltResponse = 10f;
        float tiltResponse;
        
        public Vector3 CurrentTilt { get; private set; }
        public Vector3 GoalTilt { get; private set; }

        public void Initialize() {
            ResetTilt();
        }
        
        public void SetResponse(float response) =>
            tiltResponse = response > 0f
                ? response
                : defaultTiltResponse;
        
        public void ResetTilt(float response = 0f) {
            Debug.Log("Reset Tilt: " + response);
            SetResponse(response);
            GoalTilt = Vector3.zero;
        }
        
        public void SetTilt(Vector3 newTilt, float response = 0f) {
            Debug.Log("Set Tilt: " + newTilt);
            SetResponse(response);
            GoalTilt = newTilt;
        }
        
        public void UpdateTilt(float deltaTime) {
            
            CurrentTilt = Vector3.Lerp(
                CurrentTilt,
                GoalTilt, 
                1f - Mathf.Exp(-tiltResponse * deltaTime)
            );
            
            transform.localRotation = Quaternion.Euler(CurrentTilt);
            Debug.Log($"Current Tilt: {CurrentTilt}, Goal Tilt: {GoalTilt}, Local Rotation: {transform.localRotation}");
        }
    }
}