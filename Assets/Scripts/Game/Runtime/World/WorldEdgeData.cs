using System;

namespace Game.Runtime.World
{
    [Serializable]
    public class WorldEdgeData
    {
        public string edgeId;
        public string fromNodeId;
        public string toNodeId;
        public string unlockConditionId;
    }
}
