using System;
using UnityEngine;

namespace Game.Runtime.Enemy
{
    /// <summary>
    /// Simple AI: move toward player when in aggro range, stop at stop distance. Not a MonoBehaviour (PHASE 17).
    /// </summary>
    public class EnemyAI
    {
        private readonly EnemyRuntime _runtime;
        private readonly Func<Vector2> _getPlayerPosition;

        public EnemyAI(EnemyRuntime runtime, Func<Vector2> getPlayerPosition)
        {
            _runtime = runtime;
            _getPlayerPosition = getPlayerPosition;
        }

        public void Tick(float dt)
        {
            if (_runtime == null || !_runtime.isAlive) return;

            var playerPos = _getPlayerPosition != null ? _getPlayerPosition() : Vector2.zero;
            var target = playerPos + _runtime.TargetOffset;
            var toPlayer = target - _runtime.Position;
            var distance = toPlayer.magnitude;

            if (distance > _runtime.AggroRadius)
                return;

            if (distance <= _runtime.StopDistance)
                return;

            var dir = toPlayer.normalized;
            _runtime.Position += dir * _runtime.MoveSpeed * dt;
        }
    }
}
