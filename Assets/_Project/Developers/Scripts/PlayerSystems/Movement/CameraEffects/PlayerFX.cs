using UnityEngine;

namespace PlayerSystems.Movement.CameraEffects {
    public class PlayerFX : MonoBehaviour {
        //[SerializeField] private Volume volume;
        [Space] [SerializeField] CameraSpring cameraSpring;
        [SerializeField] CameraLean cameraLean;

        [SerializeField] CameraBob cameraBob;
        [SerializeField] CameraTilt cameraTilt;
        [SerializeField] CameraOffset cameraOffset;

        // [SerializeField] private CameraSpring weaponCameraSpring;
        // [SerializeField] private CameraLean weaponCameraLean;
        [SerializeField] StanceVignette stanceVignette;
        [SerializeField] SpeedFOV speedFOV;

        //private PlayerMediator playerMediator;
        //[SerializeField] private VignetteSettings vignetteSettings;

        //public static FOVTweener FOV { get; private set; }

        PlayerController player;
        PlayerMovement playerMovement;

        public CameraSpring CameraSpring => cameraSpring;
        public CameraLean CameraLean => cameraLean;
        public CameraBob CameraBob => cameraBob;
        public CameraTilt CameraTilt => cameraTilt;
        public CameraOffset CameraOffset => cameraOffset;

        public void Initialize(PlayerController playerController, Camera camera) {
            player = playerController;
            playerMovement = playerController.Movement;

            cameraSpring.Initialize();
            cameraLean.Initialize();
            cameraBob.Initialize();
            cameraTilt.Initialize();
            cameraOffset.Initialize();

            // Initialize effects
            // Vignette = VignetteEffect.CreateAndInitialize(vignetteSettings.Volume.profile);
            //FOV = FOVTweener.Initialize(camera);

            // Initialize player effects
            //speedFOV.Initialize(FOV);
        }

        public void UpdateEffects(float deltaTime) {
            var unscaledDeltaTime = Time.unscaledDeltaTime;

            var cameraTarget = playerMovement.GetCameraTarget();
            var currentState = playerMovement.GetState();

            cameraSpring.UpdateSpring(deltaTime, cameraTarget.up);
            cameraLean.UpdateLean(deltaTime, currentState.Stance is Stance.Slide, currentState.Velocity,
                cameraTarget.up);
            cameraBob.UpdateBob(deltaTime);
            cameraTilt.UpdateTilt(deltaTime);
            cameraOffset.UpdateOffset(deltaTime);
            
            // weaponCameraSpring.UpdateSpring(deltaTime, cameraTarget.up);
            // weaponCameraLean.UpdateLean(deltaTime, currentState.Stance is Stance.Slide, currentState.Velocity, cameraTarget.up);
            //
            // Update player effects
            //stanceVignette.UpdateVignette(deltaTime, currentState.Stance);
            //speedFOV.UpdateFOV(deltaTime, currentState.Velocity.magnitude, currentState.Stance);

            // Update all effects
            //Vignette.UpdateVignette(deltaTime, unscaledDeltaTime);
            //FOV.UpdateFOV(deltaTime);
        }
    }
}