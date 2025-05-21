using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PlayerSystems.Modules {
    public class PlayerModules : MonoBehaviour {
        public event Action<IPlayerModule> OnModuleAdded;
        public event Action<IPlayerModule> OnModuleRemoved;
        public event Action<MovementAbilityModule> OnMovementAbilityModuleChanged;
        public event Action<UltimateAbilityModule> OnUltimateAbilityModuleChanged;
        
        [SerializeField] List<MovementModule> movementModules;
        [SerializeField] List<EffectModule> effectModules;
        
        [SerializeField] MovementAbilityModule movementAbilityModule;
        [SerializeField] UltimateAbilityModule ultimateAbilityModule;

        IPlayerModule activeModule;
        MovementModule activeBaseModule;
        
        PlayerController playerController;

        public void InitializeModules(PlayerController player) {
            playerController = player;
            player.Movement.BeforeVelocityUpdate += CheckMovementModuleActivation;
            
            foreach (var module in movementModules) {
                module.InitializeModule(player);
            }
            foreach (var module in effectModules) {
                module.InitializeModule(player);
            }
            movementAbilityModule?.InitializeModule(player);
            ultimateAbilityModule?.InitializeModule(player);
        }

        void OnDestroy() {
            playerController.Movement.BeforeVelocityUpdate -= CheckMovementModuleActivation;
        }
        
        public void OnDisable() {
            foreach (var module in movementModules.Where(module => module.Enabled)) {
                module.DisableModule();
            }
            
            foreach (var module in effectModules.Where(module => module.Enabled)) {
                module.DisableModule();
            }
            
            if (movementAbilityModule && movementAbilityModule.Enabled)
                movementAbilityModule.DisableModule();

            if (ultimateAbilityModule && ultimateAbilityModule.Enabled)
                ultimateAbilityModule?.DisableModule();
        }

        public void ActivateModule(IPlayerModule module) {
            switch (module) {
                case MovementModule movementModule:
                    if (movementModule.ModuleLevel is ModuleLevel.BaseModule) {
                        if (movementModule.CanOverride(activeBaseModule)) {
                            activeBaseModule?.DisableModule();
                            activeBaseModule = movementModule;
                            activeBaseModule.EnableModule();
                        }
                    }
                    else if (activeModule == null || activeModule is MovementModule mm && movementModule.CanOverride(mm)) {
                        activeModule?.DisableModule();
                        activeModule = movementModule;
                        activeModule.EnableModule();
                    }
                    break;
                case EffectModule effectModule:
                    effectModule.EnableModule();
                    break;
                case MovementAbilityModule movAbilityModule:
                    activeModule?.DisableModule();
                    activeModule = movAbilityModule;
                    activeModule.EnableModule();
                    break;
                case UltimateAbilityModule ultAbilityModule:
                    activeModule?.DisableModule();
                    activeModule = ultAbilityModule;
                    activeModule.EnableModule();
                    break;
                default:
                    Debug.LogWarning("Invalid module type for activation");
                    return;
            }
            
            if (!module.AllowBaseModuleActivation && module.ModuleLevel is not ModuleLevel.BaseModule)
                activeBaseModule?.DisableModule();
        }

        void CheckMovementModuleActivation() {
            if (movementAbilityModule?.ShouldActivate == true && CanActivateModule(movementAbilityModule)) 
                ActivateModule(movementAbilityModule);
            else if (ultimateAbilityModule?.ShouldActivate == true && CanActivateModule(ultimateAbilityModule))
                ActivateModule(ultimateAbilityModule);
            
            var baseModulesAllowed = activeModule?.AllowBaseModuleActivation ?? true;
            
            foreach (var module in movementModules) {
                if (module.ModuleLevel is ModuleLevel.BaseModule && !baseModulesAllowed)
                    continue;
                
                if (module.ShouldActivate && module.Enabled == false && CanActivateModule(module)) {
                    ActivateModule(module);
                }
            }
        }

        // TODO: Implement module removal after UpdateModules (if module removed during UpdateModules, it will throw an error)
        public void UpdateModules() {
            if (activeModule?.Enabled == true)
                activeModule?.ModuleUpdate();
            else 
                activeModule = null;

            if (activeBaseModule?.Enabled == true)
                activeBaseModule?.ModuleUpdate();
            else
                activeBaseModule = null;
            
            foreach (var module in effectModules) {
                if (module.Enabled) {
                    module.ModuleUpdate();
                }
                else if (module.ShouldActivate) {
                    ActivateModule(module);
                }
            }
        }

        bool CanActivateModule(IPlayerModule module) {
            if (module.ModuleLevel is ModuleLevel.BaseModule)
                return true;
            
            bool isHigherOrEqualLevel = activeModule == null || module.ModuleLevel >= activeModule.ModuleLevel;
            bool canOverride = activeModule == null || activeModule.CannotBeOverridden == false;
            return canOverride && isHigherOrEqualLevel;
        }
 
        public void AddModule(IPlayerModule module) {
            RemoveDuplicateModule(module);
            module.InitializeModule(playerController);
            
            switch (module) {
                case MovementModule movementModule:
                    movementModules.Add(movementModule);
                    break;
                case EffectModule effectModule:
                    effectModules.Add(effectModule);
                    break;
                case MovementAbilityModule:
                    Debug.LogWarning("Use SetMovementAbilityModule instead.");
                    return;
                case UltimateAbilityModule:
                    Debug.LogWarning("Use SetUltimateAbilityModule instead.");
                    return;
                default:
                    Debug.LogWarning("Invalid module type for addition");
                    return;
            }

            OnModuleAdded?.Invoke(module);
        }
        
        bool RemoveDuplicateModule(IPlayerModule module) {
            IPlayerModule duplicateModule = null;
            
            switch (module) {
                case MovementModule movementModule:
                    duplicateModule = movementModules.Find(mod => mod.GetType() == module.GetType());
                    if (duplicateModule != null)
                        movementModules.Remove((MovementModule)duplicateModule);
                    break;
                case EffectModule effectModule:
                    duplicateModule = effectModules.Find(mod => mod.GetType() == module.GetType());
                    if (duplicateModule != null)
                        effectModules.Remove((EffectModule)duplicateModule);
                    break;
                default:
                    return false;
            }
            
            return duplicateModule != null;
        }

        public void RemoveModule(IPlayerModule module) {
            module.InitializeModule(playerController);
            
            switch (module) {
                case MovementModule movementModule:
                    movementModules.Remove(movementModule);
                    break;
                case EffectModule effectModule:
                    effectModules.Remove(effectModule);
                    break;
                default:
                    Debug.LogWarning("Invalid module type for removal");
                    return;
            }
            
            OnModuleRemoved?.Invoke(module);
        }
        
        public void SetMovementAbilityModule(MovementAbilityModule module) {
            movementAbilityModule = module;
            module.InitializeModule(playerController);
            OnMovementAbilityModuleChanged?.Invoke(module);
        }

        public void SetUltimateAbilityModule(UltimateAbilityModule module) {
            ultimateAbilityModule = module;
            module.InitializeModule(playerController);
            OnUltimateAbilityModuleChanged?.Invoke(module);
        }

        public MovementAbilityModule GetMovementAbilityModule() => movementAbilityModule;
        public UltimateAbilityModule GetUltimateAbilityModule() => ultimateAbilityModule;
        
        public bool TryGetModule<T>(out T module) where T : class, IPlayerModule {
            module = null;

            switch (typeof(T)) {
                case { } t when typeof(MovementModule).IsAssignableFrom(t):
                    module = movementModules.Find(m => m is T) as T;
                    break;
                case { } t when typeof(EffectModule).IsAssignableFrom(t):
                    module = effectModules.Find(e => e is T) as T;
                    break;
                case { } t when typeof(MovementAbilityModule).IsAssignableFrom(t):
                    if (movementAbilityModule is T movAbilityModule) {
                        module = movAbilityModule;
                    }
                    break;
                case { } t when typeof(UltimateAbilityModule).IsAssignableFrom(t):
                    if (ultimateAbilityModule is T ultAbilityModule) {
                        module = ultAbilityModule;
                    }
                    break;
            }
            
            return module != null;
        }
    }
}