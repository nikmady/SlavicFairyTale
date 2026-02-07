using System.Collections.Generic;
using UnityEngine;
using Game.Runtime.Services;

namespace Game.Runtime.Combat
{
    /// <summary>
    /// Owned by WorldRuntime; probes CombatSystem for enemies in range of player. No interaction with enemies (PHASE 12).
    /// </summary>
    public class PlayerCombatProbe
    {
        private readonly CombatSystem _combatSystem;
        private readonly System.Func<Vector2> _getPlayerPosition;
        private readonly float _detectionRadius;
        private readonly ICombatant _playerCombatant;
        private readonly List<ICombatant> _filtered = new List<ICombatant>();

        public IReadOnlyList<ICombatant> EnemiesInRange { get; private set; } = new List<ICombatant>().AsReadOnly();

        public PlayerCombatProbe(CombatSystem combatSystem, System.Func<Vector2> getPlayerPosition, float detectionRadius, ICombatant playerCombatant)
        {
            _combatSystem = combatSystem;
            _getPlayerPosition = getPlayerPosition;
            _detectionRadius = detectionRadius;
            _playerCombatant = playerCombatant;
        }

        public void Tick(float dt)
        {
            if (_combatSystem == null || _getPlayerPosition == null) return;

            var center = _getPlayerPosition();
            var raw = _combatSystem.QueryInRange(center, _detectionRadius);
            _filtered.Clear();
            if (raw != null)
            {
                foreach (var c in raw)
                {
                    if (c != null && c != _playerCombatant)
                        _filtered.Add(c);
                }
            }
            EnemiesInRange = _filtered;

            if (_filtered.Count > 0)
                Log.Info($"[Combat] Player sees {_filtered.Count} enemies");
        }
    }
}
