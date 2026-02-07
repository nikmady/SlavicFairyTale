using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Runtime.Player;

namespace Game.Runtime.World
{
    /// <summary>
    /// Стриминг чанков по позиции игрока. Не знает про Unity Editor.
    /// Отслеживает PlayerRuntime.Position, определяет нужные чанки, загружает/выгружает через ChunkLoader.
    /// </summary>
    public class ChunkStreamingSystem
    {
        private readonly Func<Vector2> _getPlayerPosition;
        private readonly float _streamingRadius;
        private readonly ChunkLoader _loader;
        private readonly List<ChunkConfig> _allConfigs;
        private readonly Dictionary<string, ChunkInstance> _activeChunks = new Dictionary<string, ChunkInstance>();

        public IReadOnlyDictionary<string, ChunkInstance> ActiveChunks => _activeChunks;

        public ChunkStreamingSystem(
            PlayerRuntime playerRuntime,
            float streamingRadius,
            IReadOnlyList<ChunkConfig> configs,
            ChunkLoader loader)
        {
            if (playerRuntime == null)
                throw new ArgumentNullException(nameof(playerRuntime));
            _getPlayerPosition = () => playerRuntime.Position;
            _streamingRadius = Mathf.Max(0f, streamingRadius);
            _loader = loader ?? new ChunkLoader();
            _allConfigs = configs != null ? new List<ChunkConfig>(configs) : new List<ChunkConfig>();
        }

        /// <summary>
        /// Вызывать каждый кадр/тик. Обновляет набор чанков по позиции игрока.
        /// </summary>
        public void Tick(float dt)
        {
            Vector2 playerPos = _getPlayerPosition();
            var center = new Vector3(playerPos.x, playerPos.y, 0f);
            var size = new Vector3(_streamingRadius * 2f, _streamingRadius * 2f, 1f);
            var streamingBounds = new Bounds(center, size);

            var desiredIds = new HashSet<string>();
            foreach (var config in _allConfigs)
            {
                if (config == null || string.IsNullOrEmpty(config.chunkId)) continue;
                if (!ChunkBoundsIntersects(config, streamingBounds)) continue;
                desiredIds.Add(config.chunkId);
            }

            foreach (var config in _allConfigs)
            {
                if (config == null || string.IsNullOrEmpty(config.chunkId)) continue;
                if (!desiredIds.Contains(config.chunkId)) continue;
                if (_activeChunks.ContainsKey(config.chunkId)) continue;

                var go = _loader.LoadChunk(config);
                if (go != null)
                {
                    var instance = new ChunkInstance(config, go);
                    instance.IsLoaded = true;
                    _activeChunks[config.chunkId] = instance;
                }
            }

            var toUnload = new List<string>();
            foreach (var kv in _activeChunks)
            {
                if (!desiredIds.Contains(kv.Key))
                    toUnload.Add(kv.Key);
            }
            foreach (var id in toUnload)
            {
                if (_activeChunks.TryGetValue(id, out var instance))
                {
                    _loader.UnloadChunk(instance);
                    _activeChunks.Remove(id);
                }
            }
        }

        private static bool ChunkBoundsIntersects(ChunkConfig config, Bounds area)
        {
            var chunkBounds = new Bounds(
                new Vector3(config.worldPosition.x, config.worldPosition.y, 0f),
                new Vector3(config.size.x, config.size.y, 1f));
            return chunkBounds.Intersects(area);
        }
    }
}
