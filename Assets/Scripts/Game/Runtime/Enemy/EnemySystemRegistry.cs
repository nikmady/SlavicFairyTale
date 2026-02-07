using System;

namespace Game.Runtime.Enemy
{
    /// <summary>
    /// Static registry for current EnemySystem. Set/cleared by WorldRuntime.
    /// Allows chunk registrars to resolve the system and subscribe to OnReady (late binding).
    /// </summary>
    public static class EnemySystemRegistry
    {
        public static EnemySystem Current { get; private set; }

        public static event Action OnReady;

        public static void Set(EnemySystem system)
        {
            Current = system;
            OnReady?.Invoke();
        }

        public static void Clear()
        {
            Current = null;
        }
    }
}
