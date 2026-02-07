using UnityEngine;

namespace Game.Runtime.Player
{
    public class PlayerInputBehaviour : MonoBehaviour
    {
        private PlayerController _controller;

        public void Init(PlayerController controller)
        {
            _controller = controller;
        }

        private void Update()
        {
            if (_controller == null) return;
            if (UnityEngine.Input.GetKeyDown(KeyCode.Q))
                _controller.OnAbilityPressed("ability_primary");
            if (UnityEngine.Input.GetKeyDown(KeyCode.E))
                _controller.OnAbilityPressed("ability_secondary");
            if (UnityEngine.Input.GetKeyDown(KeyCode.R))
                _controller.OnAbilityPressed("ability_ultimate");
        }
    }
}
