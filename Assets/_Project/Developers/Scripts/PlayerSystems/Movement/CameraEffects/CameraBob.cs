using Extensions;
using UnityEngine;

namespace PlayerSystems.Movement.CameraEffects {
    public class CameraBob : MonoBehaviour {
        [SerializeField] float bobResponse = 5f;
        [Space]
        [SerializeField] float verticalBobSpeed = 10f;
        [SerializeField] float verticalBobAmount = 0.2f;
        //[SerializeField, Range(0,2f)] float verticalSpeedScale = 1f;
        [Space]
        [SerializeField] float horizontalBobSpeed = 0f;
        [SerializeField] float horizontalBobAmount = 0f;
        //[SerializeField, Range(0,2f)] float horizontalSpeedScale = 1f;
        
        float startTime;
        
        public bool IsEnabled { get; private set; }

        public void Initialize() {
            transform.localPosition = Vector3.zero;
        }
        
        public void Enable() {
            IsEnabled = true;
            startTime = Time.time;
        }

        public void Disable() {
            IsEnabled = false;
        }

        public void UpdateBob(float deltaTime) {
            //Debug.Log($"Bob: Enabled: {IsEnabled}, LocalPos: {transform.localPosition}");
            
            if (!IsEnabled) {
                var defaultPos = Vector3.Lerp(
                    transform.localPosition,
                    Vector3.zero, 
                    1f - Mathf.Exp(-bobResponse * deltaTime)
                );
                
                transform.localPosition = defaultPos;
                
                return;
            }
                
            var time = Time.time - startTime;
            var y = Mathf.Sin(time * verticalBobSpeed) * verticalBobAmount;
            var x = Mathf.Sin(time * horizontalBobSpeed) * horizontalBobAmount;
            
            var newPosition = Vector3.Lerp(
                transform.localPosition,
                new Vector3(x, y, 0), 
                1f - Mathf.Exp(-bobResponse * deltaTime)
            );
            
            transform.localPosition = newPosition;
        }
    }
}