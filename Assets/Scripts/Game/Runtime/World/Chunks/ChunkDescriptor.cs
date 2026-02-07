using System;
using UnityEngine;

namespace Game.Runtime.World.Chunks
{
    /// <summary>
    /// Pure data: describes one chunk (id, future addressable key, world AABB, biome, always-loaded flag).
    /// No logic, no loading. Used by LocationChunkSet and IChunkSource.
    /// </summary>
    [Serializable]
    public class ChunkDescriptor
    {
        public string chunkId;
        /// <summary>Future Addressables key for loading this chunk.</summary>
        public string addressableKey;
        /// <summary>World-space AABB of the chunk.</summary>
        public Bounds bounds;
        /// <summary>Optional; for filtering/debug.</summary>
        public string biomeId;
        /// <summary>If true, chunk is not unloaded (e.g. hub).</summary>
        public bool isAlwaysLoaded;
    }
}
