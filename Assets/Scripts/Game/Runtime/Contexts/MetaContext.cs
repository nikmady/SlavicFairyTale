using System;

namespace Game.Runtime.Contexts
{
    /// <summary>
    /// Pure persistent data: progression, economy, hub, unlocks, current world node.
    /// Rules: no UnityEngine references, no scenes, no runtime GameObjects or MonoBehaviours.
    /// Serialized to JSON and loaded/saved by SaveService. Safe to pass across layers.
    /// </summary>
    [Serializable]
    public class MetaContext
    {
        public PlayerProgression progression = new PlayerProgression();
        public EconomyState economy = new EconomyState();
        public HubState hub = new HubState();
        public UnlockState unlocks = new UnlockState();
        public string currentWorldNodeId;

        public event Action OnMetaChanged;

        public string GetCurrentNodeId()
        {
            return currentWorldNodeId ?? string.Empty;
        }

        public void SetCurrentNode(string nodeId)
        {
            currentWorldNodeId = nodeId ?? string.Empty;
            NotifyChanged();
        }

        protected void NotifyChanged()
        {
            OnMetaChanged?.Invoke();
        }

        public void WireChangeCallbacks()
        {
            progression?.SetOnChanged(NotifyChanged);
            economy?.SetOnChanged(NotifyChanged);
            hub?.SetOnChanged(NotifyChanged);
            unlocks?.SetOnChanged(NotifyChanged);
        }

        public bool HasNodeUnlocked(string nodeId)
        {
            return unlocks != null && unlocks.IsNodeUnlocked(nodeId);
        }

        public static MetaContext CreateDefault()
        {
            var meta = new MetaContext();
            meta.progression.selectedClassId = string.Empty;
            meta.unlocks.UnlockNode("village");
            meta.currentWorldNodeId = "village";
            meta.WireChangeCallbacks();
            return meta;
        }
    }
}
