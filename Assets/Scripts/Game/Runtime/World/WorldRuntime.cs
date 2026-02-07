using System;
using UnityEngine;
using Game.Runtime.World.Chunks;
using Game.Runtime.Player;
using Game.Runtime.Interaction;
using Game.Runtime.Enemy;
using Game.Runtime.Combat;
using Game.Runtime.Abilities;
using Game.Runtime.Services;

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
        private InteractionSystem _interactionSystem;
        private EnemyConfigDatabase _enemyConfigDatabase;
        private CombatSystem _combatSystem;
        private EnemySystem _enemySystem;
        private PlayerSystem _playerSystem;
        private PlayerCombatProbe _playerCombatProbe;
        private AbilitySystem _abilitySystem;
        private AutoCombatResolver _autoCombatResolver;
        private bool _disposed;

        private const float InteractionRequestRadius = 5f;
        private const float CombatDetectionRadius = 8f;
        private const float CombatAttackInterval = 1f;

        public WorldRuntimeContext Context => _context;
        public InteractionSystem InteractionSystem => _interactionSystem;

        public event Action OnWorldRuntimeReady;
        public event Action OnWorldRuntimeDisposed;

        /// <summary>Set chunk source (e.g. LocationChunkSet). Also forwards to ChunkStreamer.</summary>
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
            _playerSystem = new PlayerSystem(_combatSystem, _interactionSystem);
            PlayerSystemRegistry.Set(_playerSystem);
            _playerSystem.Initialize();
            _enemySystem = new EnemySystem(_enemyConfigDatabase, _combatSystem, () => _playerSystem.GetPosition());
            EnemySystemRegistry.Set(_enemySystem);
            _playerCombatProbe = new PlayerCombatProbe(_combatSystem, () => _playerSystem.GetPosition(), CombatDetectionRadius, _playerSystem.PlayerRuntime);
            _abilitySystem = new AbilitySystem(_combatSystem, _playerSystem.PlayerRuntime);
            foreach (var config in Resources.LoadAll<AbilityConfig>(""))
                _abilitySystem.AddAbility(config);
            _autoCombatResolver = new AutoCombatResolver(_combatSystem, _playerCombatProbe, _playerSystem.PlayerRuntime, _abilitySystem, CombatAttackInterval);
            _context.isInitialized = true;
            InteractionSystemRegistry.Set(_interactionSystem);
            OnWorldRuntimeReady?.Invoke();
        }

        public void Tick(float dt)
        {
            if (_context == null || !_context.isInitialized || _disposed) return;

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
            _abilitySystem = null;
            _playerCombatProbe = null;
            _combatSystem = null;
            _chunkStreamer = null;
            _chunkSource = null;
            _context = null;
        }
    }
}
