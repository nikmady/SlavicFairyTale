using UnityEngine;
using Game.Runtime.World;

namespace Game.Runtime.Player.Movement
{
    /// <summary>
    /// Applies movement to PlayerAnchor. Pure logic, no MonoBehaviour.
    /// </summary>
    public class PlayerMovement
    {
        private readonly PlayerAnchor _anchor;
        private readonly float _moveSpeed;

        public PlayerMovement(PlayerAnchor anchor, float moveSpeed)
        {
            _anchor = anchor;
            _moveSpeed = Mathf.Max(0f, moveSpeed);
        }

        public void ApplyMovement(Vector2 direction, float deltaTime)
        {
            if (_anchor == null || deltaTime <= 0f) return;
            if (direction.sqrMagnitude <= 0.0001f) return;

            var normalized = direction.normalized;
            var delta = normalized * (_moveSpeed * deltaTime);
            _anchor.Position = _anchor.Position + delta;
        }
    }
}
