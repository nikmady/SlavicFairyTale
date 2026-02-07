using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Runtime.Services;

namespace Game.Runtime.Enemy
{
    /// <summary>
    /// On chunk root. Registers EnemySpawnPoint with EnemySystem on enable; despawns all on disable.
    /// Supports late binding: if EnemySystem is not ready, subscribes to OnReady then registers.
    /// Chunk fully owns the lifecycle of spawned enemies.
    /// </summary>
    public class ChunkEnemyRegistrar : MonoBehaviour
    {
        private readonly List<EnemyRuntime> _spawnedEnemies = new List<EnemyRuntime>();
        private bool _subscribedToReady;
        private bool _isRegistered;

        private void RegisterAll()
        {
            var system = EnemySystemRegistry.Current;
            if (system == null) return;

            var spawnPoints = GetComponentsInChildren<EnemySpawnPoint>(true);
            foreach (var sp in spawnPoints)
            {
                if (sp == null || string.IsNullOrEmpty(sp.enemyId)) continue;
                var pos = (Vector2)sp.transform.position;
                var enemy = system.SpawnEnemy(sp.enemyId, pos);
                if (enemy != null)
                    _spawnedEnemies.Add(enemy);
            }
            _isRegistered = true;
            Log.Info($"[Game] Spawned {_spawnedEnemies.Count} enemies in chunk {gameObject.name}");
        }

        private void UnregisterAll()
        {
            if (!_isRegistered) return;

            var system = EnemySystemRegistry.Current;
            if (system != null)
            {
                foreach (var e in _spawnedEnemies)
                {
                    if (e != null)
                        system.DespawnEnemy(e);
                }
            }
            _spawnedEnemies.Clear();
            _isRegistered = false;
            Log.Info($"[Game] Despawned enemies in chunk {gameObject.name}");
        }

        private void OnEnable()
        {
            if (_isRegistered) return;

            if (EnemySystemRegistry.Current != null)
                RegisterAll();
            else
            {
                _subscribedToReady = true;
                EnemySystemRegistry.OnReady += OnEnemySystemReady;
            }
        }

        private void OnEnemySystemReady()
        {
            if (!_subscribedToReady) return;
            if (_isRegistered) return;
            _subscribedToReady = false;
            EnemySystemRegistry.OnReady -= OnEnemySystemReady;
            RegisterAll();
        }

        private void OnDisable()
        {
            if (_subscribedToReady)
            {
                _subscribedToReady = false;
                EnemySystemRegistry.OnReady -= OnEnemySystemReady;
            }
            UnregisterAll();
        }
    }
}
