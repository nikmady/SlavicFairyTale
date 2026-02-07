using UnityEngine;
using Game.Runtime.World;

namespace Game.Runtime.Player
{
    /// <summary>
    /// Runtime player entity. Creates/destroys player GameObject, syncs view with anchor, drives camera.
    /// Not a MonoBehaviour; does not know about StateMachine or MetaContext.
    /// </summary>
    public class PlayerRuntime
    {
        public PlayerAnchor anchor { get; }
        public GameObject root { get; private set; }
        public PlayerView view { get; private set; }
        public PlayerCameraRig cameraRig { get; private set; }

        private bool _disposed;

        public PlayerRuntime(PlayerAnchor anchor)
        {
            this.anchor = anchor;
        }

        public void Initialize()
        {
            if (anchor == null || _disposed) return;

            root = new GameObject("Player");
            root.transform.position = new Vector3(anchor.Position.x, anchor.Position.y, 0f);
            view = root.AddComponent<PlayerView>();

            var cameraGo = new GameObject("PlayerCameraRig");
            cameraGo.AddComponent<Camera>();
            cameraRig = cameraGo.AddComponent<PlayerCameraRig>();
        }

        public void Tick(float dt)
        {
            if (root == null || view == null || cameraRig == null || _disposed) return;

            var pos = anchor.Position;
            root.transform.position = new Vector3(pos.x, pos.y, 0f);
            cameraRig.Follow(pos);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            if (cameraRig != null && cameraRig.gameObject != null)
                Object.Destroy(cameraRig.gameObject);
            cameraRig = null;
            if (root != null)
                Object.Destroy(root);
            root = null;
            view = null;
        }
    }
}
