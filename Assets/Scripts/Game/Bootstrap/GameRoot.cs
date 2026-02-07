using System.Collections.Generic;
using UnityEngine;
using Game.Runtime.Services;
using Game.Runtime.Contexts;
using Game.Runtime.World;
using Game.Bootstrap.States;

namespace Game.Bootstrap
{
    /// <summary>
    /// Composition Root of the game. Single entry point: owns bootstrap, tick loop, meta, and state machine.
    /// Does NOT contain gameplay logic — only wiring and lifecycle.
    /// Future extension point: WorldRuntime, ChunkStreamer, PlayerRuntime will be injected or created here (not yet).
    /// </summary>
    public class GameRoot : MonoBehaviour, IGameBootstrap
    {
        private readonly List<ITickable> _tickables = new List<ITickable>();
        private HeartbeatService _heartbeatService;
        private GameStateMachine _stateMachine;
        private SaveService _saveService;
        private MetaContext _meta;
        private WorldGraphRuntime _currentWorldGraph;
        private WorldRuntime _worldRuntime;

        // --- Extension points (reserved for future systems; do not assign yet) ---
        // WorldRuntime: runtime representation of the loaded world (chunk-based, non-scene).
        // ChunkStreamer: load/unload addressable chunk prefabs by player position.
        // PlayerRuntime: player entity and position; drives chunk streaming.
        private object _worldRuntimeReserved;
        private object _chunkStreamerReserved;
        private object _playerRuntimeReserved;

        /// <summary>For future use: inject or create WorldRuntime from this root. Currently unused.</summary>
        protected void SetWorldRuntimeReserved(object reserved) => _worldRuntimeReserved = reserved;

        /// <summary>For future use: inject or create ChunkStreamer from this root. Currently unused.</summary>
        protected void SetChunkStreamerReserved(object reserved) => _chunkStreamerReserved = reserved;

        /// <summary>For future use: inject or create PlayerRuntime from this root. Currently unused.</summary>
        protected void SetPlayerRuntimeReserved(object reserved) => _playerRuntimeReserved = reserved;

        public float ElapsedTime => _heartbeatService != null ? _heartbeatService.ElapsedTime : 0f;
        public int TotalTicks => _heartbeatService != null ? _heartbeatService.TickCount : 0;
        public GameStateMachine StateMachine => _stateMachine;
        public string CurrentStateId => _stateMachine != null ? _stateMachine.CurrentStateId : string.Empty;
        public MetaContext Meta => _meta;
        public SaveService Save => _saveService;
        public WorldGraphRuntime CurrentWorldGraph => _currentWorldGraph;
        public WorldRuntime CurrentWorldRuntime => _worldRuntime;
        public string SelectedWorldNodeId { get; set; }

        public void CreateWorldRuntime(string locationId, string biomeId)
        {
            DestroyWorldRuntime();
            _worldRuntime = WorldBootstrap.CreateWorld(locationId, biomeId);
            _worldRuntime.OnRequestRunEnd += () => _stateMachine?.SwitchState("RunEnd");
        }

        public void DestroyWorldRuntime()
        {
            if (_worldRuntime != null)
            {
                _worldRuntime.Dispose();
                _worldRuntime = null;
            }
        }

        public void SetCurrentWorldGraph(WorldGraphRuntime graph)
        {
            _currentWorldGraph = graph;
        }

        public void ClearCurrentWorldGraph()
        {
            _currentWorldGraph = null;
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            _heartbeatService = new HeartbeatService();
            _tickables.Add(_heartbeatService);
        }

        private void Start()
        {
            _saveService = new SaveService();
            _stateMachine = new GameStateMachine();
            _stateMachine.Register(new BootState(_stateMachine, (IGameBootstrap)this));
            _stateMachine.Register(new MetaHubState(_stateMachine));
            _stateMachine.Register(new WorldMapState(_stateMachine, this));
            _stateMachine.Register(new LoadLocationState(_stateMachine, this));
            _stateMachine.Register(new RunLocationState(_stateMachine, this));
            _stateMachine.Register(new RunEndState(_stateMachine, this));
            _stateMachine.SetInitialState("Boot");
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            for (int i = 0; i < _tickables.Count; i++)
            {
                _tickables[i].Tick(dt);
            }
            _stateMachine?.Tick(dt);
        }

        public void ResetHeartbeat()
        {
            if (_heartbeatService != null)
                _heartbeatService.Reset();
        }

        public string GetHeartbeatStatus()
        {
            return _heartbeatService != null ? _heartbeatService.GetStatusString() : "N/A";
        }

        public void LoadMeta()
        {
            LoadMetaFromSave();
        }

        public MetaContext GetMeta()
        {
            return _meta;
        }

        public void LoadMetaFromSave()
        {
            MetaContext loaded = _saveService != null ? _saveService.LoadMeta() : null;
            if (loaded != null)
            {
                _meta = loaded;
                _meta.WireChangeCallbacks();
                Log.Info("MetaContext loaded from save.");
            }
            else
            {
                _meta = MetaContext.CreateDefault();
                Log.Info("New MetaContext created (no save).");
            }
        }

        public void ReloadMetaFromDisk()
        {
            LoadMetaFromSave();
        }

        public void WipeProgress()
        {
            _saveService?.DeleteMetaSave();
            LoadMetaFromSave();
        }
    }
}
