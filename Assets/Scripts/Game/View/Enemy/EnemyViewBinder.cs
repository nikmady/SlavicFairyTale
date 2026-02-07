using Game.Runtime.Enemy;

namespace Game.View.Enemy
{
    /// <summary>
    /// Binds EnemyRuntime to EnemyView. Tick: view.SetPosition(runtime.Position) (PHASE 17).
    /// </summary>
    public class EnemyViewBinder
    {
        private readonly EnemyRuntime _runtime;
        private readonly EnemyView _view;

        public EnemyViewBinder(EnemyRuntime runtime, EnemyView view)
        {
            _runtime = runtime;
            _view = view;
        }

        public void Tick()
        {
            if (_runtime != null && _view != null && _runtime.isAlive)
                _view.SetPosition(_runtime.Position);
        }
    }
}
