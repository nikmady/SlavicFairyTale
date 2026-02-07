using UnityEngine;
using Game.Runtime.Player;

namespace Game.View.Player
{
    /// <summary>
    /// Binds PlayerRuntime to PlayerView. Tick: view.SetPosition(runtime.Position) (PHASE 16).
    /// </summary>
    public class PlayerViewBinder
    {
        private readonly PlayerRuntime _runtime;
        private readonly PlayerView _view;

        public PlayerViewBinder(PlayerRuntime runtime, PlayerView view)
        {
            _runtime = runtime;
            _view = view;
        }

        public void Tick()
        {
            if (_runtime != null && _view != null)
                _view.SetPosition(_runtime.Position);
        }
    }
}
