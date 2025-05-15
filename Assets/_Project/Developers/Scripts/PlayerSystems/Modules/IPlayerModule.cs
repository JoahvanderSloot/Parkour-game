namespace PlayerSystems.Modules {
    public enum ModuleLevel {
        BaseModule,
        AutomaticActivationModule,
        ManualActivationModule
    }
    
    public interface IPlayerModule {
        ModuleLevel ModuleLevel { get; }
        
        bool Enabled { get; }
        PlayerController Player { get; }

        bool CannotBeOverridden { get; }
        bool ShouldActivate { get; }
        
        void InitializeModule(PlayerController playerController);
        void EnableModule();
        void DisableModule();
        void ModuleUpdate();
    }
}