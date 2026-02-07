using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Runtime.World
{
    /// <summary>
    /// Static graph definition: nodes and edges only. No scenes, chunks, or prefabs.
    /// Used by WorldGraphRuntime; nodeIds are logical keys for future world-content resolution.
    /// </summary>
    [Serializable]
    public class WorldGraphData
    {
        public List<WorldNodeData> nodes = new List<WorldNodeData>();
        public List<WorldEdgeData> edges = new List<WorldEdgeData>();

        public WorldNodeData GetNode(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId)) return null;
            return nodes?.FirstOrDefault(n => n != null && n.nodeId == nodeId);
        }

        public IEnumerable<WorldNodeData> GetConnectedNodes(string nodeId)
        {
            var node = GetNode(nodeId);
            if (node?.outgoingEdgeIds == null) yield break;
            foreach (string edgeId in node.outgoingEdgeIds)
            {
                var edge = edges?.FirstOrDefault(e => e != null && e.edgeId == edgeId);
                if (edge == null) continue;
                var toNode = GetNode(edge.toNodeId);
                if (toNode != null)
                    yield return toNode;
            }
        }
    }
}
