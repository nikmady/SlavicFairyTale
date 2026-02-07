using UnityEngine;

namespace Game.Runtime.Enemy
{
    /// <summary>
    /// View-only: visual representation of an enemy. SpriteRenderer, no logic.
    /// Position is driven from outside (EnemyRuntime.Tick).
    /// In Editor: draws player detection radius in Scene view (match WorldRuntime.CombatDetectionRadius).
    /// </summary>
    public class EnemyView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField, Tooltip("Radius in which player detects this enemy. Should match CombatDetectionRadius in WorldRuntime.")]
        private float _playerDetectionRadius = 8f;

        public SpriteRenderer SpriteRenderer => _spriteRenderer;

        /// <summary>View-only: set world position. Called by EnemyViewBinder (PHASE 17).</summary>
        public void SetPosition(Vector2 worldPos)
        {
            transform.position = new Vector3(worldPos.x, worldPos.y, transform.position.z);
        }

        private void Awake()
        {
            if (_spriteRenderer == null)
                _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer == null)
                _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            UnityEditor.Handles.color = new Color(1f, 0.3f, 0.2f, 0.35f);
            UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.forward, _playerDetectionRadius);
        }
#endif
    }
}
