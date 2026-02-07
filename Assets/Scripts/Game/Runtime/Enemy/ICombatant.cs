using UnityEngine;

namespace Game.Runtime.Enemy
{
    /// <summary>
    /// Stub interface for future combat. EnemyRuntime implements it; no behaviour in PHASE 11B.
    /// </summary>
    public interface ICombatant
    {
        Vector2 Position { get; }
        bool IsAlive { get; }
    }
}
