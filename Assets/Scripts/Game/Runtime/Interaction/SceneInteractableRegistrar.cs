using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Runtime.Services;

namespace Game.Runtime.Interaction
{
    /// <summary>
    /// For interactables placed in the scene (not under streamed chunk roots). Registers all IInteractable
    /// on this GameObject and children with InteractionSystemRegistry when WorldRuntime is ready.
    /// Add to Bootstrap or to the parent of resource_obj / rune_obj / npc_obj so they get registered.
    /// </summary>
    public class SceneInteractableRegistrar : MonoBehaviour
    {
        private readonly List<IInteractable> _cachedInteractables = new List<IInteractable>();
        private InteractionSystem _registeredSystem;
        private bool _subscribedToReady;

        private void RegisterAll()
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
            Log.Info($"[Game] Registered {_cachedInteractables.Count} scene interactables in {gameObject.name}");
        }

        private void UnregisterAll()
        {
            if (_registeredSystem == null) return;
            foreach (var i in _cachedInteractables)
            {
                if (i != null)
                    _registeredSystem.Unregister(i);
            }
            Log.Info($"[Game] Unregistered scene interactables in {gameObject.name}");
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
                RegisterAll();
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
