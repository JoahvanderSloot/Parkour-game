using UnityEngine;

namespace PlayerSystems.Movement.CameraEffects
{
    public class SpeedFOV : MonoBehaviour
    {
        [SerializeField] private float maxConsideredSpeed = 100f;
        [Space]
        [SerializeField] private float crouchedFOVMultiplier = 0.8f;
        [SerializeField] private float slidingFOVMultiplier = 1.1f;
        [SerializeField] private float maxFOVMultiplier = 1.5f;
        [Space]
        [SerializeField] private float response = 10f;
        
        //private FOVTweener fovTweener;
        private float DefaultFOV => 90 /*Settings.FieldOfView*/;
        
        // public void Initialize(FOVTweener fovTweener)
        // {
        //     this.fovTweener = fovTweener;
        // }
        //
        // public void UpdateFOV(float deltaTime, float speed, Stance stance)
        // {
        //     fovTweener.UpdateCurrentFOV(deltaTime, GetFOV(speed, stance), response);
        // }
        
        private float GetFOV(float speed, Stance stance)
        {
            var minFOV = stance switch
            {
                Stance.Crouch => DefaultFOV * crouchedFOVMultiplier,
                Stance.Slide => DefaultFOV * slidingFOVMultiplier,
                _ => DefaultFOV
            };
            
            var maxFOV = DefaultFOV * maxFOVMultiplier;
            
            return (speed-0f) * ((maxFOV-minFOV)/(maxConsideredSpeed-0f)) + minFOV;
        }
    }
}