namespace PlayerSystems.Movement.CameraEffects
{
    // public enum RequestType
    // {
    //     Set,
    //     AddToBase,
    //     MultiplyBase
    // }
    //
    // [Serializable]
    // public struct VolumeSettings
    // {
    //     [Range(0, 1)]
    //     public float Weight;
    //     public int Priority;
    //     
    //     public static VolumeSettings Default => new VolumeSettings
    //     {
    //         Weight = 1f,
    //         Priority = 0
    //     };
    // }
    //
    // public static class EffectManager
    // {
    //     public static VignetteEffect VignetteEffect;
    //     
    //     [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    //     private static void InitializeEffects()
    //     {
    //         NullStatics();
    //         
    //         UnityEngine.SceneManagement.SceneManager.sceneLoaded += (scene, mode) =>
    //         {
    //             ResetEffects();
    //         };
    //         
    //         // Modify The player loop to include effect update
    //         {
    //             // Get the current player loop
    //             var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
    //
    //             // Find the LateUpdate phase
    //             for (var i = 0; i < playerLoop.subSystemList.Length; i++)
    //             {
    //                 if (playerLoop.subSystemList[i].type != typeof(PostLateUpdate)) continue;
    //                 
    //                 // Create a new PlayerLoopSystem for effect updates
    //                 var effectUpdate = new PlayerLoopSystem
    //                 {
    //                     type = typeof(EffectManager),
    //                     updateDelegate = UpdateEffects
    //                 };
    //
    //                 // Insert UpdateEffects into the PostLateUpdate phase
    //                 var subSystemList = playerLoop.subSystemList[i].subSystemList.ToList();
    //                 subSystemList.Add(effectUpdate);
    //                 playerLoop.subSystemList[i].subSystemList = subSystemList.ToArray();
    //                 break;
    //             }
    //             
    //             // Set the modified player loop
    //             PlayerLoop.SetPlayerLoop(playerLoop);
    //         }
    //
    //
    //         var volumeGameObject = new GameObject("EffectVolume [Auto Generated]", typeof(Volume));
    //         Object.DontDestroyOnLoad(volumeGameObject);
    //         
    //         // TODO: Add a way to load settings from a scriptable object (weight and priority)
    //         var volume = volumeGameObject.GetComponent<Volume>();
    //         var volumeProfile = volume.profile;
    //
    //         EffectSettings effectSettings;
    //         
    //         try
    //         {
    //             effectSettings = Resources.Load<EffectSettings>("EffectSettings");
    //         }
    //         catch (Exception e)
    //         {
    //             effectSettings = ScriptableObject.CreateInstance<EffectSettings>();
    //             effectSettings.GetDefaults();
    //             throw new ArgumentNullException($"Could not find EffectSettings in Resources.. exception: ", e);
    //         }
    //
    //         effectSettings.SetDefaults();
    //         
    //         // Initialize all effects
    //         VignetteEffect = VignetteEffect.CreateAndInitialize(volumeProfile, effectSettings.VignetteSettings);
    //     }
    //     
    //     private static void UpdateEffects()
    //     {
    //         if (!Application.isPlaying) return;
    //         
    //         var deltaTime = Time.deltaTime;
    //         var unscaledDeltaTime = Time.unscaledDeltaTime;
    //
    //         // Update all effects
    //         VignetteEffect.UpdateVignette(deltaTime, unscaledDeltaTime);
    //     }
    //
    //     private static void NullStatics()
    //     {
    //         VignetteEffect = null;
    //     }
    //     
    //     private static void ResetEffects()
    //     {
    //         VignetteEffect?.EndAllRequests(1000f);
    //     }
    //     
    //     private static void DisposeEffects()
    //     {
    //         Debug.Log("Disposing Effects: " + VignetteEffect);
    //         VignetteEffect?.Dispose();
    //     }
    // }
}