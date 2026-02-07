using System;
using UnityEngine;
using Game.Runtime.World.Chunks;
using Game.Runtime.Player;
using Game.Runtime.Player.Input;
using Game.Runtime.Player.Movement;
using Game.Runtime.Interaction;
using Game.Runtime.Enemy;

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
        private PlayerAnchor _playerAnchor;
        private PlayerRuntime _playerRuntime;
        private PlayerInput _playerInput;
        private PlayerMovement _playerMovement;
        private InteractionSystem _interactionSystem;
        private EnemySystem _enemySystem;
        private bool _disposed;

        private const float InteractionRequestRadius = 5f;

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
            _playerAnchor = new PlayerAnchor { Position = Vector2.zero };
            _chunkStreamer = new ChunkStreamer(_chunkSource, 30f);
            _playerInput = new PlayerInput();
            _playerMovement = new PlayerMovement(_playerAnchor, 5f);
            _interactionSystem = new InteractionSystem();
            _enemySystem = new EnemySystem();
            EnemySystemRegistry.Set(_enemySystem);
            _playerRuntime = new PlayerRuntime(_playerAnchor);
            _playerRuntime.Initialize();
            _context.isInitialized = true;
            InteractionSystemRegistry.Set(_interactionSystem);
            OnWorldRuntimeReady?.Invoke();
        }

        public void Tick(float dt)
        {
            if (_context == null || !_context.isInitialized || _disposed) return;

            _chunkStreamer?.UpdateStreaming(_playerAnchor.Position);

            Vector2 input = _playerInput.ReadMoveInput();
            _playerMovement.ApplyMovement(input, dt);

            if (UnityEngine.Input.GetKeyDown(KeyCode.E))
            {
                var request = new InteractionRequest { origin = _playerAnchor.Position, radius = InteractionRequestRadius };
                _interactionSystem.TryInteract(request);
            }

            _enemySystem?.Tick(dt);
            _playerRuntime?.Tick(dt);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            OnWorldRuntimeDisposed?.Invoke();
            InteractionSystemRegistry.Clear();
            _playerRuntime?.Dispose();
            _playerRuntime = null;
            _playerInput = null;
            _playerMovement = null;
            _interactionSystem = null;
            EnemySystemRegistry.Clear();
            _enemySystem = null;
            _chunkStreamer = null;
            _playerAnchor = null;
            _chunkSource = null;
            _context = null;
        }
    }
}
