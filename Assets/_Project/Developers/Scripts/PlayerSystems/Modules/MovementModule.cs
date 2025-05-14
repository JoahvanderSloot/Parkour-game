using System.Collections.Generic;
using PlayerSystems.Input;
using UnityEngine;

namespace PlayerSystems.Modules {
    public abstract class MovementModule : ScriptableObject, IPlayerModule {
        [SerializeField] List<MovementModule> CannotOverride;
        MovementModule[] modulesToActivateInTandem;
        
        public bool CanOverride(MovementModule module) {
            return !CannotOverride.Contains(module);
        }

        protected abstract void Initialize();
        protected GameplayInputReader Input { get; private set; }
        
        protected Vector3 RequestedMovement => Player.Movement.RequestedMovementDirection;

        // Interface implementation

        public abstract ModuleLevel ModuleLevel { get; }
        public bool Enabled { get; private set; }
        public PlayerController Player { get; private set; }
        public bool CannotBeOverridden { get; protected set; }
        public abstract bool ShouldActivate { get; }

        public void InitializeModule(PlayerController playerController) {
            Player = playerController;
            Input = playerController.GameplayInput;
            Initialize();
        }
        // ReSharper disable Unity.PerformanceAnalysis
        public virtual void EnableModule() {
            if (Enabled) {
                Debug.LogWarning($"Module {GetType().Name} is already enabled.");
            }
            
            Enabled = true;
        }
        // ReSharper disable Unity.PerformanceAnalysis
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