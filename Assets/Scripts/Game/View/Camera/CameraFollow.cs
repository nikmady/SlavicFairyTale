using UnityEngine;

namespace Game.View.Camera
{
    /// <summary>
    /// Follows target transform. Updates camera position every frame (PHASE 16).
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private float _zOffset = -10f;

        public Transform Target
        {
            get => _target;
            set => _target = value;
        }

        private void LateUpdate()
        {
            if (_target == null) return;
            var p = _target.position;
            transform.position = new Vector3(p.x, p.y, p.z + _zOffset);
        }
    }
}
