using System.Collections.Generic;
using System.Linq;
using Game.Runtime.Contexts;

namespace Game.Runtime.World
{
    /// <summary>
    /// Runtime view of the world graph: nodes, edges, reachability. No dependency on Unity scenes,
    /// chunks, prefabs, or Addressables. Pure logic and data only.
    /// nodeId is the logical key; in future it will be used to decide which world content (e.g. chunk set)
    /// to load — but this class does NOT perform or reference any loading.
    /// </summary>
    public class WorldGraphRuntime
    {
        private readonly WorldGraphData _data;
        private readonly MetaContext _meta;

        public WorldGraphRuntime(WorldGraphData data, MetaContext meta)
        {
            _data = data ?? new WorldGraphData();
            _meta = meta;
        }

        public WorldGraphData Data => _data;

        public IReadOnlyList<WorldNodeData> GetAllNodes()
        {
            if (_data?.nodes == null) return new List<WorldNodeData>();
            return _data.nodes;
        }

        public bool IsNodeUnlocked(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId)) return false;
            return _meta != null && _meta.HasNodeUnlocked(nodeId);
        }

        public bool IsNodeReachable(string fromNodeId, string toNodeId)
        {
            if (string.IsNullOrEmpty(fromNodeId) || string.IsNullOrEmpty(toNodeId)) return false;
            if (fromNodeId == toNodeId) return true;
            var node = _data.GetNode(fromNodeId);
            if (node?.outgoingEdgeIds == null) return false;
            foreach (string edgeId in node.outgoingEdgeIds)
            {
                var edge = _data.edges?.FirstOrDefault(e => e != null && e.edgeId == edgeId);
                if (edge != null && edge.toNodeId == toNodeId)
                    return true;
            }
            return false;
        }

        public bool CanTravel(string fromNodeId, string toNodeId)
        {
            if (string.IsNullOrEmpty(fromNodeId) || string.IsNullOrEmpty(toNodeId)) return false;
            if (!IsNodeUnlocked(fromNodeId) || !IsNodeUnlocked(toNodeId)) return false;
            var node = _data.GetNode(fromNodeId);
            if (node?.outgoingEdgeIds == null) return false;
            foreach (string edgeId in node.outgoingEdgeIds)
            {
                var edge = _data.edges?.FirstOrDefault(e => e != null && e.edgeId == edgeId);
                if (edge != null && edge.toNodeId == toNodeId)
                    return true;
            }
            return false;
        }

        public void UnlockNode(string nodeId)
        {
            _meta?.unlocks?.UnlockNode(nodeId);
        }

        public void UnlockEdge(string edgeId)
        {
            _meta?.unlocks?.UnlockEdge(edgeId);
        }
    }
}
