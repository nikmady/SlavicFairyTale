using UnityEngine;

namespace Game.Runtime.Enemy
{
    /// <summary>
    /// DEPRECATED: Use ChunkEnemyRegistrar instead. This component no longer spawns enemies.
    /// Kept for backward compatibility if present on existing prefabs/scenes.
    /// </summary>
    [DisallowMultipleComponent]
    public class ChunkEnemySpawner : MonoBehaviour
    {
        private void OnEnable()
        {
            Debug.LogWarning("[Game] ChunkEnemySpawner is deprecated and does nothing. Use ChunkEnemyRegistrar on chunk root instead.");
        }
    }
}
