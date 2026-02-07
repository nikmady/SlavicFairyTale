using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Runtime.Services;

namespace Game.Runtime.Interaction
{
    /// <summary>
    /// Sits on chunk root. Registers all IInteractable in children with the current InteractionSystem.
    /// Uses InteractionSystemRegistry (no FindObjectOfType). Handles late binding when WorldRuntime is created after OnEnable.
    /// </summary>
    public class ChunkInteractionRegistrar : MonoBehaviour
    {
        private readonly List<IInteractable> _cachedInteractables = new List<IInteractable>();
        private InteractionSystem _registeredSystem;
        private bool _subscribedToReady;

        public void RegisterAll()
        {
            var system = InteractionSystemRegistry.Current;
            if (system == null) return;

            CacheInteractables();
            foreach (var i in _cachedInteractables)
            {
                if (i != null)
                    system.Register(i);
            }
            _registeredSystem = system;
            Log.Info($"[Game] Registered {_cachedInteractables.Count} interactables in chunk {gameObject.name}");
        }

        public void UnregisterAll()
        {
            if (_registeredSystem == null) return;
            foreach (var i in _cachedInteractables)
            {
                if (i != null)
                    _registeredSystem.Unregister(i);
            }
            Log.Info($"[Game] Unregistered interactables in chunk {gameObject.name}");
            _registeredSystem = null;
        }

        private void CacheInteractables()
        {
            _cachedInteractables.Clear();
            var monos = GetComponentsInChildren<MonoBehaviour>(true);
            foreach (var m in monos)
            {
                if (m != null && m is IInteractable ia)
                    _cachedInteractables.Add(ia);
            }
        }

        private void OnEnable()
        {
            if (InteractionSystemRegistry.Current != null)
            {
                RegisterAll();
            }
            else
            {
                _subscribedToReady = true;
                InteractionSystemRegistry.OnReady += OnWorldRuntimeReady;
            }
        }

        private void OnWorldRuntimeReady()
        {
            if (!_subscribedToReady) return;
            _subscribedToReady = false;
            InteractionSystemRegistry.OnReady -= OnWorldRuntimeReady;
            RegisterAll();
        }

        private void OnDisable()
        {
            if (_subscribedToReady)
            {
                _subscribedToReady = false;
                InteractionSystemRegistry.OnReady -= OnWorldRuntimeReady;
            }
            UnregisterAll();
        }
    }
}
