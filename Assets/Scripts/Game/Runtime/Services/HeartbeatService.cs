using UnityEngine;

namespace Game.Runtime.Services
{
    public class HeartbeatService : ITickable
    {
        private float _elapsedTime;
        private int _tickCount;

        public float ElapsedTime => _elapsedTime;
        public int TickCount => _tickCount;

        public void Tick(float deltaTime)
        {
            _elapsedTime += deltaTime;
            _tickCount++;
        }

        public void Reset()
        {
            _elapsedTime = 0f;
            _tickCount = 0;
        }

        public string GetStatusString()
        {
            return $"Elapsed: {_elapsedTime:F2}s, Ticks: {_tickCount}";
        }
    }
}
