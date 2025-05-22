using UnityEngine;

namespace PlayerSystems.Interaction {
    public static class InteractableProber {
        static readonly Collider[] s_overlapResults = new Collider[5];

        public static bool Probe(Ray ray, float distance, out IInteractable interactable, out Vector3 hitPoint) {
            interactable = null;
            hitPoint = Vector3.positiveInfinity;
            
            if (Physics.Raycast(ray, out var hit, distance, IInteractable.InteractableLayerMask, QueryTriggerInteraction.Collide)) {
                hit.collider.TryGetComponent(out interactable);
                hitPoint = hit.point;
            }

            return interactable != null;
        }

        public static bool ProbeAroundPoint(Vector3 position, float radius, out IInteractable interactable, out Vector3 hitPoint) {
            var count = Physics.OverlapSphereNonAlloc(position, radius, s_overlapResults, IInteractable.InteractableLayerMask, QueryTriggerInteraction.Collide);

            IInteractable closest = null;
            var closestPoint = Vector3.positiveInfinity;
            var closestDist = float.MaxValue;

            for (var i = 0; i < count; i++) {
                var collider = s_overlapResults[i];
                if (!collider || !collider.TryGetComponent(out interactable) || interactable.RequireLook)
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