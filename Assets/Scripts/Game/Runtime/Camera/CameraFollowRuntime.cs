using UnityEngine;

namespace Game.Runtime.Camera
{
    public class CameraFollowRuntime
    {
        public Vector2 TargetPosition { get; private set; }
        public Vector2 CurrentPosition { get; set; }
        public float FollowSpeed { get; set; }
        public Rect WorldBounds { get; private set; }

        public CameraFollowRuntime(float followSpeed = 8f)
        {
            FollowSpeed = followSpeed;
        }

        public void SetTargetPosition(Vector2 pos)
        {
            TargetPosition = pos;
        }

        public void SetWorldBounds(Rect bounds)
        {
            WorldBounds = bounds;
        }

        public void Tick(float dt)
        {
            var delta = TargetPosition - CurrentPosition;
            var step = FollowSpeed * dt;
            if (delta.sqrMagnitude <= step * step)
                CurrentPosition = TargetPosition;
            else
                CurrentPosition += delta.normalized * step;

            CurrentPosition = new Vector2(
                Mathf.Clamp(CurrentPosition.x, WorldBounds.xMin, WorldBounds.xMax),
                Mathf.Clamp(CurrentPosition.y, WorldBounds.yMin, WorldBounds.yMax));
        }
    }
}
