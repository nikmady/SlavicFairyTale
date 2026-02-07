using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Runtime.Combat
{
    /// <summary>
    /// Holds registered combatants, range queries, and ApplyDamage. Enemies register via EnemySystem (PHASE 13).
    /// </summary>
    public class CombatSystem
    {
        private readonly List<ICombatant> _combatants = new List<ICombatant>();

        public event Action<ICombatant> OnCombatantDeath;

        public void Register(ICombatant combatant)
        {
            if (combatant != null && !_combatants.Contains(combatant))
                _combatants.Add(combatant);
        }

        public void Unregister(ICombatant combatant)
        {
            if (combatant != null)
                _combatants.Remove(combatant);
        }

        /// <summary>Returns combatants within radius of center. Linear pass; no damage.</summary>
        public IReadOnlyList<ICombatant> QueryInRange(Vector2 center, float radius)
        {
            var result = new List<ICombatant>();
            if (radius <= 0f) return result;

            float radiusSq = radius * radius;
            foreach (var c in _combatants)
            {
                if (c == null || !c.IsAlive) continue;
                var delta = c.WorldPosition - center;
                if (delta.sqrMagnitude <= radiusSq)
                    result.Add(c);
            }
            return result;
        }

        /// <summary>Apply damage to target; if target dies, raises OnCombatantDeath.</summary>
        public void ApplyDamage(DamageEvent evt)
        {
            if (evt?.target == null) return;

            evt.target.TakeDamage(evt.damage);
            if (!evt.target.IsAlive)
                OnCombatantDeath?.Invoke(evt.target);
        }
    }
}
