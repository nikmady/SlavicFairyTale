using UnityEngine;
using Game.Runtime.Combat;
using Game.Runtime.Interaction;
using Game.Runtime.Services;
using Game.View.Player;

namespace Game.Runtime.Player
{
    /// <summary>
    /// Creates PlayerRuntime, PlayerView prefab, PlayerViewBinder, CameraFollow. Registers in CombatSystem. Tick Runtime + Binder (PHASE 16).
    /// </summary>
    public class PlayerSystem
    {
        private const string PlayerViewPrefabPath = "PlayerView";

        private readonly CombatSystem _combatSystem;
        private readonly InteractionSystem _interactionSystem;
        private PlayerRuntime _playerRuntime;
        private Game.View.Player.PlayerView _view;
        private PlayerViewBinder _binder;
        private GameObject _viewRoot;

        public PlayerRuntime PlayerRuntime => _playerRuntime;
        public Vector2 GetPosition() => _playerRuntime != null ? _playerRuntime.Position : Vector2.zero;

        public PlayerSystem(CombatSystem combatSystem, InteractionSystem interactionSystem)
        {
            _combatSystem = combatSystem;
            _interactionSystem = interactionSystem;
        }

        public void Initialize(PlayerHealthRuntime health)
        {
            _playerRuntime = new PlayerRuntime(Vector2.zero, health, 5f);

            var prefab = Resources.Load<GameObject>(PlayerViewPrefabPath);
            if (prefab != null)
            {
                _viewRoot = Object.Instantiate(prefab);
                _view = _viewRoot.GetComponent<Game.View.Player.PlayerView>();
            }
            if (_view == null)
            {
                _viewRoot = new GameObject("Player");
                _view = _viewRoot.AddComponent<Game.View.Player.PlayerView>();
                _viewRoot.AddComponent<PlayerMovement>();
            }

            _viewRoot.name = "Player";
            _view.SetPosition(_playerRuntime.Position);
            _binder = new PlayerViewBinder(_playerRuntime, _view);

            _combatSystem?.Register(_playerRuntime);
            Log.Info("[Player] Spawned");
        }

        public void SetInput(Vector2 input)
        {
            _playerRuntime?.SetInput(input);
        }

        public void Tick(float dt)
        {
            if (_playerRuntime == null) return;

            _playerRuntime.Tick(dt);
            _binder?.Tick();
        }

        public void Dispose()
        {
            _combatSystem?.Unregister(_playerRuntime);
            _playerRuntime = null;
            _binder = null;
            if (_viewRoot != null)
                Object.Destroy(_viewRoot);
            _viewRoot = null;
            _view = null;
        }
    }
}
