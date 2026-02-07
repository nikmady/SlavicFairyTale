using System.Collections.Generic;
using UnityEngine;

namespace Game.Runtime.Interaction
{
    /// <summary>
    /// Holds registered interactables and processes interaction requests. Pure logic; does not know about Meta/UI.
    /// </summary>
    public class InteractionSystem
    {
        private readonly List<IInteractable> _interactables = new List<IInteractable>();

        public void Register(IInteractable interactable)
        {
            if (interactable != null && !_interactables.Contains(interactable))
                _interactables.Add(interactable);
        }

        public void Unregister(IInteractable interactable)
        {
            if (interactable != null)
                _interactables.Remove(interactable);
        }

        /// <summary>Finds nearest valid interactable in request radius, checks CanInteract, calls Interact. Returns true if interaction occurred.</summary>
        public bool TryInteract(InteractionRequest request)
        {
            float maxSq = request.radius * request.radius;
            IInteractable nearest = null;
            float nearestSq = float.MaxValue;

            foreach (var i in _interactables)
            {
                if (i == null) continue;
                float sqDist = (i.WorldPosition - request.origin).sqrMagnitude;
                if (sqDist > maxSq) continue;
                if (sqDist > i.InteractionRadius * i.InteractionRadius) continue;
                if (!i.CanInteract()) continue;
                if (sqDist < nearestSq)
                {
                    nearestSq = sqDist;
                    nearest = i;
                }
            }

            if (nearest == null) return false;
            nearest.Interact();
            return true;
        }
    }
}
