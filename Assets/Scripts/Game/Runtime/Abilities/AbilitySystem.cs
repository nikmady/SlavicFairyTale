using System.Collections.Generic;
using Game.Runtime.Combat;

namespace Game.Runtime.Abilities
{
    /// <summary>
    /// Holds AbilityRuntime instances. TryActivateAbility and Tick (PHASE 14).
    /// </summary>
    public class AbilitySystem
    {
        private readonly List<AbilityRuntime> _abilities = new List<AbilityRuntime>();

        public IReadOnlyList<AbilityRuntime> Abilities => _abilities;

        public AbilitySystem(CombatSystem combatSystem, ICombatant caster)
        {
            CombatSystem = combatSystem;
            Caster = caster;
        }

        public CombatSystem CombatSystem { get; }
        public ICombatant Caster { get; }

        public void AddAbility(AbilityConfig config)
        {
            if (config == null) return;
            _abilities.Add(new AbilityRuntime(config, CombatSystem, Caster));
        }

        public AbilityRuntime GetById(string abilityId)
        {
            if (string.IsNullOrEmpty(abilityId)) return null;
            foreach (var a in _abilities)
            {
                if (a?.config != null && a.config.abilityId == abilityId)
                    return a;
            }
            return null;
        }

        public bool TryActivateAbility(string abilityId, IReadOnlyList<ICombatant> targets)
        {
            var ability = GetById(abilityId);
            if (ability == null || !ability.CanActivate()) return false;
            if (targets == null || targets.Count == 0) return false;

            ability.Activate(targets);
            return true;
        }

        public void Tick(float dt)
        {
            foreach (var a in _abilities)
                a?.Tick(dt);
        }
    }
}
