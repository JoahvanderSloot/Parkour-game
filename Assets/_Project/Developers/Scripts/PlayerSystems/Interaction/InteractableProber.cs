using UnityEngine;

namespace PlayerSystems.Interaction {
    public static class InteractableProber {
        static readonly Collider[] s_overlapResults = new Collider[5];

        public static bool ProbeCrosshair(Ray ray, float distance, LayerMask layerMask, out IInteractable interactable) {
            interactable = null;
            if (Physics.Raycast(ray, out var hit, distance, layerMask, QueryTriggerInteraction.Collide))
                hit.collider.TryGetComponent(out interactable);

            return false;
        }

        public static bool ProbeAroundPoint(Vector3 position, float radius, LayerMask layerMask, out IInteractable interactable) {
            var count = Physics.OverlapSphereNonAlloc(position, radius, s_overlapResults, layerMask, QueryTriggerInteraction.Collide);

            IInteractable closest = null;
            var closestDist = float.MaxValue;

            for (var i = 0; i < count; i++) {
                var collider = s_overlapResults[i];
                if (!collider || !collider.TryGetComponent(out interactable) || interactable.MustBeLookedAt)
                    continue;
                
                var dist = Vector3.Distance(position, collider.ClosestPoint(position));
                if (dist > closestDist)
                    continue;
                
                closestDist = dist;
                closest = interactable;
            }

            interactable = closest;
            return interactable != null;
        }
    }
}