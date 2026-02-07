using UnityEngine;
using Game.Runtime.Services;
using Game.Runtime.World;
using Game.Runtime.World.Chunks;
using Game.Bootstrap;

namespace Game.Bootstrap.States
{
    /// <summary>
    /// Single point where the playable world is loaded. No other state must perform world load.
    /// Future: BeginLoadLocation will trigger chunk/addressable load for the given nodeId;
    /// CompleteLoadLocation will run when load is done and transition to RunLocation.
    /// Currently: no scene load, no chunks, no addressables — only logging.
    /// </summary>
    public class LoadLocationState : IGameState
    {
        private readonly GameStateMachine _machine;
        private readonly GameRoot _gameRoot;

        public string StateId => "LoadLocation";

        public LoadLocationState(GameStateMachine machine)
        {
            _machine = machine;
            _gameRoot = null;
        }

        public LoadLocationState(GameStateMachine machine, GameRoot gameRoot)
        {
            _machine = machine;
            _gameRoot = gameRoot;
        }

        public void Enter()
        {
            Log.Info("Enter LoadLocation");
            string nodeId = _gameRoot?.SelectedWorldNodeId ?? "";
            Log.Info($"Preparing location: {nodeId}");
            BeginLoadLocation(nodeId);
            SetChunkSourceForLocation(nodeId);
            CompleteLoadLocation();
        }

        public void Exit()
        {
            Log.Info("Exit LoadLocation");
        }

        public void Tick(float deltaTime)
        {
        }

        /// <summary>
        /// Single point of world creation. Creates WorldRuntime via GameRoot (WorldBootstrap).
        /// TODO: Resolve LocationChunkSet by locationId here in future; set via WorldRuntime.SetChunkSource.
        /// Future: load addressable chunks by nodeId, show loading UI, then call CompleteLoadLocation.
        /// </summary>
        private void BeginLoadLocation(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId) || _gameRoot == null) return;
            string biomeId = "default";
            var graph = _gameRoot.CurrentWorldGraph;
            var node = graph?.Data?.GetNode(nodeId);
            if (node != null && !string.IsNullOrEmpty(node.biomeId))
                biomeId = node.biomeId;
            _gameRoot.CreateWorldRuntime(nodeId, biomeId);
        }

        /// <summary>
        /// Resolve LocationChunkSet for locationId and set on WorldRuntime. Streaming works with zero chunks if none found.
        /// </summary>
        private void SetChunkSourceForLocation(string locationId)
        {
            var world = _gameRoot?.CurrentWorldRuntime;
            if (world == null) return;
            var sets = Resources.LoadAll<LocationChunkSet>("");
            LocationChunkSet chunkSet = null;
            foreach (var set in sets)
            {
                if (set != null && set.locationId == locationId)
                {
                    chunkSet = set;
                    break;
                }
            }
            if (chunkSet != null)
                world.SetChunkSource(chunkSet);
            else
                Log.Warn($"No LocationChunkSet found for locationId '{locationId}'; chunk streaming will have zero chunks.");
        }

        /// <summary>
        /// Called when world is ready. Transition to RunLocation.
        /// Future: finalize streamed chunks, spawn player, then switch.
        /// </summary>
        private void CompleteLoadLocation()
        {
            _machine?.SwitchState("RunLocation");
        }
    }
}
