using System.Collections.Generic;
using UnityEngine;
using Game.Runtime.Services;
using Game.Runtime.Abilities;

namespace Game.Runtime.Combat
{
    /// <summary>
    /// Auto-combat: uses ready ability on nearest enemy, else base damage. All via CombatSystem (PHASE 14).
    /// </summary>
    public class AutoCombatResolver
    {
        private readonly CombatSystem _combatSystem;
        private readonly PlayerCombatProbe _probe;
        private readonly ICombatant _playerCombatant;
        private readonly AbilitySystem _abilitySystem;
        private readonly float _attackInterval;
        private float _timer;

        public AutoCombatResolver(CombatSystem combatSystem, PlayerCombatProbe probe, ICombatant playerCombatant, AbilitySystem abilitySystem, float attackInterval)
        {
            _combatSystem = combatSystem;
            _probe = probe;
            _playerCombatant = playerCombatant;
            _abilitySystem = abilitySystem;
            _attackInterval = attackInterval;
            _timer = 0f;
        }

        public void Tick(float dt)
        {
            var enemies = _probe?.EnemiesInRange;
            if (enemies == null || enemies.Count == 0)
            {
                _timer = 0f;
                return;
            }

            var nearest = GetNearest(enemies, _playerCombatant.WorldPosition);
            if (nearest == null || !nearest.IsAlive) return;

            _timer -= dt;
            if (_timer <= 0f)
            {
                var playerPos = _playerCombatant.WorldPosition;
                var distance = (nearest.WorldPosition - playerPos).magnitude;
                var targets = new List<ICombatant> { nearest };
                var usedAbility = false;

                if (_abilitySystem != null)
                {
                    foreach (var ability in _abilitySystem.Abilities)
                    {
                        if (ability != null && ability.CanActivate() && ability.config != null && ability.config.range >= distance)
                        {
                            _abilitySystem.TryActivateAbility(ability.config.abilityId, targets);
                            usedAbility = true;
                            break;
                        }
                    }
                }

                if (!usedAbility)
                {
                    var damage = _playerCombatant.Stats.attackPower;
                    var evt = new DamageEvent { source = _playerCombatant, target = nearest, damage = damage };
                    _combatSystem.ApplyDamage(evt);
                    Log.Info($"[Combat] Player hits Enemy for {damage}");
                }

                _timer = _attackInterval;
            }
        }

        private static ICombatant GetNearest(IReadOnlyList<ICombatant> list, Vector2 from)
        {
            ICombatant nearest = null;
            float nearestSq = float.MaxValue;
            for (int i = 0; i < list.Count; i++)
            {
                var c = list[i];
                if (c == null || !c.IsAlive) continue;
                var sq = (c.WorldPosition - from).sqrMagnitude;
                if (sq < nearestSq)
                {
                    nearestSq = sq;
                    nearest = c;
                }
            }
            return nearest;
        }
    }
}
