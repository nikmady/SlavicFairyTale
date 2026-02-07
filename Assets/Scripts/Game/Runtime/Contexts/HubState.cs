using System;
using System.Collections.Generic;

namespace Game.Runtime.Contexts
{
    [Serializable]
    public class HubState
    {
        public List<BuildingEntry> buildingsList = new List<BuildingEntry>();
        private Dictionary<string, int> _buildings;
        private Action _onChanged;

        public void SetOnChanged(Action callback)
        {
            _onChanged = callback;
        }

        private void EnsureBuildings()
        {
            if (_buildings != null) return;
            _buildings = new Dictionary<string, int>();
            if (buildingsList == null) return;
            foreach (var e in buildingsList)
            {
                if (e == null || string.IsNullOrEmpty(e.buildingId)) continue;
                _buildings[e.buildingId] = e.level;
            }
        }

        private void SyncToList()
        {
            buildingsList.Clear();
            if (_buildings == null) return;
            foreach (var kv in _buildings)
                buildingsList.Add(new BuildingEntry { buildingId = kv.Key, level = kv.Value });
        }

        public void Build(string buildingId)
        {
            if (string.IsNullOrEmpty(buildingId)) return;
            EnsureBuildings();
            if (!_buildings.ContainsKey(buildingId))
                _buildings[buildingId] = 1;
            else
                _buildings[buildingId]++;
            SyncToList();
            _onChanged?.Invoke();
        }

        public void Upgrade(string buildingId)
        {
            if (string.IsNullOrEmpty(buildingId)) return;
            EnsureBuildings();
            if (_buildings.TryGetValue(buildingId, out int level))
            {
                _buildings[buildingId] = level + 1;
                SyncToList();
                _onChanged?.Invoke();
            }
        }

        [Serializable]
        public class BuildingEntry
        {
            public string buildingId;
            public int level;
        }
    }
}
