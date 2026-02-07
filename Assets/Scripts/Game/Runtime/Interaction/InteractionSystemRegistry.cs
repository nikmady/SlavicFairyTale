using System;

namespace Game.Runtime.Interaction
{
    /// <summary>
    /// Static registry for current InteractionSystem and lifecycle events. Set/cleared by WorldRuntime.
    /// Allows ChunkInteractionRegistrar to resolve the system and subscribe to ready/disposed without FindObjectOfType.
    /// </summary>
    public static class InteractionSystemRegistry
    {
        public static InteractionSystem Current { get; private set; }

        public static event Action OnReady;
        public static event Action OnDisposed;

        public static void Set(InteractionSystem system)
        {
            Current = system;
            OnReady?.Invoke();
        }

        public static void Clear()
        {
            OnDisposed?.Invoke();
            Current = null;
        }
    }
}
