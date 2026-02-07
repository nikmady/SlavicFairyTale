using Game.Runtime.Services;
using Game.Bootstrap;

namespace Game.Bootstrap.States
{
    public class RunEndState : IGameState
    {
        private readonly GameStateMachine _machine;
        private readonly GameRoot _gameRoot;

        public string StateId => "RunEnd";

        public RunEndState(GameStateMachine machine)
        {
            _machine = machine;
            _gameRoot = null;
        }

        public RunEndState(GameStateMachine machine, GameRoot gameRoot)
        {
            _machine = machine;
            _gameRoot = gameRoot;
        }

        public void Enter()
        {
            Log.Info("Run Ended");
            _gameRoot?.DestroyWorldRuntime();
            Log.Info("WorldRuntime disposed");
            _machine?.SwitchState("MetaHub");
        }

        public void Exit()
        {
            Log.Info("Exit RunEnd");
        }

        public void Tick(float deltaTime)
        {
        }
    }
}
