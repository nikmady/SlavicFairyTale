using System.Collections.Generic;
using UnityEngine;

namespace Game.Runtime.World
{
    /// <summary>
    /// Набор ChunkConfig для локации. Загружается из Resources, передаётся в ChunkStreamingSystem.
    /// </summary>
    [CreateAssetMenu(fileName = "ChunkConfigSet", menuName = "Game/Chunk Config Set", order = 2)]
    public class ChunkConfigSet : ScriptableObject
    {
        public List<ChunkConfig> configs = new List<ChunkConfig>();
    }
}
