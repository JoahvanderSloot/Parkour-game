using UnityEngine;

namespace PlayerSystems.Movement.CameraEffects
{
    public class CameraLean : MonoBehaviour
    {
        [SerializeField] private float attackDamping = 0.5f;
        [SerializeField] private float decayDamping = 0.3f;
        [SerializeField] private float walkStrength = 0.075f;
        [SerializeField] private float slideStrength = 0.2f;
        [SerializeField] private float strengthResponse = 5f;

        private Vector3 _dampedAcceleration;
        private Vector3 _dampedAccelerationVelocity;

        private float _smoothStrength;

        public void Initialize()
        {
            _smoothStrength = walkStrength;
        }

        public void UpdateLean(float deltaTime, bool sliding, Vector3 acceleration, Vector3 up)
        {
            Vector3 planarAcceleration = Vector3.ProjectOnPlane(acceleration, up);
            float damping = planarAcceleration.magnitude > _dampedAcceleration.magnitude ? attackDamping : decayDamping;

            _dampedAcceleration = Vector3.SmoothDamp
            (
                current: _dampedAcceleration,
                target: planarAcceleration,
                currentVelocity: ref _dampedAccelerationVelocity,
                smoothTime: damping,
                maxSpeed: Mathf.Infinity,
                deltaTime: deltaTime
            );

            // Get the rotation axis based on the acceleration and the up vector
            Vector3 leanAxis = Vector3.Cross(_dampedAcceleration.normalized, up);

            // Reset the rotation to zero and apply the lean rotation
            transform.localRotation = Quaternion.identity;
            float targetStrength = sliding ? slideStrength : walkStrength;
            _smoothStrength = Mathf.Lerp(_smoothStrength, targetStrength, 1f - Mathf.Exp(-strengthResponse * deltaTime));
            transform.rotation = Quaternion.AngleAxis(-_dampedAcceleration.magnitude * _smoothStrength, leanAxis) * transform.rotation;

            //Debug.DrawRay(transform.position, acceleration, Color.red);
            //Debug.DrawRay(transform.position, _dampedAcceleration, Color.blue);
        }
    }
}
