using UnityEngine;

namespace Game.Runtime.World
{
    /// <summary>
    /// Изолированный загрузчик чанков: только Resources.Load, Instantiate, Destroy.
    /// Без Unity Editor, без Addressables — позже addressableKey можно использовать для Addressables.LoadAsync.
    /// </summary>
    public class ChunkLoader
    {
        /// <summary>
        /// Загружает чанк по конфигу: Resources.Load по addressableKey, Instantiate, позиция из config.worldPosition.
        /// </summary>
        public GameObject LoadChunk(ChunkConfig config)
        {
            if (config == null || string.IsNullOrEmpty(config.addressableKey))
                return null;

            var prefab = Resources.Load<GameObject>(config.addressableKey);
            if (prefab == null)
                return null;

            var instance = Object.Instantiate(prefab);
            instance.name = "Chunk_" + (string.IsNullOrEmpty(config.chunkId) ? config.addressableKey : config.chunkId);
            instance.transform.position = new Vector3(config.worldPosition.x, config.worldPosition.y, 0f);
            return instance;
        }

        /// <summary>
        /// Выгружает чанк: уничтожает GameObject экземпляра.
        /// </summary>
        public void UnloadChunk(ChunkInstance chunkInstance)
        {
            if (chunkInstance == null) return;
            if (chunkInstance.instance != null)
                Object.Destroy(chunkInstance.instance);
            chunkInstance.instance = null;
            chunkInstance.IsLoaded = false;
        }
    }
}
