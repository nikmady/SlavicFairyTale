using UnityEngine;

namespace Game.Runtime.Player
{
    /// <summary>
    /// MonoBehaviour: reads WASD, normalizes, sends to PlayerSystem.SetInput() (PHASE 16).
    /// </summary>
    public class PlayerMovement : MonoBehaviour
    {
        private void Update()
        {
            float h = UnityEngine.Input.GetAxisRaw("Horizontal");
            float v = UnityEngine.Input.GetAxisRaw("Vertical");
            var raw = new Vector2(h, v);
            var input = raw.sqrMagnitude > 1f ? raw.normalized : raw;
            PlayerSystemRegistry.Current?.SetInput(input);
        }
    }
}
