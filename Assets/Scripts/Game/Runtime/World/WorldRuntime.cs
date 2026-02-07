using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Runtime.World.Chunks;
using Game.Runtime.Player;
using Game.Runtime.Interaction;
using Game.Runtime.Enemy;
using Game.Runtime.Combat;
using Game.Runtime.Abilities;
using Game.Runtime.Camera;
using Game.Runtime.Services;
using Game.View.Camera;

namespace Game.Runtime.World
{
    /// <summary>
    /// Runtime container for the active world. Owns WorldRuntimeContext and lifecycle.
    /// Does NOT know about MetaContext, StateMachine, or UI. Not a MonoBehaviour.
    /// </summary>
    public class WorldRuntime
    {
        private WorldRuntimeContext _context;
        private IChunkSource _chunkSource;
        private ChunkStreamer _chunkStreamer;
        private ChunkStreamingSystem _chunkStreamingSystem;
        private InteractionSystem _interactionSystem;
        private EnemyConfigDatabase _enemyConfigDatabase;
        private CombatSystem _combatSystem;
        private EnemySystem _enemySystem;
        private PlayerSystem _playerSystem;
        private PlayerCombatProbe _playerCombatProbe;
        private AbilitySystem _abilitySystem;
        private AutoCombatResolver _autoCombatResolver;
        private PlayerController _playerController;
        private GameObject _playerInputBehaviourRoot;
        private PlayerHealthRuntime _playerHealth;
        private CameraFollowRuntime _cameraFollowRuntime;
        private CameraBoundsProvider _cameraBoundsProvider;
        private CameraViewBinder _cameraViewBinder;
        private GameObject _cameraRoot;
        private bool _disposed;

        private const float InteractionRequestRadius = 5f;
        private static readonly Rect DefaultWorldBounds = new Rect(-500f, -500f, 1000f, 1000f);
        private const float CombatDetectionRadius = 8f;
        private const float CombatAttackInterval = 1f;

        public WorldRuntimeContext Context => _context;
        public InteractionSystem InteractionSystem => _interactionSystem;

        public event Action OnWorldRuntimeReady;
        public event Action OnWorldRuntimeDisposed;
        public event Action OnRequestRunEnd;

        private void RequestRunEnd()
        {
            OnRequestRunEnd?.Invoke();
        }

        public void SetChunkSource(IChunkSource source)
        {
            _chunkSource = source;
            _chunkStreamer?.SetSource(source);
        }

        public WorldRuntime(WorldRuntimeContext context)
        {
            _context = context ?? new WorldRuntimeContext();
        }

