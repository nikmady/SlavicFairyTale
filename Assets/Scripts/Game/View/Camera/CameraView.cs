using UnityEngine;

namespace Game.View.Camera
{
    public class CameraView : MonoBehaviour
    {
        [SerializeField] private UnityEngine.Camera _camera;
        [SerializeField] private float _zOffset = -10f;

        public UnityEngine.Camera Camera => _camera;

        private void Awake()
        {
            if (_camera == null)
                _camera = GetComponent<UnityEngine.Camera>();
            if (_camera == null)
                _camera = gameObject.AddComponent<UnityEngine.Camera>();
        }

        public void SetPosition(Vector2 worldPos)
        {
            transform.position = new Vector3(worldPos.x, worldPos.y, _zOffset);
        }
    }
}
