using UnityEngine;

namespace Game.Runtime.Abilities
{
    /// <summary>
    /// Data-only ability config. Effect is resolved by AbilityEffectType (PHASE 14).
    /// </summary>
    [CreateAssetMenu(fileName = "AbilityConfig", menuName = "Game/Ability Config", order = 0)]
    public class AbilityConfig : ScriptableObject
    {
        public string abilityId;
        public float cooldown;
        public float range;
        public float power;
        public AbilityEffectType effectType;
    }

    public enum AbilityEffectType
    {
        DealDamage
    }
}
