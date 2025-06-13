using UnityEngine;

namespace PlayerSystems.Interaction {
    public static class InteractionProber {
        static readonly Collider[] s_overlapResults = new Collider[5];
        static readonly RaycastHit[] s_results = new RaycastHit[10];
        static readonly object gate = new ();

        public static bool ProbeRay(Ray ray, out IInteractable interactable, out Vector3 hitPoint) {
            lock (gate) {
                interactable = null;
                hitPoint = Vector3.positiveInfinity;

                var count = Physics.RaycastNonAlloc(ray, s_results, Mathf.Infinity, IInteractable.InteractableLayerMask,
                    QueryTriggerInteraction.Collide);

                for (var i = 0; i < count; ++i) {
                    var result = s_results[i];
                    if (!result.collider.TryGetComponent(out interactable))
                        continue;
                    if (!interactable.CanInteract())
                        continue;
                    if (result.distance > interactable.MaxInteractionDistance)
                        continue;

                    hitPoint = result.point;
                    return interactable != null;
                }

                return false;
            }
        }

        public static bool ProbeAroundPoint(Vector3 position, float radius, out IInteractable interactable, out Vector3 hitPoint) {
            lock (gate) {
                var count = Physics.OverlapSphereNonAlloc(position, radius, s_overlapResults, IInteractable.InteractableLayerMask, QueryTriggerInteraction.Collide);

                IInteractable closest = null;
                var closestPoint = Vector3.positiveInfinity;
                var closestDist = float.MaxValue;

                for (var i = 0; i < count; i++) {
                    var collider = s_overlapResults[i];
                    if (!collider || !collider.TryGetComponent(out interactable) || interactable.RequireLook || !interactable.CanInteract())
                        continue;
                    
                    var colliderPoint = collider.ClosestPoint(position);
                    var dist = Vector3.Distance(position, colliderPoint);
                    if (dist > closestDist)
                        continue;
                    
                    closestDist = dist;
                    closestPoint = colliderPoint;
                    closest = interactable;
                }

                hitPoint = closestPoint;
                interactable = closest;
                return interactable != null;
            }
        }
    }
}