using System;

namespace Game.Runtime.Player
{
    public class PlayerHealthRuntime
    {
        private int _currentHP;

        public int maxHP { get; private set; }
        public int currentHP => _currentHP;
        public bool IsAlive { get; private set; } = true;

        public event Action OnDeath;

        public PlayerHealthRuntime(int maxHP = 100)
        {
            this.maxHP = Math.Max(1, maxHP);
            _currentHP = this.maxHP;
        }

        public void ApplyDamage(int dmg)
        {
            if (!IsAlive || dmg <= 0) return;
            _currentHP = Math.Max(0, _currentHP - dmg);
            if (_currentHP <= 0)
            {
                IsAlive = false;
                OnDeath?.Invoke();
            }
        }

        public void Heal(int value)
        {
            if (!IsAlive || value <= 0) return;
            _currentHP = Math.Min(maxHP, _currentHP + value);
        }
    }
}