        public void Initialize()
        {
            if (_context == null || _disposed) return;
            _chunkStreamer = new ChunkStreamer(_chunkSource, 30f);
            _interactionSystem = new InteractionSystem();
            _enemyConfigDatabase = Resources.Load<EnemyConfigDatabase>("EnemyConfigDatabase");
            _combatSystem = new CombatSystem();
            _combatSystem.OnCombatantDeath += _ => Log.Info("[Combat] Enemy died");
            _playerHealth = new PlayerHealthRuntime(100);
            _playerHealth.OnDeath += RequestRunEnd;
            _playerSystem = new PlayerSystem(_combatSystem, _interactionSystem);
            PlayerSystemRegistry.Set(_playerSystem);
            _playerSystem.Initialize(_playerHealth);

            var chunkConfigSet = Resources.Load<ChunkConfigSet>("ChunkConfigSet");
            var chunkConfigs = chunkConfigSet != null && chunkConfigSet.configs != null ? chunkConfigSet.configs : new List<ChunkConfig>();
            if (chunkConfigs.Count > 0)
            {
                _chunkStreamingSystem = new ChunkStreamingSystem(_playerSystem.PlayerRuntime, 30f, chunkConfigs, new ChunkLoader());
            }

            _enemySystem = new EnemySystem(_enemyConfigDatabase, _combatSystem, () => _playerSystem.GetPosition(), _playerSystem.PlayerRuntime);
            EnemySystemRegistry.Set(_enemySystem);
            _playerCombatProbe = new PlayerCombatProbe(_combatSystem, () => _playerSystem.GetPosition(), CombatDetectionRadius, _playerSystem.PlayerRuntime);
            _abilitySystem = new AbilitySystem(_combatSystem, _playerSystem.PlayerRuntime);
            foreach (var config in Resources.LoadAll<AbilityConfig>(""))
                _abilitySystem.AddAbility(config);
            _autoCombatResolver = new AutoCombatResolver(_combatSystem, _playerCombatProbe, _playerSystem.PlayerRuntime, _abilitySystem, CombatAttackInterval);
            _playerController = new PlayerController(_abilitySystem, _playerCombatProbe);
            _playerInputBehaviourRoot = new GameObject("PlayerAbilityInput");
            _playerInputBehaviourRoot.AddComponent<PlayerInputBehaviour>().Init(_playerController);
            _cameraBoundsProvider = new CameraBoundsProvider();
            _cameraBoundsProvider.SetBounds(DefaultWorldBounds);
            _cameraFollowRuntime = new CameraFollowRuntime(8f);
            _cameraFollowRuntime.SetWorldBounds(_cameraBoundsProvider.GetBounds());
            _cameraFollowRuntime.SetTargetPosition(_playerSystem.GetPosition());
            _cameraFollowRuntime.CurrentPosition = _playerSystem.GetPosition();
            _cameraRoot = new GameObject("WorldCamera");
            var cameraView = _cameraRoot.AddComponent<CameraView>();
            _cameraViewBinder = new CameraViewBinder(_cameraFollowRuntime, cameraView);
            _cameraViewBinder.Tick();
            _context.isInitialized = true;
            InteractionSystemRegistry.Set(_interactionSystem);
            OnWorldRuntimeReady?.Invoke();
        }

        public void Tick(float dt)
        {
            if (_context == null || !_context.isInitialized || _disposed) return;

            if (_chunkStreamingSystem != null)
                _chunkStreamingSystem.Tick(dt);
            else
                _chunkStreamer?.UpdateStreaming(_playerSystem.GetPosition());

            if (UnityEngine.Input.GetKeyDown(KeyCode.E))
            {
                var request = new InteractionRequest { origin = _playerSystem.GetPosition(), radius = InteractionRequestRadius };
                _interactionSystem.TryInteract(request);
            }

            _enemySystem?.Tick(dt);
            _playerCombatProbe?.Tick(dt);
            _abilitySystem?.Tick(dt);
            _autoCombatResolver?.Tick(dt);
            _playerSystem?.Tick(dt);
            _cameraFollowRuntime?.SetTargetPosition(_playerSystem.GetPosition());
            _cameraFollowRuntime?.SetWorldBounds(_cameraBoundsProvider.GetBounds());
            _cameraFollowRuntime?.Tick(dt);
            _cameraViewBinder?.Tick();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            OnWorldRuntimeDisposed?.Invoke();
            InteractionSystemRegistry.Clear();
            PlayerSystemRegistry.Clear();
            _playerSystem?.Dispose();
            _playerSystem = null;
            _interactionSystem = null;
            EnemySystemRegistry.Clear();
            _enemySystem = null;
            _autoCombatResolver = null;
            if (_playerInputBehaviourRoot != null)
                UnityEngine.Object.Destroy(_playerInputBehaviourRoot);
            _playerInputBehaviourRoot = null;
            _playerController = null;
            if (_playerHealth != null)
            {
                _playerHealth.OnDeath -= RequestRunEnd;
                _playerHealth = null;
            }
            _abilitySystem = null;
            _playerCombatProbe = null;
            _cameraViewBinder = null;
            _cameraFollowRuntime = null;
            _cameraBoundsProvider = null;
            if (_cameraRoot != null)
                UnityEngine.Object.Destroy(_cameraRoot);
            _cameraRoot = null;
            _combatSystem = null;
            _chunkStreamer = null;
            _chunkStreamingSystem = null;
            _chunkSource = null;
            _context = null;
        }
    }
}
