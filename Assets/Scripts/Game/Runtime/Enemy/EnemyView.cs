using UnityEngine;

namespace Game.Runtime.Enemy
{
    /// <summary>
    /// View-only: visual representation of an enemy. SpriteRenderer, no logic.
    /// Position is driven from outside (EnemyRuntime.Tick).
    /// </summary>
    public class EnemyView : MonoBehaviour
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
