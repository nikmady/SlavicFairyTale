using UnityEngine;

namespace Game.Runtime.Combat
{
    /// <summary>
    /// Interface for combat query and damage. ApplyDamage is handled by CombatSystem (PHASE 13).
    /// </summary>
    public interface ICombatant
    {
        Vector2 WorldPosition { get; }
        bool IsAlive { get; }
        CombatStats Stats { get; }
        /// <summary>Apply damage; implementation sets currentHealth and IsAlive. Called by CombatSystem.ApplyDamage.</summary>
        void TakeDamage(float damage);
    }
}
