using UnityEngine;
using Game.Runtime.Combat;

namespace Game.Runtime.Player
{
    /// <summary>
    /// Runtime player entity. Position, input, movement. Implements ICombatant. Not a MonoBehaviour (PHASE 16).
    /// </summary>
    public class PlayerRuntime : ICombatant
    {
        public Vector2 Position { get; set; }
        public Vector2 InputVector { get; private set; }
        public float MoveSpeed { get; set; }

        Vector2 ICombatant.WorldPosition => Position;
        bool ICombatant.IsAlive => true;
        CombatStats ICombatant.Stats => new CombatStats { maxHealth = 100f, currentHealth = 100f, attackPower = 10f };
        void ICombatant.TakeDamage(float damage) { }

        public PlayerRuntime(Vector2 startPosition, float moveSpeed = 5f)
        {
            Position = startPosition;
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
