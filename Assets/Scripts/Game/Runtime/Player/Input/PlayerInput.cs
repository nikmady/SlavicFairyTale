using UnityEngine;

namespace Game.Runtime.Player.Input
{
    /// <summary>
    /// Reads player move input (WASD / Arrow keys). Pure logic, no MonoBehaviour.
    /// </summary>
    public class PlayerInput
    {
        /// <summary>Returns normalized move direction or Vector2.zero if no input.</summary>
        public Vector2 ReadMoveInput()
        {
            float h = UnityEngine.Input.GetAxisRaw("Horizontal");
            float v = UnityEngine.Input.GetAxisRaw("Vertical");
            var raw = new Vector2(h, v);
            if (raw.sqrMagnitude <= 0.0001f)
                return Vector2.zero;
            return raw.normalized;
        }
    }
}
