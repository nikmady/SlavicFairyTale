using UnityEngine;

namespace Game.Runtime.Player
{
    /// <summary>
    /// Holds Camera and follows a target position. No Update — position set from outside (PlayerRuntime.Tick).
    /// Simple follow, no smoothing.
    /// </summary>
    [RequireComponent(typeof(global::UnityEngine.Camera))]
    public class PlayerCameraRig : MonoBehaviour
    {
        private global::UnityEngine.Camera _camera;
        private static readonly Vector3 CameraOffset = new Vector3(0f, 0f, -10f);

        public global::UnityEngine.Camera Camera => _camera;

        private void Awake()
        {
            _camera = GetComponent<global::UnityEngine.Camera>();
        }

        /// <summary>Move camera to follow given world position (2D). Called from PlayerRuntime.Tick.</summary>
        public void Follow(Vector2 worldPosition)
        {
            transform.position = new Vector3(worldPosition.x, worldPosition.y, 0f) + CameraOffset;
        }
    }
}
