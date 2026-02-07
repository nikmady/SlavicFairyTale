using Game.Runtime.Services;
using Game.Runtime.World;
using Game.Bootstrap;

namespace Game.Bootstrap.States
{
    public class WorldMapState : IGameState
    {
        private readonly GameStateMachine _machine;
        private readonly GameRoot _gameRoot;
        private WorldGraphRuntime _runtime;

        public string StateId => "WorldMap";

        public WorldMapState(GameStateMachine machine)
        {
            _machine = machine;
            _gameRoot = null;
        }

        public WorldMapState(GameStateMachine machine, GameRoot gameRoot)
        {
            _machine = machine;
            _gameRoot = gameRoot;
        }

        public void Enter()
        {
            Log.Info("Enter WorldMap");
            if (_gameRoot?.Meta == null)
            {
                Log.Warn("WorldMapState entered without MetaContext");
                return;
            }
            WorldGraphData data = DefaultWorldGraph.Create();
            _runtime = new WorldGraphRuntime(data, _gameRoot.Meta);
            _gameRoot.SetCurrentWorldGraph(_runtime);
        }

        public void Exit()
        {
            Log.Info("Exit WorldMap");
            // Не очищаем граф: он нужен при клике по узлу из RunLocation (переход LoadLocation) и в LoadLocationState.
            _runtime = null;
        }

        public void Tick(float deltaTime)
        {
        }
    }
}
