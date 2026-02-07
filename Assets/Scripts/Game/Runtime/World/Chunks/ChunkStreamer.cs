using System.Collections.Generic;
using UnityEngine;
using Game.Runtime.Services;
using Game.Runtime.Interaction;
using Game.Runtime.Enemy;

namespace Game.Runtime.World.Chunks
{
    /// <summary>
    /// Runtime chunk streaming: loads/unloads chunks by player position. Uses IChunkSource only; no Addressables.
    /// </summary>
    public class ChunkStreamer
    {
        private IChunkSource _source;
        private readonly Dictionary<string, ChunkRuntime> _activeChunks = new Dictionary<string, ChunkRuntime>();
        private readonly float _streamingRadius;

        public float streamingRadius => _streamingRadius;

        public ChunkStreamer(IChunkSource source, float streamingRadius)
        {
            _source = source;
            _streamingRadius = Mathf.Max(0f, streamingRadius);
        }

        public void SetSource(IChunkSource source)
        {
            _source = source;
        }

        public void UpdateStreaming(Vector2 playerPosition)
        {
            var center = new Vector3(playerPosition.x, playerPosition.y, 0f);
            var size = new Vector3(_streamingRadius * 2f, _streamingRadius * 2f, 1f);
            var streamingBounds = new Bounds(center, size);

            var desired = new HashSet<string>();
            if (_source != null)
            {
                foreach (var desc in _source.GetChunksIntersecting(streamingBounds))
                {
                    if (desc != null && !string.IsNullOrEmpty(desc.chunkId))
                        desired.Add(desc.chunkId);
                }
            }

            foreach (var id in desired)
            {
                if (!_activeChunks.ContainsKey(id) && _source != null && _source.TryGetChunk(id, out var desc))
                    LoadChunk(desc);
            }

            var toUnload = new List<string>();
            foreach (var kv in _activeChunks)
            {
                if (kv.Value.descriptor != null && kv.Value.descriptor.isAlwaysLoaded)
                    continue;
                if (!desired.Contains(kv.Key))
                    toUnload.Add(kv.Key);
            }
            foreach (var id in toUnload)
                UnloadChunk(id);
        }

        public void LoadChunk(ChunkDescriptor descriptor)
        {
            if (descriptor == null || string.IsNullOrEmpty(descriptor.chunkId)) return;
            if (_activeChunks.ContainsKey(descriptor.chunkId)) return;

            var root = new GameObject("Chunk_" + descriptor.chunkId);
            var identity = root.AddComponent<ChunkIdentity>();
            identity.chunkId = descriptor.chunkId;
            root.AddComponent<ChunkInteractionRegistrar>();
            root.AddComponent<ChunkEnemyRegistrar>();
            var runtime = new ChunkRuntime(descriptor, root);
            _activeChunks[descriptor.chunkId] = runtime;
            Log.Info($"Load chunk {descriptor.chunkId}");
        }

        public void UnloadChunk(string chunkId)
        {
            if (string.IsNullOrEmpty(chunkId)) return;
            if (!_activeChunks.TryGetValue(chunkId, out var runtime)) return;

            _activeChunks.Remove(chunkId);
            if (runtime.root != null)
                Object.Destroy(runtime.root);
            Log.Info($"Unload chunk {chunkId}");
        }
    }
}
