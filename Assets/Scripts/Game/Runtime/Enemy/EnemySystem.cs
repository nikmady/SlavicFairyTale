using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Runtime.Combat;
using Game.View.Enemy;

namespace Game.Runtime.Enemy
{
    /// <summary>
    /// Spawns enemies with EnemyRuntime, EnemyAI, EnemyView prefab, EnemyViewBinder. Ticks AI and binders. Registers with CombatSystem (PHASE 17).
    /// </summary>
    public class EnemySystem
    {
        private readonly EnemyConfigDatabase _database;
        private readonly CombatSystem _combatSystem;
        private readonly Func<Vector2> _getPlayerPosition;
        private readonly List<EnemyInstance> _instances = new List<EnemyInstance>();
        private readonly List<EnemyRuntime> _toRemove = new List<EnemyRuntime>();

        public IReadOnlyList<EnemyRuntime> Enemies
        {
            get
            {
                var list = new List<EnemyRuntime>(_instances.Count);
                foreach (var i in _instances)
                    if (i.Runtime != null) list.Add(i.Runtime);
                return list;
            }
        }

        public EnemySystem(EnemyConfigDatabase database, CombatSystem combatSystem, Func<Vector2> getPlayerPosition)
        {
            _database = database;
            _combatSystem = combatSystem;
            _getPlayerPosition = getPlayerPosition;
        }

        public EnemyRuntime SpawnEnemy(string enemyId, Vector2 position, string chunkId = null)
        {
            if (string.IsNullOrEmpty(enemyId)) return null;

            var config = _database != null ? _database.GetById(enemyId) : null;
            if (config == null)
            {
                Debug.LogError($"[Game] EnemyConfig not found for enemyId: {enemyId}");
                return null;
            }

            var runtime = new EnemyRuntime(config, position, chunkId);

            if (config.enemyViewPrefab == null)
            {
                Debug.LogError($"[Game] EnemyConfig '{enemyId}' has no enemyViewPrefab assigned.");
                return null;
            }

            var viewRoot = UnityEngine.Object.Instantiate(config.enemyViewPrefab);
            viewRoot.name = $"Enemy_{enemyId}";

            var view = viewRoot.GetComponent<Game.View.Enemy.EnemyView>();
            if (view == null)
                view = viewRoot.AddComponent<Game.View.Enemy.EnemyView>();

            view.SetPosition(runtime.Position);

            var ai = new EnemyAI(runtime, _getPlayerPosition);
            var binder = new EnemyViewBinder(runtime, view);

            _instances.Add(new EnemyInstance { Runtime = runtime, AI = ai, ViewRoot = viewRoot, Binder = binder });
            _combatSystem?.Register(runtime);
            return runtime;
        }

        public void DespawnEnemy(EnemyRuntime enemy)
        {
            if (enemy == null) return;
            _toRemove.Add(enemy);
        }

        private void RemoveEnemy(EnemyRuntime enemy)
        {
            for (int i = _instances.Count - 1; i >= 0; i--)
            {
                if (_instances[i].Runtime != enemy) continue;
                _combatSystem?.Unregister(enemy);
                enemy.Dispose();
                if (_instances[i].ViewRoot != null)
                    UnityEngine.Object.Destroy(_instances[i].ViewRoot);
                _instances.RemoveAt(i);
                return;
            }
        }

        public void DespawnEnemiesInChunk(string chunkId)
        {
            if (string.IsNullOrEmpty(chunkId)) return;
            _toRemove.Clear();
            foreach (var inst in _instances)
            {
                if (inst.Runtime != null && inst.Runtime.spawnChunkId == chunkId)
                    _toRemove.Add(inst.Runtime);
            }
            foreach (var e in _toRemove)
                RemoveEnemy(e);
        }

        public void Tick(float dt)
        {
            foreach (var e in _toRemove)
                RemoveEnemy(e);
            _toRemove.Clear();

            foreach (var inst in _instances)
            {
                if (inst.Runtime == null || !inst.Runtime.isAlive) continue;
                inst.AI?.Tick(dt);
                inst.Binder?.Tick();
            }
        }

        private class EnemyInstance
        {
            public EnemyRuntime Runtime;
            public EnemyAI AI;
            public GameObject ViewRoot;
            public EnemyViewBinder Binder;
        }
    }
}
