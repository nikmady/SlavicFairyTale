using System;
using UnityEngine;
using Game.Runtime.Combat;

namespace Game.Runtime.Enemy
{
    public class EnemyAttackRuntime
    {
        private readonly EnemyRuntime _runtime;
        private readonly CombatSystem _combatSystem;
        private readonly ICombatant _playerCombatant;
        private readonly Func<Vector2> _getPlayerPosition;
        private readonly float _attackDamage;
        private readonly float _attackRange;
        private readonly float _attackCooldown;
        private float _cooldownTimer;

        public EnemyAttackRuntime(
            EnemyRuntime runtime,
            CombatSystem combatSystem,
            ICombatant playerCombatant,
            Func<Vector2> getPlayerPosition,
            float attackDamage,
            float attackRange,
            float attackCooldown)
        {
            _runtime = runtime;
            _combatSystem = combatSystem;
            _playerCombatant = playerCombatant;
            _getPlayerPosition = getPlayerPosition;
            _attackDamage = attackDamage;
            _attackRange = attackRange;
            _attackCooldown = attackCooldown;
            _cooldownTimer = 0f;
        }

        public void Tick(float dt)
        {
            if (_runtime == null || !_runtime.isAlive) return;
            if (_playerCombatant == null || !_playerCombatant.IsAlive) return;
            if (_combatSystem == null) return;

            _cooldownTimer -= dt;
            if (_cooldownTimer > 0f) return;

            var playerPos = _getPlayerPosition != null ? _getPlayerPosition() : Vector2.zero;
            var dist = (playerPos - _runtime.Position).magnitude;
            if (dist > _attackRange) return;

            var evt = new DamageEvent
            {
                source = _runtime,
                target = _playerCombatant,
                damage = _attackDamage
            };
            _combatSystem.ApplyDamage(evt);
            _cooldownTimer = _attackCooldown;
        }
    }
}
