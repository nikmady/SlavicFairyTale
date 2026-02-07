namespace Game.Runtime.World
{
    public static class DefaultWorldGraph
    {
        public static WorldGraphData Create()
        {
            var data = new WorldGraphData();

            data.nodes.Add(new WorldNodeData
            {
                nodeId = "village",
                displayName = "Village",
                biomeId = "grassland",
                isStartNode = true,
                outgoingEdgeIds = new System.Collections.Generic.List<string> { "village_forest" }
            });
            data.nodes.Add(new WorldNodeData
            {
                nodeId = "forest",
                displayName = "Forest",
                biomeId = "forest",
                isStartNode = false,
                outgoingEdgeIds = new System.Collections.Generic.List<string> { "forest_swamp", "forest_ruins" }
            });
            data.nodes.Add(new WorldNodeData
            {
                nodeId = "swamp",
                displayName = "Swamp",
                biomeId = "swamp",
                isStartNode = false,
                outgoingEdgeIds = new System.Collections.Generic.List<string>()
            });
            data.nodes.Add(new WorldNodeData
            {
                nodeId = "ruins",
                displayName = "Ruins",
                biomeId = "ruins",
                isStartNode = false,
                outgoingEdgeIds = new System.Collections.Generic.List<string>()
            });

            data.edges.Add(new WorldEdgeData { edgeId = "village_forest", fromNodeId = "village", toNodeId = "forest", unlockConditionId = "" });
            data.edges.Add(new WorldEdgeData { edgeId = "forest_swamp", fromNodeId = "forest", toNodeId = "swamp", unlockConditionId = "" });
            data.edges.Add(new WorldEdgeData { edgeId = "forest_ruins", fromNodeId = "forest", toNodeId = "ruins", unlockConditionId = "" });

            return data;
        }
    }
}
