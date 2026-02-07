using System.Collections.Generic;
using Game.Runtime.Combat;

namespace Game.Runtime.Abilities
{
    /// <summary>
    /// Base for ability effects. Apply uses CombatSystem (PHASE 14).
    /// </summary>
    public abstract class AbilityEffect
    {
        public abstract void Apply(CombatSystem combatSystem, ICombatant source, IReadOnlyList<ICombatant> targets, AbilityConfig config);
    }
}
