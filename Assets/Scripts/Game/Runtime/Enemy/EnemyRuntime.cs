using UnityEngine;
using Game.Runtime.Combat;

namespace Game.Runtime.Enemy
{
    /// <summary>
    /// Runtime enemy entity. Config, health, position, movement params. View bound from outside. Implements ICombatant (PHASE 17).
    /// </summary>
    public class EnemyRuntime : ICombatant
    {
        public EnemyConfig Config { get; }
        public string enemyId => Config != null ? Config.enemyId : string.Empty;
        public Vector2 Position { get; set; }
        public float MoveSpeed { get; private set; }
        public float AggroRadius { get; private set; }
        public float StopDistance { get; private set; }
        public Vector2 TargetOffset { get; private set; }
        public bool isAlive { get; set; }
        public string spawnChunkId { get; }

        /// <summary>Current health. Initialized from Config.maxHealth.</summary>
        public float currentHealth { get; set; }

        public float maxHealth => Config != null ? Config.maxHealth : 0f;

        Vector2 ICombatant.WorldPosition => Position;
        bool ICombatant.IsAlive => isAlive;
        CombatStats ICombatant.Stats => new CombatStats { maxHealth = maxHealth, currentHealth = currentHealth, attackPower = 0f };

        void ICombatant.TakeDamage(float damage)
        {
            currentHealth -= damage;
            if (currentHealth <= 0f)
                isAlive = false;
        }

        private bool _disposed;

        public EnemyRuntime(EnemyConfig config, Vector2 position, string spawnChunkId = null)
        {
            Config = config;
            Position = position;
            this.spawnChunkId = spawnChunkId ?? string.Empty;
            currentHealth = config != null ? config.maxHealth : 0f;
            isAlive = true;
            MoveSpeed = config != null ? config.moveSpeed : 3f;
            AggroRadius = config != null ? config.aggroRadius : 10f;
            StopDistance = config != null ? config.stopDistance : 1.5f;
            TargetOffset = Random.insideUnitCircle * 1.2f;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            isAlive = false;
        }
    }
}
