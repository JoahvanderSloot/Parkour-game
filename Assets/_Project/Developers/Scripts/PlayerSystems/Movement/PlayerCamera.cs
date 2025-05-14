using KinematicCharacterController;
using PlayerSystems.Input;
using UnityEngine;

namespace PlayerSystems.Movement
{
    public class PlayerCamera : MonoBehaviour {
        [SerializeField] float sensitivity = 0.1f;

        Vector3 inputAngles;

        KinematicCharacterMotor motor;

        Transform target;

        Vector3 currentUp;

        float pitch = 0;
        float addedYaw = 0;

        Vector3 currentCharacterForward;
        Quaternion currentCharacterRotation;

        PlayerController player;

        public void Initialize(Transform cameraTarget, PlayerController playerController) {
            player = playerController;
            motor = player.Motor;
            target = cameraTarget;
            transform.position = cameraTarget.position;
            transform.eulerAngles = cameraTarget.eulerAngles;

            player.Movement.OnRotationUpdated += RotationUpdated;
            
            //player.MainCamera.fieldOfView = PlayerSettings.FOV;
            //PlayerSettings.s_FovReactive.Subscribe(player.MainCamera, (value, cam) => cam.fieldOfView = value);
        }

        void OnDestroy() {
            player.Movement.OnRotationUpdated -= RotationUpdated;
        }

        public void UpdateRotation(GameplayInputReader input) {
            // Reset the added yaw when the character's forward vector changes
            // if (currentCharacterForward != motor.CharacterForward) {
            //     currentCharacterForward = motor.CharacterForward;
            //     addedYaw = 0;
            // }

            // if (currentCharacterRotation != motor.InitialTickRotation) {
            //     currentCharacterRotation = motor.InitialTickRotation;
            //     addedYaw = 0;
            // }

            // input.look.y moves the camera up and down along the local x-axis
            // input.look.x moves the camera left and right along the local y-axis
            var addedRotationEuler =
                new Vector3(-input.LookDirectionDelta.y, input.LookDirectionDelta.x) * (sensitivity)/* * PlayerSettings.Sensitivity*/;
            pitch = Mathf.Clamp(pitch + addedRotationEuler.x, -85f, 85f);
            addedYaw += addedRotationEuler.y;

            // rotate horizontally (addedYaw) along motor.CharacterUp
            // rotate vertically (pitch) along motor.CharacterRight
            Quaternion yawRotation = Quaternion.AngleAxis(addedYaw, motor.CharacterUp);
            Quaternion pitchRotation = Quaternion.AngleAxis(pitch, motor.CharacterRight);

            // Apply yaw rotation first, then pitch rotation
            Quaternion newRotation = yawRotation * pitchRotation;

            // Apply the new rotation to the camera with the character's forward and up vectors
            transform.rotation = newRotation * Quaternion.LookRotation(motor.CharacterForward, motor.CharacterUp);
        }

        void RotationUpdated() {
            addedYaw = 0;
        }

        public void UpdatePosition() {
            transform.position = target.position;
            //transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, target.eulerAngles.z);
        }

        public void AddRotation(Quaternion rotation) {
            // Add the rotation to the camera's current rotation
            var eulerAngles = rotation.eulerAngles;
            pitch = Mathf.Clamp(pitch + eulerAngles.x, -85f, 85f);
            addedYaw += eulerAngles.y;

            // rotate horizontally (addedYaw) along motor.CharacterUp
            // rotate vertically (pitch) along motor.CharacterRight
            Quaternion yawRotation = Quaternion.AngleAxis(addedYaw, motor.CharacterUp);
            Quaternion pitchRotation = Quaternion.AngleAxis(pitch, motor.CharacterRight);

            // Apply yaw rotation first, then pitch rotation
            Quaternion newRotation = yawRotation * pitchRotation;

            // Apply the new rotation to the camera with the character's forward and up vectors
            transform.rotation = newRotation * Quaternion.LookRotation(motor.CharacterForward, motor.CharacterUp);
        }
        
        public void SetFacingDirection(Vector3 direction) {
            // Calculate the target rotation based on the direction
            Quaternion targetRotation = Quaternion.LookRotation(direction, motor.CharacterUp);
            
            // calculate yaw difference from current rotation to new rotation
            // and add it to addedYaw
            Vector3 currentEulerAngles = transform.rotation.eulerAngles;
            Vector3 targetEulerAngles = targetRotation.eulerAngles;
            float yawDifference = Mathf.DeltaAngle(currentEulerAngles.y, targetEulerAngles.y);
            addedYaw += yawDifference;
            
            // calculate pitch difference from current rotation to new rotation
            // and add it to pitch
            float pitchDifference = Mathf.DeltaAngle(currentEulerAngles.x, targetEulerAngles.x);
            pitch = Mathf.Clamp(pitch + pitchDifference, -85f, 85f);
            
            // Update the camera's rotation to match the character's forward direction
            Quaternion yawRotation = Quaternion.AngleAxis(addedYaw, motor.CharacterUp);
            Quaternion pitchRotation = Quaternion.AngleAxis(pitch, motor.CharacterRight);
            Quaternion newRotation = yawRotation * pitchRotation;
            transform.rotation = newRotation * Quaternion.LookRotation(motor.CharacterForward, motor.CharacterUp);
        }
    }
}