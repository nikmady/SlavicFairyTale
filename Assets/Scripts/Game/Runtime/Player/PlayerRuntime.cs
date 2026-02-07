using UnityEngine;
using Game.Runtime.Combat;

namespace Game.Runtime.Player
{
    public class PlayerRuntime : ICombatant
    {
        private readonly PlayerHealthRuntime _health;

        public Vector2 Position { get; set; }
        public Vector2 InputVector { get; private set; }
        public float MoveSpeed { get; set; }
        public PlayerHealthRuntime Health => _health;

        Vector2 ICombatant.WorldPosition => Position;
        bool ICombatant.IsAlive => _health != null && _health.IsAlive;
        CombatStats ICombatant.Stats => new CombatStats
        {
            maxHealth = _health != null ? _health.maxHP : 0,
            currentHealth = _health != null ? _health.currentHP : 0,
            attackPower = 10f
        };

        void ICombatant.TakeDamage(float damage)
        {
            _health?.ApplyDamage(Mathf.RoundToInt(damage));
        }

        public PlayerRuntime(Vector2 startPosition, PlayerHealthRuntime health, float moveSpeed = 5f)
        {
            Position = startPosition;
            _health = health;
            MoveSpeed = Mathf.Max(0f, moveSpeed);
        }

        public void SetInput(Vector2 input)
        {
            InputVector = input.sqrMagnitude > 1f ? input.normalized : input;
        }

        public void Tick(float dt)
        {
            Position += InputVector * MoveSpeed * dt;
        }
    }
}
