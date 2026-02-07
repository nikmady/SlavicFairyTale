using Game.Runtime.Services;
using Game.Bootstrap;

namespace Game.Bootstrap.States
{
    public class BootState : IGameState
    {
        private readonly GameStateMachine _machine;
        private readonly IGameBootstrap _bootstrap;

        public string StateId => "Boot";

        public BootState(GameStateMachine machine)
        {
            _machine = machine;
            _bootstrap = null;
        }

        public BootState(GameStateMachine machine, IGameBootstrap bootstrap)
        {
            _machine = machine;
            _bootstrap = bootstrap;
        }

        public void Enter()
        {
            Log.Info("Enter Boot");
            if (_bootstrap != null)
                _bootstrap.LoadMeta();
        }

        public void Exit()
        {
            Log.Info("Exit Boot");
        }

        public void Tick(float deltaTime)
        {
        }
    }
}
