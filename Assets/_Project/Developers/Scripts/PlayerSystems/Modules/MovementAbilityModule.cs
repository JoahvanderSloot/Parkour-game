using ImprovedTimers;
using PlayerSystems.Input;
using UnityEngine;

namespace PlayerSystems.Modules {
    public enum CooldownStartMode {
        OnEnable,
        OnDisable,
        Custom
    }
    
    public abstract class MovementAbilityModule : ScriptableObject, IPlayerModule {
        [SerializeField] CooldownStartMode cooldownStartMode;
        [SerializeField] float cooldown;
        protected CountdownTimer cooldownTimer;
        protected abstract void Initialize();

        protected GameplayInputReader Input { get; private set; }
        
        bool activationRequested;
        
        void HandleInput(ButtonPhase phase) {
            if (phase == ButtonPhase.Pressed)
                activationRequested = true;
        }
        bool ShouldActivateInternal() {
            var shouldActivate = activationRequested && cooldownTimer.IsFinished;
            activationRequested = false;
            return shouldActivate;
        }

        void OnDisable() {
            if (Application.isPlaying && Input != null)
                Input.MovementAbility -= HandleInput;
        }
        
        // Interface implementation

        public abstract ModuleLevel ModuleLevel { get; }
        public bool Enabled { get; private set; }
        public PlayerController Player { get; private set; }
        public bool CannotBeOverridden { get; protected set; }
        public virtual bool ShouldActivate => ShouldActivateInternal();

        public void InitializeModule(PlayerController playerController) {
            Player = playerController;
            Input = playerController.GameplayInput;
            cooldownTimer = new CountdownTimer(cooldown);

            Input.MovementAbility += HandleInput;
            
            Initialize();
        }
        // ReSharper disable Unity.PerformanceAnalysis
        public virtual void EnableModule() {
            if (Enabled) {
                Debug.LogWarning($"Module {GetType().Name} is already enabled.");
            }
            
            if (cooldownStartMode == CooldownStartMode.OnEnable) {
                cooldownTimer.Reset(cooldown);
                cooldownTimer.Start();
            }
            
            Enabled = true;
        }
        // ReSharper disable Unity.PerformanceAnalysis
        public virtual void DisableModule() {
            if (!Enabled) {
                Debug.LogWarning($"Module {GetType().Name} is already disabled.");
                return;
            }
            
            if (cooldownStartMode == CooldownStartMode.OnDisable) {
                cooldownTimer.Start();
            }
            
            Enabled = false;
        }
        public abstract void ModuleUpdate();
    }
}