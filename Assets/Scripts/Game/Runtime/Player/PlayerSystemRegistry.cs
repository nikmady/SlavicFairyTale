namespace Game.Runtime.Player
{
    /// <summary>
    /// Static registry for current PlayerSystem. Set/cleared by WorldRuntime (PHASE 15).
    /// </summary>
    public static class PlayerSystemRegistry
    {
        public static PlayerSystem Current { get; private set; }

        public static void Set(PlayerSystem system) => Current = system;
        public static void Clear() => Current = null;
    }
}
