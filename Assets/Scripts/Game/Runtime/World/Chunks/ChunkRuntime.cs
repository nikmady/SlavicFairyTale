using UnityEngine;

namespace Game.Runtime.World.Chunks
{
    /// <summary>
    /// Runtime representation of a loaded chunk. Holds descriptor and root GameObject.
    /// No behavior; lifecycle managed by ChunkStreamer.
    /// </summary>
    public class ChunkRuntime
    {
        public ChunkDescriptor descriptor;
        public GameObject root;

        public ChunkRuntime(ChunkDescriptor descriptor, GameObject root)
        {
            this.descriptor = descriptor;
            this.root = root;
        }
    }
}
