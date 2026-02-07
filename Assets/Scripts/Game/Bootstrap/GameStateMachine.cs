using System.Collections.Generic;
using Game.Bootstrap.States;
using Game.Runtime.Services;

namespace Game.Bootstrap
{
    public class GameStateMachine
    {
        private readonly Dictionary<string, IGameState> _states = new Dictionary<string, IGameState>();
        private IGameState _currentState;

        public string CurrentStateId => _currentState?.StateId ?? string.Empty;

        public void Register(IGameState state)
        {
            if (state == null) return;
            _states[state.StateId] = state;
        }

        public void SetInitialState(string stateId)
        {
            if (_states.TryGetValue(stateId, out IGameState state))
            {
                _currentState = state;
                _currentState.Enter();
            }
        }

        public void SwitchState(string stateId)
        {
            if (!_states.TryGetValue(stateId, out IGameState nextState))
                return;

            string previousId = _currentState?.StateId ?? "none";
            if (previousId == stateId)
                return;

            _currentState?.Exit();
            _currentState = nextState;
            _currentState.Enter();
            Log.Info($"State change: {previousId} -> {stateId}");
        }

        public void Tick(float deltaTime)
        {
            _currentState?.Tick(deltaTime);
        }
    }
}
