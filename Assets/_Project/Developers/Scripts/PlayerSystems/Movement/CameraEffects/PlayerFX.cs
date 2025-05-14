using UnityEngine;

namespace PlayerSystems.Movement.CameraEffects {
    public class PlayerFX : MonoBehaviour {
        //[SerializeField] private Volume volume;
        [Space] [SerializeField] private CameraSpring cameraSpring;
        [SerializeField] private CameraLean cameraLean;

        [SerializeField] private CameraBob cameraBob;

        // [SerializeField] private CameraSpring weaponCameraSpring;
        // [SerializeField] private CameraLean weaponCameraLean;
        [SerializeField] private StanceVignette stanceVignette;
        [SerializeField] private SpeedFOV speedFOV;

        //private PlayerMediator playerMediator;
        //[SerializeField] private VignetteSettings vignetteSettings;

        //public static FOVTweener FOV { get; private set; }

        PlayerController player;
        PlayerMovement playerMovement;

        public CameraSpring CameraSpring => cameraSpring;
        public CameraLean CameraLean => cameraLean;
        public CameraBob CameraBob => cameraBob;

        public void Initialize(PlayerController playerController, Camera camera) {
            player = playerController;
            playerMovement = playerController.Movement;

            cameraSpring.Initialize();
            cameraLean.Initialize();
            cameraBob.Initialize();

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