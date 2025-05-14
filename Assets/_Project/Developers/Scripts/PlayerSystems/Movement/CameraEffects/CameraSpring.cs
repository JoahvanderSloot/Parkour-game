using UnityEngine;
using UnityEngine.Serialization;

namespace PlayerSystems.Movement.CameraEffects
{
    public class CameraSpring : MonoBehaviour
    {
        [FormerlySerializedAs("halfLife")]
        [Min(0.1f)]
        [SerializeField] private float springHalfLife = 0.075f;
        [Space]
        [FormerlySerializedAs("frequency")]
        [Min(0.1f)]
        [SerializeField] private float springFrequency = 18f;
        [Space]
        [SerializeField] private float angularDisplacement = 2f;
        [SerializeField] private float linearDisplacement = 0.05f;

        private Vector3 _springPosition;
        private Vector3 _springVelocity;

        public void Initialize()
        {
            _springPosition = transform.position;
            _springVelocity = Vector3.zero;
        }

        public void UpdateSpring(float deltaTime, Vector3 up)
        {
            transform.localPosition = Vector3.zero;
            Spring(ref _springPosition, ref _springVelocity, transform.position, springHalfLife, springFrequency, deltaTime);

            var relativeSpringPosition = _springPosition - transform.position;
            var springHeight = Vector3.Dot(relativeSpringPosition, up);

            transform.localEulerAngles = new Vector3(-springHeight * angularDisplacement, 0f, 0f);
            transform.position += relativeSpringPosition * linearDisplacement;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, _springPosition);
            Gizmos.DrawWireSphere(_springPosition, 0.1f);
        }

        private static void Spring(ref Vector3 current, ref Vector3 velocity, Vector3 target, float halfLife, float frequency, float timeStep)
        {
            var dampingRatio = -Mathf.Log(0.5f) / (frequency * halfLife);
            var f = 1.0f + 2.0f * timeStep * dampingRatio * frequency;
            var oo = frequency * frequency;
            var hoo = timeStep * oo;
            var hhoo = timeStep * hoo;
            var detInv = 1.0f / (f + hhoo);
            var detX = f * current + timeStep * velocity + hhoo * target;
            var detV = velocity + hoo * (target - current);
            current = detX * detInv;
            velocity = detV * detInv;
        }
    }
}
