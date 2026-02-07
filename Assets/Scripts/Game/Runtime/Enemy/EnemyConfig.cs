using UnityEngine;

namespace Game.Runtime.Enemy
{
    /// <summary>
    /// Data-only config for one enemy type. Used by EnemySystem to spawn with correct view and stats.
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyConfig", menuName = "Game/Enemy Config", order = 0)]
    public class EnemyConfig : ScriptableObject
    {
        public string enemyId;
        public float maxHealth = 100f;
        public GameObject enemyViewPrefab;
        [Header("Movement (PHASE 17)")]
        public float moveSpeed = 3f;
        public float aggroRadius = 10f;
        public float stopDistance = 1.5f;
        [Header("Attack (PHASE 18)")]
        public float attackDamage = 10f;
        public float attackRange = 2f;
        public float attackCooldown = 1.5f;
    }
}
