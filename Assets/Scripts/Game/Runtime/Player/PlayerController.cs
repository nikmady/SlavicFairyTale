using System.Collections.Generic;
using Game.Runtime.Combat;
using Game.Runtime.Abilities;

namespace Game.Runtime.Player
{
    public class PlayerController
    {
        private readonly AbilitySystem _abilitySystem;
        private readonly PlayerCombatProbe _probe;

        public PlayerController(AbilitySystem abilitySystem, PlayerCombatProbe probe)
        {
            _abilitySystem = abilitySystem;
            _probe = probe;
        }

        public void OnAbilityPressed(string abilityId)
        {
            var targets = _probe?.EnemiesInRange;
            if (targets == null || targets.Count == 0) return;
            _abilitySystem?.TryActivateAbility(abilityId, targets);
        }
    }
}
