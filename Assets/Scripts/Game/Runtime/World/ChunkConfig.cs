using UnityEngine;

namespace Game.Runtime.World
{
    /// <summary>
    /// ScriptableObject: конфигурация чанка (id, позиция в мире, размер, ключ для загрузки).
    /// Addressables-ready: addressableKey можно заменить на ключ Addressables без смены архитектуры.
    /// </summary>
    [CreateAssetMenu(fileName = "ChunkConfig", menuName = "Game/Chunk Config", order = 1)]
    public class ChunkConfig : ScriptableObject
    {
        public string chunkId;
        public Vector2 worldPosition;
        public Vector2 size;
        public string addressableKey;
    }
}
