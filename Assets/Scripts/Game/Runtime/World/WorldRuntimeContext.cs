using UnityEngine;

namespace Game.Runtime.World
{
    /// <summary>
    /// Runtime context for the active world location. Pure data, no logic.
    /// worldOrigin reserved for future chunk/streaming coordinates.
    /// </summary>
    public class WorldRuntimeContext
    {
        public string locationId;
        public string biomeId;
        public Vector2 worldOrigin;
        public bool isInitialized;
    }
}
