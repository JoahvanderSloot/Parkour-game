

// using Debugging;
// using DefaultNamespace;

namespace PlayerSystems.Movement.CameraEffects
{
    // public class FOVTweener
    // {
    //     private static float maxRequestTime = 30f;
    //     
    //     private static FOVTweener instance;
    //     public static float CurrentFOV => instance.mainCamera.fieldOfView;
    //     public static float CurrentScaledFOV { get; private set; } = 90f;
    //     
    //     // ReSharper disable InconsistentNaming
    //     private readonly Dictionary<object, float> FOVChangeRequests = new Dictionary<object, float>();
    //     private readonly Dictionary<object, float> FOVChangeRequestTimes = new Dictionary<object, float>();
    //     private Tween FOVTween;
    //     // ReSharper restore InconsistentNaming
    //     
    //     private Camera mainCamera;
    //     
    //     private CountdownTimer checkFovRequestsTimer;
    //     private const float C_CheckFOVRequestsInterval = 5f;
    //     
    //     public static FOVTweener Initialize(Camera camera)
    //     {
    //         if (instance is not null)
    //             instance.mainCamera = camera;
    //         
    //         instance ??= new FOVTweener(camera);
    //         instance.InitializeFOVRequestTimer();
    //         return instance;
    //     }
    //     private FOVTweener(Camera camera)
    //     {
    //         mainCamera = camera;
    //     }
    //     
    //     private void InitializeFOVRequestTimer()
    //     {
    //         checkFovRequestsTimer = new CountdownTimer(C_CheckFOVRequestsInterval);
    //         
    //         checkFovRequestsTimer.OnTimerStop += () =>
    //         {
    //             foreach (var request in FOVChangeRequestTimes
    //                          .Where(request => Time.time - request.Value > maxRequestTime))
    //             {
    //                 //DBug.LogWarning($"FOV request from {request.Key} has been active for too long.. Ending request");
    //                 EndFOVRequest(request.Key);
    //                 checkFovRequestsTimer.Reset();
    //             }
    //         };
    //         
    //         checkFovRequestsTimer.Start();
    //     }
    //     
    //     public void UpdateCurrentFOV(float deltaTime, float fov, float response)
    //     {
    //         //checkFovRequestsTimer.Tick(deltaTime);
    //         
    //         CurrentScaledFOV = fov;
    //     
    //         if (FOVChangeRequests.Count != 0) return;
    //         
    //         mainCamera.fieldOfView = Mathf.Lerp(
    //             mainCamera.fieldOfView,
    //             fov,
    //             1f - Mathf.Exp(-response * deltaTime)
    //         );
    //     }
    //     
    //     private async void RequestFOV(object sender, float targetFOV, float duration, float easeInDuration, float easeOutDuration, Ease ease)
    //     {
    //         var requestTime = Time.time;
    //         var waitTime = duration - easeInDuration - easeOutDuration;
    //         
    //         if (maxRequestTime < duration) maxRequestTime = duration;
    //         
    //         RequestFOV(sender, targetFOV, easeInDuration, ease);
    //         
    //         //await TaskUtils.WaitWhile(() => requestTime + waitTime > Time.time);
    //         while (requestTime + waitTime > Time.time) {
    //             await Task.Delay(100);
    //         }
    //         EndFOVRequest(sender, easeOutDuration, ease);
    //     }
    //     
    //     private void RequestFOV(object sender, float targetFOV, float easeDuration, Ease ease)
    //     {
    //         targetFOV = CurrentScaledFOV - 90/*Settings.FieldOfView*/ + targetFOV;
    //         
    //         if (FOVChangeRequests.Count == 0)
    //             TweenFOV(targetFOV, easeDuration, ease);
    //         else if (FOVChangeRequests.Values.Max() < targetFOV)
    //             TweenFOV(targetFOV, easeDuration, ease);
    //     
    //         if (!FOVChangeRequests.TryAdd(sender, targetFOV))
    //             FOVChangeRequests[sender] = targetFOV;
    //         if (!FOVChangeRequestTimes.TryAdd(sender, Time.time))
    //             FOVChangeRequestTimes[sender] = Time.time;
    //     }
    //     
    //     private void EndFOVRequest(object sender, float easeOutDuration = 0.5f, Ease ease = Ease.Default)
    //     {
    //         if (FOVChangeRequests.ContainsKey(sender)) FOVChangeRequests.Remove(sender);
    //     
    //         var targetFOV = FOVChangeRequests.Count == 0 ? CurrentScaledFOV : FOVChangeRequests.Values.Max();
    //         TweenFOV(targetFOV, easeOutDuration, ease);
    //     }
    //     
    //     private void TweenFOV(float targetFOV, float easeDuration, Ease ease)
    //     {
    //         if (FOVTween.isAlive) FOVTween.Stop();
    //         FOVTween = Tween.CameraFieldOfView(mainCamera, targetFOV, easeDuration, ease);
    //     }
    //     
    //     // ReSharper disable Unity.PerformanceAnalysis
    //     public static void REQUEST_FOV(object sender, float targetFOV, float duration, float easeInDuration, float easeOutDuration, Ease ease = Ease.Default)
    //     {
    //         if (instance is null)
    //         {
    //             //DBug.LogWarning("FOVTweener instance is null.. Creating new instance with camera.main");
    //             Initialize(Camera.main);
    //         }
    //     
    //         instance?.RequestFOV(sender, targetFOV, duration, easeInDuration, easeOutDuration, ease);
    //     }
    //     // ReSharper disable Unity.PerformanceAnalysis
    //     public static void REQUEST_FOV(object sender, float targetFOV, float easeDuration, Ease ease = Ease.Default)
    //     {
    //         if (instance is null)
    //         {
    //             //DBug.LogWarning("FOVTweener instance is null. Creating new instance with camera.main");
    //             Initialize(Camera.main);
    //         }
    //     
    //         instance?.RequestFOV(sender, targetFOV, easeDuration, ease);
    //     }
    //     // ReSharper disable Unity.PerformanceAnalysis
    //     public static void END_FOV_REQUEST(object sender, float easeOutDuration = 0.5f, Ease ease = Ease.Default)
    //     {
    //         if (instance is null)
    //         {
    //             //DBug.LogWarning("FOVTweener instance is null.. Creating new instance with camera.main");
    //             Initialize(Camera.main);
    //         }
    //         
    //         instance?.EndFOVRequest(sender, easeOutDuration, ease);
    //     }
    // }
}