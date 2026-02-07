using System.Collections.Generic;
using UnityEngine;

namespace Game.Runtime.World.Chunks
{
    /// <summary>
    /// Data-only ScriptableObject: registry of chunks for one location.
    /// Does NOT load chunks, does NOT know about WorldRuntime or Addressables.
    /// Future: created in Editor, assigned per location; loaded/resolved by locationId in LoadLocationState.
    /// </summary>
    [CreateAssetMenu(fileName = "LocationChunkSet", menuName = "Game/Location Chunk Set", order = 0)]
    public class LocationChunkSet : ScriptableObject, IChunkSource
    {
        public string locationId;
        public List<ChunkDescriptor> chunks = new List<ChunkDescriptor>();

        public IEnumerable<ChunkDescriptor> GetChunks() => GetAllChunks();

        public bool TryGetChunk(string chunkId, out ChunkDescriptor descriptor)
        {
            descriptor = null;
            if (string.IsNullOrEmpty(chunkId) || chunks == null) return false;
            foreach (var c in chunks)
            {
                if (c != null && c.chunkId == chunkId)
                {
                    descriptor = c;
                    return true;
                }
            }
            return false;
        }

        public IEnumerable<ChunkDescriptor> GetAllChunks()
        {
            if (chunks == null) yield break;
            foreach (var c in chunks)
            {
                if (c != null)
                    yield return c;
            }
        }

        public IEnumerable<ChunkDescriptor> GetChunksIntersecting(Bounds area)
        {
            if (chunks == null) yield break;
            foreach (var c in chunks)
            {
                if (c != null && c.bounds.Intersects(area))
                    yield return c;
            }
        }
    }
}
