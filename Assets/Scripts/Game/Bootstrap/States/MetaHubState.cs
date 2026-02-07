using Game.Runtime.Services;

namespace Game.Bootstrap.States
{
    public class MetaHubState : IGameState
    {
        private readonly GameStateMachine _machine;

        public string StateId => "MetaHub";

        public MetaHubState(GameStateMachine machine)
        {
            _machine = machine;
        }

        public void Enter()
        {
            Log.Info("Enter MetaHub");
        }

        public void Exit()
        {
            Log.Info("Exit MetaHub");
        }

        public void Tick(float deltaTime)
        {
        }
    }
}
