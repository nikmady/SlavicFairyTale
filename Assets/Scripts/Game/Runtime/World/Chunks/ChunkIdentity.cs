using UnityEngine;

namespace Game.Runtime.World.Chunks
{
    /// <summary>
    /// Holds chunk id on the chunk root. Set by ChunkStreamer when creating the root.
    /// </summary>
    public class ChunkIdentity : MonoBehaviour
    {
        public string chunkId;
    }
}
