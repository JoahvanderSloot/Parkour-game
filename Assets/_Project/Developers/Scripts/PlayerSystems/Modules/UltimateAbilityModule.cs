using UnityEngine;

namespace PlayerSystems.Modules {
    public abstract class UltimateAbilityModule : ScriptableObject, IPlayerModule {
        public abstract void Initialize();

        // Interface implementation

        public abstract ModuleLevel ModuleLevel { get; }
        public virtual bool AllowBaseModuleActivation => false;
        public bool Enabled { get; private set; }
        public PlayerController Player { get; private set; }
        public bool CannotBeOverridden { get; }
        public abstract bool ShouldActivate { get; }

        public void InitializeModule(PlayerController playerController) {
            Player = playerController;
            Initialize();
        }
        public virtual void EnableModule() {
            if (Enabled) {
                Debug.LogWarning($"Module {GetType().Name} is already enabled.");
            }
            
            Enabled = true;
        }
        public virtual void DisableModule() {
            if (!Enabled) {
                Debug.LogWarning($"Module {GetType().Name} is already disabled.");
                return;
            }
            
            Enabled = false;
        }
        public abstract void ModuleUpdate();
    }
}