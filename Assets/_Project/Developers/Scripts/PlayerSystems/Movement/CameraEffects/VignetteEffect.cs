// using System;
// using System.Collections.Generic;
// using System.Diagnostics.CodeAnalysis;
// using System.Linq;
// using UnityEngine;
// using UnityEngine.Rendering;
// using UnityEngine.Rendering.Universal;
// using UnityEngine.Serialization;
// using UnityUtils;
//
// //using Debugging;
//
// namespace Player.Movement.CameraEffects
// {
//     public class VignetteEffect : IDisposable
//     {
//         private readonly List<VignetteRequest> requests = new ();
//         private readonly Dictionary<int, List<VignetteRequest>> requestsPerPriority = new ();
//         private readonly Dictionary<int, VignetteRequest> highestRequestPerPriority = new ();
//
//         private readonly VolumeProfile volumeProfile;
//         private readonly VignetteSettings settings;
//         private Vignette vignette;
//
//         public float BaseIntensity { get; private set; }
//         private VignetteRequest currentRequest;
//         private float currentResponse;
//         private bool useUnscaledTime;
//
//         public bool AnyActive => !currentRequest.Equals(VignetteRequest.Default);
//         public VignetteRequest GetCurrentRequest() => currentRequest;
//         public bool TryGetCurrentRequest(out VignetteRequest request) => !(request = GetCurrentRequest()).Equals(VignetteRequest.Default);
//         public List<VignetteRequest> GetRequests() => requests.ToList();
//         public bool TryGetRequests(out List<VignetteRequest> requests) => (requests = GetRequests()).Count != 0;
//         public List<VignetteRequest> GetRequestsFrom(object sender) => requests.Where(request => request.Sender.Equals(sender)).ToList();
//         public bool TryGetRequestsFrom(object sender, out List<VignetteRequest> requestsFrom) => (requestsFrom = GetRequestsFrom(sender)).Count != 0;
//         public List<VignetteRequest> GetRequestsWithPriority(int priority) => requestsPerPriority.TryGetValue(priority, out var value) ? value : new List<VignetteRequest>();
//         public bool TryGetRequestsWithPriority(int priority, out List<VignetteRequest> requestsWithPriority) => (requestsWithPriority = GetRequestsWithPriority(priority)).Count != 0;
//         
//         public static VignetteEffect CreateAndInitialize(VolumeProfile profile, VignetteSettings settings)
//         {
//             var newEffect = new VignetteEffect(profile, settings);
//             
//             if (!newEffect.volumeProfile.TryGet(out newEffect.vignette))
//                 newEffect.vignette = newEffect.volumeProfile.Add<Vignette>();
//             
//             newEffect.vignette.intensity.Override(settings.Intensity);
//             newEffect.vignette.smoothness.Override(settings.Smoothness);
//             newEffect.vignette.color.Override(settings.Color);
//             newEffect.vignette.center.Override(settings.Center);
//             
//             newEffect.SetCurrentRequest(VignetteRequest.Default);
//             
//             return newEffect;
//         }
//         private VignetteEffect(VolumeProfile profile, VignetteSettings settings)
//         {
//             volumeProfile = profile;
//             this.settings = settings;
//         }
//         ~VignetteEffect() => Dispose();
//         
//         public void SetBaseIntensity(float intensity)
//         {
//             BaseIntensity = intensity;
//          
//             if (requests.Count == 0) return;
//             
//             // Reorder requestsPerPriority and highestRequestPerPriority based on new base intensity
//             foreach (var key in highestRequestPerPriority.Keys.ToList())
//             {
//                 if (requestsPerPriority.TryGetValue(key, out var value))
//                 {
//                     if (value.Count == 0)
//                     {
//                         highestRequestPerPriority.Remove(key);
//                         continue;
//                     }
//                     
//                     highestRequestPerPriority[key] = value.Max();
//                 }
//                 else
//                 {
//                     //DBug.LogError("Highest request for priority does not exist in requestsPerPriority..");
//                 }
//             }
//             
//             // Set new current request if it is the highest priority
//             var highestRequest = highestRequestPerPriority[highestRequestPerPriority.Keys.Max()];
//             if (!currentRequest.Equals(highestRequest)) SetCurrentRequest(highestRequest);
//         }
//         
//         public void UpdateVignette(float deltaTime, float unscaledDeltaTime)
//         {
//             deltaTime = useUnscaledTime ? unscaledDeltaTime : deltaTime;
//             
//             var desiredIntensity = currentRequest.Equals(VignetteRequest.Default) ? BaseIntensity : currentRequest.GetActualIntensity();
//             
//             var intensity = Mathf.Lerp(
//                 vignette.intensity.value,
//                 desiredIntensity,
//                 1f - Mathf.Exp(-currentResponse * deltaTime)
//             );
//
//             var smoothness = Mathf.Lerp(
//                 vignette.smoothness.value,
//                 currentRequest.Smoothness,
//                 1f - Mathf.Exp(-currentResponse * deltaTime)
//             );
//
//             var color = Color.Lerp(
//                 vignette.color.value,
//                 currentRequest.Color,
//                 1f - Mathf.Exp(-currentResponse * deltaTime)
//             );
//
//             var center = Vector2.Lerp(
//                 vignette.center.value,
//                 currentRequest.Center,
//                 1f - Mathf.Exp(-currentResponse * deltaTime)
//             );
//
//             vignette.intensity.value = Mathf.Clamp(intensity, settings.MinIntensity, settings.MaxIntensity);
//             vignette.smoothness.value = Mathf.Clamp(smoothness, settings.MinSmoothness, settings.MaxSmoothness);
//             vignette.color.value = color;
//             vignette.center.value = center;
//         }
//         
//         public void Request(VignetteRequest request)
//         {
//             if (request.Sender is null)
//                 throw new NullReferenceException($"Trying to make a VignetteRequest with no sender.. Request: {request}");
//             
//             if (requests.Contains(request))
//                 //DBug.LogWarning($"Trying to add a VignetteRequest that already exists.. Request: {request}");
//             
//             requests.Add(request);
//             
//             if (requestsPerPriority.TryGetValue(request.Priority, out var value))
//                 value.Add(request);
//             else
//                 requestsPerPriority.Add(request.Priority, new List<VignetteRequest> { request });
//             
//             if (currentRequest.Equals(VignetteRequest.Default) || request.Priority > currentRequest.Priority)
//                 SetCurrentRequest(request);
//             else if (request.Priority == currentRequest.Priority && request > currentRequest)
//                 SetCurrentRequest(request);
//
//             if (highestRequestPerPriority.TryGetValue(request.Priority, out var highestRequest))
//             {
//                 if (request > highestRequest)
//                     highestRequestPerPriority[request.Priority] = request;
//             }
//             else
//                 highestRequestPerPriority.Add(request.Priority, request);   
//         }
//
//         public void UpdateRequest(VignetteRequest request, VignetteRequest newRequest)
//         {
//             var exists = requests.Contains(request);
//             if (!exists)
//                 throw new Exception($"Trying to update a request that does not exist.. Request: {request}");
//             
//             RemoveRequest(request);
//             Request(newRequest);
//         }
//         
//         public void EndAllRequests(float returnResponse = float.MinValue)
//         {
//             if (requests.Count == 0)
//                 return;
//             
//             requests.Clear();
//             requestsPerPriority.Values.ToList().ForEach(list => list.Clear());
//             
//             if (returnResponse.Approx(float.MinValue))
//                 returnResponse = currentRequest.Response;
//             
//             var def = VignetteRequest.Default;
//             var defaultRequestWithResponse = VignetteRequest.New(def.Intensity, returnResponse, def.Smoothness, def.Priority, def.RequestType, def.Color, def.Center, def.UseUnscaledTime);
//             SetCurrentRequest(defaultRequestWithResponse);
//         }
//         public void EndRequestsFrom(object sender, float returnResponse = float.MinValue)
//         {
//             var requestsFrom = GetRequestsFrom(sender);
//
//             if (requestsFrom.Count == 0)
//                 return;
//
//             EndRequests(requestsFrom, returnResponse);
//         }
//         public void EndRequests(IEnumerable<VignetteRequest> requestsToEnd, float returnResponse = float.MinValue)
//         {
//             var vignetteRequests = requestsToEnd as VignetteRequest[] ?? requestsToEnd.ToArray();
//             if (vignetteRequests.Length == 0)
//                 return;
//             
//             var containsCurrentRequest = false;
//
//             foreach (var request in vignetteRequests)
//             {
//                 if (request.Equals(currentRequest))
//                 {
//                     containsCurrentRequest = true;
//                     
//                     if (returnResponse.Approx(float.MinValue))
//                         returnResponse = request.Response;
//                 }
//                 
//                 RemoveRequest(request);
//             }
//             
//             if (!containsCurrentRequest) return;
//             
//             if (requests.Count != 0)
//             {
//                 var highestRequest = highestRequestPerPriority[highestRequestPerPriority.Keys.Max()];
//                 SetCurrentRequest(highestRequest, returnResponse);
//                 return;
//             }
//
//             SetCurrentRequest(VignetteRequest.Default, returnResponse);
//         }
//         public void EndRequest(VignetteRequest request, float returnResponse = float.MinValue)
//         {
//             var isCurrentRequest = request.Equals(currentRequest);
//             RemoveRequest(request);
//             if (!isCurrentRequest) return;
//             
//             if (returnResponse.Approx(float.MinValue))
//                 returnResponse = request.Response;
//
//             if (requests.Count != 0)
//             {
//                 var highestRequest = highestRequestPerPriority[highestRequestPerPriority.Keys.Max()];
//                 SetCurrentRequest(highestRequest, returnResponse);
//                 return;
//             }
//
//             SetCurrentRequest(VignetteRequest.Default, returnResponse);
//         }
//         
//         private void SetCurrentRequest(VignetteRequest request, float response = float.MinValue)
//         {
//             var approx = response.Approx(float.MinValue);
//             
//             if (approx)
//                 useUnscaledTime = request.UseUnscaledTime;
//
//             currentResponse = approx ? request.Response : response;
//             currentRequest = request;
//         }
//         
//         private void RemoveRequest(VignetteRequest request)
//         {
//             // Request List
//             var exists = requests.Contains(request);
//             if (exists)
//                 requests.Remove(request);
//             else
//                 throw new Exception($"Trying to remove a request that does not exist.. Request: {request}");
//
//             // Priority Dictionaries
//             {
//                 // Remove request from requestListsPerPriority
//                 if (requestsPerPriority.TryGetValue(request.Priority, out var requestListForPriority))
//                 {
//                     if (requestListForPriority.Contains(request))
//                         requestListForPriority.Remove(request);
//                 }
//                 
//                 // Remove request from highestRequestPerPriority & set new request for priority
//                 if (highestRequestPerPriority.TryGetValue(request.Priority, out var highestRequest) && highestRequest.Equals(request))
//                 {
//                     if (requestListForPriority != null)
//                     {
//                         if (requestListForPriority.Count != 0)
//                             highestRequestPerPriority[request.Priority] = requestListForPriority.Max();
//                         else
//                             highestRequestPerPriority.Remove(request.Priority);
//                     }
//                     else
//                     {
//                         highestRequestPerPriority.Remove(request.Priority);
//                         //DBug.LogError("Highest request for priority does not exist in requestsPerPriority..");
//                     }
//                 }
//             }
//         }
//
//         public void Dispose()
//         {
//             EndAllRequests();
//             vignette.intensity.Override(settings.Intensity);
//             vignette.smoothness.Override(settings.Smoothness);
//             vignette.color.Override(settings.Color);
//             vignette.center.Override(settings.Center);
//         }
//     }
//     
//     [Serializable]
//     public struct VignetteSettings : IEquatable<VignetteSettings>
//     {
//         public float MinIntensity;
//         public float MaxIntensity;
//         [Space]
//         public float MinSmoothness;
//         public float MaxSmoothness;
//         [FormerlySerializedAs("DefaultColor")] [Header("Default Settings")]
//         public Color Color;
//         [FormerlySerializedAs("DefaultCenter")] public Vector2 Center;
//         [FormerlySerializedAs("DefaultIntensity")] public float Intensity;
//         [FormerlySerializedAs("DefaultSmoothness")] public float Smoothness;
//         [FormerlySerializedAs("DefaultResponse")] public float Response;
//         [FormerlySerializedAs("DefaultUseUnscaledTime")] public bool UseUnscaledTime;
//         
//         public static VignetteSettings Default => new VignetteSettings
//         {
//             MinIntensity = 0f,
//             MaxIntensity = 1f,
//             MinSmoothness = 0f,
//             MaxSmoothness = 1f,
//             Color = Color.black,
//             Center = new Vector2(0.5f, 0.5f),
//             Intensity = 0f,
//             Smoothness = 0.2f,
//             Response = 10f,
//             UseUnscaledTime = false,
//         };
//         public static VignetteSettings Zero => new VignetteSettings();
//
//         public bool Equals(VignetteSettings other)
//         {
//             return MinIntensity.Equals(other.MinIntensity) && MaxIntensity.Equals(other.MaxIntensity) && MinSmoothness.Equals(other.MinSmoothness) && MaxSmoothness.Equals(other.MaxSmoothness) && Color.Equals(other.Color) && Center.Equals(other.Center) && Intensity.Equals(other.Intensity) && Smoothness.Equals(other.Smoothness) && Response.Equals(other.Response) && UseUnscaledTime == other.UseUnscaledTime;
//         }
//
//         public override bool Equals(object obj)
//         {
//             return obj is VignetteSettings other && Equals(other);
//         }
//
//         public override int GetHashCode()
//         {
//             var hashCode = new HashCode();
//             hashCode.Add(MinIntensity);
//             hashCode.Add(MaxIntensity);
//             hashCode.Add(MinSmoothness);
//             hashCode.Add(MaxSmoothness);
//             hashCode.Add(Color);
//             hashCode.Add(Center);
//             hashCode.Add(Intensity);
//             hashCode.Add(Smoothness);
//             hashCode.Add(Response);
//             hashCode.Add(UseUnscaledTime);
//             return hashCode.ToHashCode();
//         }
//
//         public static bool operator ==(VignetteSettings left, VignetteSettings right)
//         {
//             return left.Equals(right);
//         }
//
//         public static bool operator !=(VignetteSettings left, VignetteSettings right)
//         {
//             return !left.Equals(right);
//         }
//     }
//     
//     [Serializable]
//     public struct VignetteRequest : IEquatable<VignetteRequest>, IComparable<VignetteRequest>
//     {
//         public object Sender { get; private set; }
//         public int ID { get; private set; }
//
//         [field: SerializeField] public RequestType RequestType { get; private set; }
//         [field: SerializeField] public int Priority { get; private set; }
//         [field: SerializeField] public Color Color { get; private set; }
//         [field: SerializeField] public Vector2 Center { get; private set; }
//         [field: SerializeField] public float Intensity { get; private set; }
//         [field: SerializeField] public float Smoothness { get; private set; }
//         [field: SerializeField] public float Response { get; private set; }
//         [field: SerializeField] public bool UseUnscaledTime { get; private set; }
//         
//         public bool IsSent { get; private set; }
//         public bool IsCurrent => Equals(EffectManager.VignetteEffect.GetCurrentRequest());
//         
//         public float GetActualIntensity()
//         {
//             var baseIntensity = EffectManager.VignetteEffect.BaseIntensity;
//             return RequestType switch
//             {
//                 RequestType.Set => Intensity,
//                 RequestType.AddToBase => baseIntensity + Intensity,
//                 RequestType.MultiplyBase => baseIntensity * Intensity,
//                 _ => throw new ArgumentOutOfRangeException()
//             };
//         }
//         
//         public int GenerateID()
//         {
//             var oldRequest = this;
//             ID = GetHashCode();
//             if (IsSent)
//                 EffectManager.VignetteEffect.UpdateRequest(oldRequest, this);
//             return ID;
//         }
//         
//         #region Creation
//         
//         [SuppressMessage("ReSharper", "ParameterHidesMember")]
//         public struct Builder
//         {
//             private int priority;
//             private RequestType requestType;
//             private Color color;
//             private Vector2 center;
//             private float intensity;
//             private float smoothness;
//             private float response;
//             private bool useUnscaledTime;
//             public static Builder NewWithDefaults()
//             {
//                 return new Builder()
//                 {
//                     priority = Default.Priority,
//                     requestType = Default.RequestType,
//                     color = Default.Color,
//                     center = Default.Center,
//                     intensity = Default.Intensity,
//                     smoothness = Default.Smoothness,
//                     response = Default.Response,
//                     useUnscaledTime = Default.UseUnscaledTime
//                 };
//             }
//             public static Builder NewWith(VignetteRequest request)
//             {
//                 return new Builder()
//                 {
//                     priority = request.Priority,
//                     requestType = request.RequestType,
//                     color = request.Color,
//                     center = request.Center,
//                     intensity = request.Intensity,
//                     smoothness = request.Smoothness,
//                     response = request.Response,
//                     useUnscaledTime = request.UseUnscaledTime
//                 };
//             }
//             public static Builder New()
//             {
//                 return new Builder();
//             }
//             
//             public Builder WithPriority(int priority)
//             { 
//                 this.priority = priority;
//                 return this;
//             }
//             public Builder WithRequestType(RequestType requestType)
//             { 
//                 this.requestType = requestType;
//                 return this;
//             }
//             public Builder WithColor(Color color)
//             { 
//                 this.color = color;
//                 return this;
//             }
//             public Builder WithCenter(Vector2 center)
//             { 
//                 this.center = center;
//                 return this;
//             }
//             public Builder WithIntensity(float intensity)
//             { 
//                 this.intensity = intensity;
//                 return this;
//             }
//             public Builder WithSmoothness(float smoothness)
//             { 
//                 this.smoothness = smoothness;
//                 return this;
//             }
//             public Builder WithResponse(float response)
//             { 
//                 this.response = response;
//                 return this;
//             }
//             public Builder WithUseUnscaledTime(bool useUnscaledTime = true)
//             { 
//                 this.useUnscaledTime = useUnscaledTime;
//                 return this;
//             }
//             public VignetteRequest Build()
//             {
//                 return VignetteRequest.New(intensity, response, smoothness, priority, requestType, color, center, useUnscaledTime);
//             }
//             public VignetteRequest Send(object sender)
//             {
//                 return Build().Send(sender);
//             }
//         }
//         
//         public static VignetteRequest New(float intensity)
//         {
//             return New(intensity, Default.Response);
//         }
//         
//         public static VignetteRequest New(float intensity, float response)
//         {
//             return New(intensity, response, Default.Smoothness);
//         }
//
//         public static VignetteRequest New(float intensity, float response, float smoothness, int priority = 0)
//         {
//             return New(intensity, response, smoothness, priority, RequestType.Set);
//         }
//         
//         public static VignetteRequest New(float intensity, float response, float smoothness, int priority, RequestType requestType)
//         {
//             return New(intensity, response, smoothness, priority, requestType, Default.Color);
//         }
//         
//         public static VignetteRequest New(float intensity, float response, float smoothness, int priority, RequestType requestType, Color color)
//         {
//             return New(intensity, response, smoothness, priority, requestType, color, Default.Center);
//         }
//
//         public static VignetteRequest New(float intensity, float response, float smoothness, int priority, RequestType requestType, Color color, Vector2 center)
//         {
//             return New(intensity, response, smoothness, priority, requestType, color, center, Default.UseUnscaledTime);
//         }
//
//         public static VignetteRequest New(float intensity, float response, float smoothness, int priority, RequestType requestType, Color color, Vector2 center, bool useUnscaledTime)
//         {
//             return new VignetteRequest(intensity, response, smoothness, priority, requestType, color, center, useUnscaledTime);
//         }
//         
//         private VignetteRequest(float intensity, float response, float smoothness, int priority, RequestType requestType, Color color, Vector2 center, bool useUnscaledTime)
//         {
//             Sender = null;
//             ID = 0;
//             Priority = priority;
//             RequestType = requestType;
//             Color = color;
//             Center = center;
//             Intensity = intensity;
//             Smoothness = smoothness;
//             Response = response;
//             UseUnscaledTime = useUnscaledTime;
//             IsSent = false;
//         }
//         
//         #endregion
//         
//         public static VignetteRequest Zero => New(0, 0, 0, 0, RequestType.Set, Color.black, new Vector2(0.5f, 0.5f), false);
//         public static VignetteRequest Default { get; private set; }
//         
//         [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
//         private static void SetDefaults() => SetNewDefault(VignetteSettings.Default);
//         public static void SetNewDefault(VignetteSettings settings)
//         {
//             Default = New(settings.Intensity, settings.Response, settings.Smoothness, 0, RequestType.Set, settings.Color, settings.Center, settings.UseUnscaledTime);
//         }
//         
//         public VignetteRequest Send(object sender)
//         {
//             Sender = sender;
//             GenerateID();
//             IsSent = true;
//             EffectManager.VignetteEffect.Request(this);
//             return this;
//         }
//         public VignetteRequest Update(VignetteRequest newRequest)
//         {
//             var oldRequest = this;
//             RequestType = newRequest.RequestType;
//             Color = newRequest.Color;
//             Center = newRequest.Center;
//             Intensity = newRequest.Intensity;
//             Smoothness = newRequest.Smoothness;
//             Response = newRequest.Response;
//             Priority = newRequest.Priority;
//             UseUnscaledTime = newRequest.UseUnscaledTime;
//             IsSent = newRequest.IsSent;
//             Sender = newRequest.Sender;
//             GenerateID();
//             
//             if (oldRequest.IsSent)
//                 EffectManager.VignetteEffect.UpdateRequest(oldRequest, this);
//             
//             return this;
//         }
//         public VignetteRequest End(float returnResponse = float.MinValue)
//         {
//             if (Sender is null)
//                 throw new NullReferenceException($"Trying to end a VignetteRequest with no sender.. Request: {this}");
//             
//             IsSent = false;
//             EffectManager.VignetteEffect.EndRequest(this, returnResponse);
//             return this;
//         }
//         
//         public VignetteRequest SetPriority(int priority)
//         {
//             var newRequest = this;
//             newRequest.Priority = priority;
//             return !IsSent ? newRequest : Update(newRequest);
//         }
//         public VignetteRequest SetType(RequestType requestType)
//         {
//             var newRequest = this;
//             newRequest.RequestType = requestType;
//             return !IsSent ? newRequest : Update(newRequest);
//         }
//         public VignetteRequest SetColor(Color color)
//         {
//             var newRequest = this;
//             newRequest.Color = color;
//             return !IsSent ? newRequest : Update(newRequest);
//         }
//         public VignetteRequest SetCenter(Vector2 center)
//         {
//             var newRequest = this;
//             newRequest.Center = center;
//             return !IsSent ? newRequest : Update(newRequest);
//         }
//         public VignetteRequest SetUseUnscaledTime(bool useUnscaledTime)
//         {
//             var newRequest = this;
//             newRequest.UseUnscaledTime = useUnscaledTime;
//             return !IsSent ? newRequest : Update(newRequest);
//         }
//         public VignetteRequest SetIntensity(float intensity)
//         {
//             // var newRequest = this;
//             // newRequest.Intensity = intensity;
//             // return !IsSent ? newRequest : Update(newRequest);
//             
//             var newRequest = this;
//             newRequest.Intensity = intensity;
//             return Update(newRequest);
//         }
//         public VignetteRequest SetSmoothness(float smoothness)
//         {
//             var newRequest = this;
//             newRequest.Smoothness = smoothness;
//             return !IsSent ? newRequest : Update(newRequest);
//         }
//         public VignetteRequest SetResponse(float response)
//         {
//             var newRequest = this;
//             newRequest.Response = response;
//             return !IsSent ? newRequest : Update(newRequest);
//         }
//         
//         public static void SendMany(VignetteRequest[] requests, object sender) => requests.ForEach(request => request.Send(sender));
//         public static void SendMany(List<VignetteRequest> requests, object sender) => requests.ForEach(request => request.Send(sender));
//         
//         public static void EndAll() => EffectManager.VignetteEffect.EndAllRequests();
//         public static void EndAll(float returnResponse) => EffectManager.VignetteEffect.EndAllRequests(returnResponse);
//         public static void EndMany(IEnumerable<VignetteRequest> requests) => EffectManager.VignetteEffect.EndRequests(requests);
//         public static void EndMany(IEnumerable<VignetteRequest> requests, float returnResponse) => EffectManager.VignetteEffect.EndRequests(requests, returnResponse);
//         public static void EndAllFrom(object sender) => EffectManager.VignetteEffect.EndRequestsFrom(sender);
//         public static void EndAllFrom(object sender, float returnResponse) => EffectManager.VignetteEffect.EndRequestsFrom(sender, returnResponse);
//         
//         public static bool AnyActive() => EffectManager.VignetteEffect.AnyActive;
//         public static VignetteRequest GetCurrent() => EffectManager.VignetteEffect.GetCurrentRequest();
//         public static bool TryGetCurrent(out VignetteRequest request) => EffectManager.VignetteEffect.TryGetCurrentRequest(out request);
//         public static List<VignetteRequest> GetAll() => EffectManager.VignetteEffect.GetRequests();
//         public static bool TryGetAll(out List<VignetteRequest> requests) => EffectManager.VignetteEffect.TryGetRequests(out requests);
//         public static List<VignetteRequest> GetAllFrom(object sender) => EffectManager.VignetteEffect.GetRequestsFrom(sender);
//         public static bool TryGetAllFrom(object sender, out List<VignetteRequest> requestsFrom) => EffectManager.VignetteEffect.TryGetRequestsFrom(sender, out requestsFrom);
//         public static List<VignetteRequest> GetAllWithPriority(int priority) => EffectManager.VignetteEffect.GetRequestsWithPriority(priority);
//         public static bool TryGetAllWithPriority(int priority, out List<VignetteRequest> requestsWithPriority) => EffectManager.VignetteEffect.TryGetRequestsWithPriority(priority, out requestsWithPriority);
//         
//         public bool Equals(VignetteRequest other)
//         {
//             if (ID != 0 || other.ID != 0)
//                 return ID == other.ID;
//
//             return (Sender == null && other.Sender == null || Sender != null && Sender.Equals(other.Sender))
//                    && RequestType == other.RequestType
//                    && UseUnscaledTime.Equals(other.UseUnscaledTime)
//                    && Intensity.Equals(other.Intensity)
//                    && Smoothness.Equals(other.Smoothness)
//                    && Response.Equals(other.Response)
//                    && Priority.Equals(other.Priority)
//                    && Color.Equals(other.Color)
//                    && Center.Equals(other.Center);
//         }
//
//         public override bool Equals(object obj)
//         {
//             return obj is VignetteRequest other && Equals(other);
//         }
//
//         public override int GetHashCode()
//         {
//             // This is a hacky way to get a unique hashcode for the request
//             // TODO: Find a better way to get a unique hashcode
//             return HashCode.Combine(Sender, RequestType, UseUnscaledTime, Intensity, Smoothness, Response, Priority, Color);
//         }
//         
//         /// <summary>
//         /// Compares the values of two VignetteRequests, Use .Equals() to compare to specific request
//         /// </summary>
//         public static bool operator == (VignetteRequest left, VignetteRequest right)
//         {
//             return left.Priority == right.Priority
//                    && left.RequestType == right.RequestType
//                    && left.UseUnscaledTime == right.UseUnscaledTime
//                    && Mathf.Approximately(left.Intensity, right.Intensity)
//                    && Mathf.Approximately(left.Smoothness, right.Smoothness)
//                    && Mathf.Approximately(left.Response, right.Response)
//                    && left.Color == right.Color
//                    && left.Center == right.Center;
//         }
//         
//         /// <summary>
//         /// Compares the values of two VignetteRequests, Use .Equals() to compare to specific request
//         /// </summary>
//         public static bool operator != (VignetteRequest left, VignetteRequest right)
//         {
//             return !(left == right);
//         }
//         
//         /// <summary>
//         /// Compares the intensity of two VignetteRequests
//         /// </summary>
//         public static bool operator > (VignetteRequest left, VignetteRequest right)
//         {
//             return left.GetActualIntensity() > right.GetActualIntensity();
//         }
//         /// <summary>
//         /// Compares the intensity of two VignetteRequests
//         /// </summary>
//         public static bool operator < (VignetteRequest left, VignetteRequest right)
//         {
//             return left.GetActualIntensity() < right.GetActualIntensity();
//         }
//         
//         public static bool operator >= (VignetteRequest left, VignetteRequest right)
//         {
//             return left.GetActualIntensity() >= right.GetActualIntensity();
//         }
//         
//         public static bool operator <= (VignetteRequest left, VignetteRequest right)
//         {
//             return left.GetActualIntensity() <= right.GetActualIntensity();
//         }
//
//         public override string ToString()
//         {
//             return $"VignetteRequest: {ID}, " +
//                    $"Sender {Sender}, " +
//                    $"RequestType: {RequestType}, " +
//                    $"Color: {Color}, " +
//                    $"Center: {Center}, " +
//                    $"Intensity: {Intensity}, " +
//                    $"Smoothness: {Smoothness}, " +
//                    $"Response: {Response}, " +
//                    $"Priority: {Priority}, " +
//                    $"UseUnscaledTime{UseUnscaledTime}";
//         }
//
//         public int CompareTo(VignetteRequest other)
//         {
//             return GetActualIntensity().CompareTo(other.GetActualIntensity());
//         }
//     }
// }