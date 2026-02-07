namespace Game.Runtime.World
{
    /// <summary>
    /// Creates and configures WorldRuntime from location data.
    /// Future: ChunkStreamer, PlayerRuntime, systems will be wired here.
    /// </summary>
    public static class WorldBootstrap
    {
        public static WorldRuntime CreateWorld(string locationId, string biomeId)
        {
            var context = new WorldRuntimeContext
            {
                locationId = locationId ?? string.Empty,
                biomeId = biomeId ?? string.Empty,
                worldOrigin = UnityEngine.Vector2.zero,
                isInitialized = false
            };
            var runtime = new WorldRuntime(context);
            runtime.Initialize();
            return runtime;
        }
    }
}
