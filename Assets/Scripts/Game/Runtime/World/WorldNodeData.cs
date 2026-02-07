using System;
using System.Collections.Generic;

namespace Game.Runtime.World
{
    /// <summary>
    /// Data for one node in the world graph. No references to scenes, prefabs, or chunks.
    /// nodeId is the stable key: in future it will be used to resolve which world content
    /// (e.g. addressable chunk prefabs) to load for this location. This type does not reference them.
    /// </summary>
    [Serializable]
    public class WorldNodeData
    {
        public string nodeId;
        public string displayName;
        public string biomeId;
        public bool isStartNode;
        public List<string> outgoingEdgeIds = new List<string>();
    }
}
