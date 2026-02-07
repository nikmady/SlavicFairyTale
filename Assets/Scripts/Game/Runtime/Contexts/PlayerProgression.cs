using System;
using System.Collections.Generic;

namespace Game.Runtime.Contexts
{
    [Serializable]
    public class PlayerProgression
    {
        public int playerLevel;
        public int totalXP;
        public string selectedClassId;
        public List<string> unlockedClassIds = new List<string>();
        public List<string> unlockedSkinIds = new List<string>();

        private Action _onChanged;

        public void SetOnChanged(Action callback)
        {
            _onChanged = callback;
        }

        public void AddXP(int amount)
        {
            totalXP += amount;
            _onChanged?.Invoke();
        }

        public void SetClass(string classId)
        {
            selectedClassId = classId ?? string.Empty;
            _onChanged?.Invoke();
        }

        public void UnlockClass(string classId)
        {
            if (string.IsNullOrEmpty(classId)) return;
            if (!unlockedClassIds.Contains(classId))
                unlockedClassIds.Add(classId);
            _onChanged?.Invoke();
        }
    }
}
