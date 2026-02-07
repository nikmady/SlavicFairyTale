using System;
using System.Collections.Generic;

namespace Game.Runtime.Contexts
{
    [Serializable]
    public class UnlockState
    {
        public List<string> unlockedNodeIdsList = new List<string>();
        public List<string> unlockedEdgeIdsList = new List<string>();
        private HashSet<string> _unlockedNodeIds;
        private HashSet<string> _unlockedEdgeIds;
        private Action _onChanged;

        public void SetOnChanged(Action callback)
        {
            _onChanged = callback;
        }

        public bool IsNodeUnlocked(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId)) return false;
            EnsureSets();
            return _unlockedNodeIds.Contains(nodeId);
        }

        public bool IsEdgeUnlocked(string edgeId)
        {
            if (string.IsNullOrEmpty(edgeId)) return false;
            EnsureSets();
            return _unlockedEdgeIds.Contains(edgeId);
        }

        private void EnsureSets()
        {
            if (_unlockedNodeIds != null) return;
            _unlockedNodeIds = new HashSet<string>();
            _unlockedEdgeIds = new HashSet<string>();
            if (unlockedNodeIdsList != null)
                foreach (var id in unlockedNodeIdsList)
                    if (!string.IsNullOrEmpty(id)) _unlockedNodeIds.Add(id);
            if (unlockedEdgeIdsList != null)
                foreach (var id in unlockedEdgeIdsList)
                    if (!string.IsNullOrEmpty(id)) _unlockedEdgeIds.Add(id);
        }

        private void SyncToLists()
        {
            unlockedNodeIdsList = new List<string>(_unlockedNodeIds ?? new HashSet<string>());
            unlockedEdgeIdsList = new List<string>(_unlockedEdgeIds ?? new HashSet<string>());
        }

        public void UnlockNode(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId)) return;
            EnsureSets();
            _unlockedNodeIds.Add(nodeId);
            SyncToLists();
            _onChanged?.Invoke();
        }

        public void UnlockEdge(string edgeId)
        {
            if (string.IsNullOrEmpty(edgeId)) return;
            EnsureSets();
            _unlockedEdgeIds.Add(edgeId);
            SyncToLists();
            _onChanged?.Invoke();
        }
    }
}
