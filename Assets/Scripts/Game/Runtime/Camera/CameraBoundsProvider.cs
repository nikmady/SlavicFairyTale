using UnityEngine;

namespace Game.Runtime.Camera
{
    public class CameraBoundsProvider
    {
        private Rect _bounds;

        public Rect GetBounds() => _bounds;

        public void SetBounds(Rect bounds)
        {
            _bounds = bounds;
        }
    }
}
