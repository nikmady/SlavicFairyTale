using UnityEngine;

namespace Game.Runtime.Enemy
{
    /// <summary>
    /// Data-marker for enemy spawn location. Does NOT spawn by itself; ChunkEnemyRegistrar reads it.
    /// Place in chunk hierarchy; set enemyId in Inspector.
    /// </summary>
    public class EnemySpawnPoint : MonoBehaviour
    {
        public string enemyId = "enemy_default";
    }
}
