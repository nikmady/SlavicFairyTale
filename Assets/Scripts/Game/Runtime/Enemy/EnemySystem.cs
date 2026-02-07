using System.Collections.Generic;
using UnityEngine;

namespace Game.Runtime.Enemy
{
    /// <summary>
    /// Holds and updates all EnemyRuntime. Spawns/despawns by id and position. Does not know about Player.
    /// </summary>
    public class EnemySystem
    {
        private readonly List<EnemyRuntime> _enemies = new List<EnemyRuntime>();
        private readonly List<EnemyRuntime> _toRemove = new List<EnemyRuntime>();

        public IReadOnlyList<EnemyRuntime> Enemies => _enemies;

        public EnemyRuntime SpawnEnemy(string enemyId, Vector2 position, string chunkId = null)
        {
            if (string.IsNullOrEmpty(enemyId)) return null;

            var enemy = new EnemyRuntime(enemyId, position, chunkId);
            enemy.Initialize();
            _enemies.Add(enemy);
            return enemy;
        }

        public void DespawnEnemy(EnemyRuntime enemy)
        {
            if (enemy == null) return;
            enemy.Dispose();
            _enemies.Remove(enemy);
        }

        public void DespawnEnemiesInChunk(string chunkId)
        {
            if (string.IsNullOrEmpty(chunkId)) return;

            _toRemove.Clear();
            foreach (var e in _enemies)
            {
                if (e != null && e.spawnChunkId == chunkId)
                    _toRemove.Add(e);
            }
            foreach (var e in _toRemove)
                DespawnEnemy(e);
        }

        public void Tick(float dt)
        {
            foreach (var e in _enemies)
            {
                if (e != null && e.isAlive)
                    e.Tick(dt);
            }
        }
    }
}
