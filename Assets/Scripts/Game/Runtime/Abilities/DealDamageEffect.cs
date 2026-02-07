using System.Collections.Generic;
using Game.Runtime.Combat;

namespace Game.Runtime.Abilities
{
    /// <summary>
    /// Creates DamageEvent per target and applies via CombatSystem (PHASE 14).
    /// </summary>
    public class DealDamageEffect : AbilityEffect
    {
        public override void Apply(CombatSystem combatSystem, ICombatant source, IReadOnlyList<ICombatant> targets, AbilityConfig config)
        {
            if (combatSystem == null || source == null || targets == null) return;

            foreach (var target in targets)
            {
                if (target == null || !target.IsAlive) continue;
                var evt = new DamageEvent { source = source, target = target, damage = config.power };
                combatSystem.ApplyDamage(evt);
            }
        }
    }
}
