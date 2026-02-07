using UnityEngine;

namespace Game.Runtime.Interaction
{
    /// <summary>
    /// Universal interface for world objects the player can interact with.
    /// </summary>
    public interface IInteractable
    {
        Vector2 WorldPosition { get; }
        float InteractionRadius { get; }
        bool CanInteract();
        void Interact();
    }
}
