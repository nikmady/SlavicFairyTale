using System.Collections.Generic;
using Game.Runtime.Combat;
using Game.Runtime.Services;

namespace Game.Runtime.Abilities
{
    /// <summary>
    /// Runtime instance of an ability: cooldown and activation. Not a MonoBehaviour (PHASE 14).
    /// </summary>
    public class AbilityRuntime
    {
        public AbilityConfig config { get; }
        public float cooldownTimer { get; private set; }

        private readonly CombatSystem _combatSystem;
        private readonly ICombatant _caster;

        public AbilityRuntime(AbilityConfig config, CombatSystem combatSystem, ICombatant caster)
        {
            this.config = config;
            _combatSystem = combatSystem;
            _caster = caster;
            cooldownTimer = 0f;
        }

        public bool CanActivate() => config != null && cooldownTimer <= 0f;

        public void Activate(IReadOnlyList<ICombatant> targets)
        {
            if (config == null || _combatSystem == null || _caster == null) return;
            if (targets == null || targets.Count == 0) return;

            var effect = CreateEffect(config.effectType);
            if (effect == null) return;

            Log.Info($"[Ability] Activated {config.abilityId}");
            effect.Apply(_combatSystem, _caster, targets, config);
            cooldownTimer = config.cooldown;
            Log.Info("[Ability] Cooldown started");
        }

        public void Tick(float dt)
        {
            if (cooldownTimer > 0f)
                cooldownTimer -= dt;
        }

        private static AbilityEffect CreateEffect(AbilityEffectType effectType)
        {
            switch (effectType)
            {
                case AbilityEffectType.DealDamage:
                    return new DealDamageEffect();
                default:
                    return null;
            }
        }
    }
}
