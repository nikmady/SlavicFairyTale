namespace Game.Runtime.Combat
{
    /// <summary>
    /// Data-only damage event. No logic (PHASE 13).
    /// </summary>
    public class DamageEvent
    {
        public ICombatant source;
        public ICombatant target;
        public float damage;
    }
}
