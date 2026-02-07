using Game.Runtime.Services;
using Game.Bootstrap;

namespace Game.Bootstrap.States
{
    public class RunLocationState : IGameState
    {
        private readonly GameStateMachine _machine;
        private readonly GameRoot _gameRoot;

        public string StateId => "RunLocation";

        public RunLocationState(GameStateMachine machine)
        {
            _machine = machine;
            _gameRoot = null;
        }

        public RunLocationState(GameStateMachine machine, GameRoot gameRoot)
        {
            _machine = machine;
            _gameRoot = gameRoot;
        }

        public void Enter()
        {
            Log.Info("Enter RunLocation");
            var world = _gameRoot?.CurrentWorldRuntime;
            if (world?.Context != null)
                Log.Info($"WorldRuntime active for location {world.Context.locationId}");
        }

        public void Exit()
        {
            Log.Info("Exit RunLocation");
        }

        public void Tick(float deltaTime)
        {
            _gameRoot?.CurrentWorldRuntime?.Tick(deltaTime);
        }
    }
}
