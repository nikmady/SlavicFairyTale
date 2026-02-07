using UnityEngine;

namespace Game.Runtime.World
{
    /// <summary>
    /// Runtime-экземпляр загруженного чанка: конфиг, корневой GameObject, флаг загрузки.
    /// </summary>
    public class ChunkInstance
    {
        public ChunkConfig config;
        public GameObject instance;
        public bool IsLoaded;

        public ChunkInstance(ChunkConfig config, GameObject instance)
        {
            this.config = config;
            this.instance = instance;
            IsLoaded = instance != null;
        }
    }
}
