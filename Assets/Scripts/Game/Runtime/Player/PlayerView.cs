using UnityEngine;

namespace Game.Runtime.Player
{
    /// <summary>
    /// View-only: visual representation of the player. No logic, no Update.
    /// Position is driven from outside (PlayerRuntime).
    /// </summary>
    public class PlayerView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;

        public SpriteRenderer SpriteRenderer => _spriteRenderer;

        private void Awake()
        {
            if (_spriteRenderer == null)
                _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer == null)
                _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
    }
}
