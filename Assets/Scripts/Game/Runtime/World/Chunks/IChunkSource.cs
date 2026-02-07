using System.Collections.Generic;
using UnityEngine;

namespace Game.Runtime.World.Chunks
{
    /// <summary>
    /// Abstraction over chunk data. WorldRuntime will use IChunkSource; implementation can be
    /// LocationChunkSet now, or later: streaming source, editor preview, etc.
    /// </summary>
    public interface IChunkSource
    {
        IEnumerable<ChunkDescriptor> GetChunks();
        bool TryGetChunk(string chunkId, out ChunkDescriptor descriptor);
        IEnumerable<ChunkDescriptor> GetChunksIntersecting(Bounds area);
    }
}
