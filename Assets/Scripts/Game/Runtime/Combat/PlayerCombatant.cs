using System;
using UnityEngine;

namespace Game.Runtime.Combat
{
    /// <summary>
    /// Player as ICombatant for DamageEvent source. Position from delegate; attackPower for auto-combat (PHASE 13).
    /// </summary>
    public class PlayerCombatant : ICombatant
    {
        private readonly Func<Vector2> _getPosition;
        private readonly float _attackPower;

        public PlayerCombatant(Func<Vector2> getPosition, float attackPower)
        {
            _getPosition = getPosition;
            _attackPower = attackPower;
        }

        public Vector2 WorldPosition => _getPosition != null ? _getPosition() : Vector2.zero;
        public bool IsAlive => true;
        public CombatStats Stats => new CombatStats { maxHealth = 100f, currentHealth = 100f, attackPower = _attackPower };

        public void TakeDamage(float damage) { /* Player damage not used in PHASE 13 */ }
    }
}
