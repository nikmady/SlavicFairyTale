using UnityEngine;

namespace Game.Runtime.Enemy
{
    /// <summary>
    /// Runtime enemy entity. Creates/destroys view, holds position. No combat/AI in PHASE 11B.
    /// Does not know about world or player.
    /// </summary>
    public class EnemyRuntime : ICombatant
    {
        public string enemyId { get; }
        public Vector2 position { get; set; }
        public EnemyView view { get; private set; }
        public bool isAlive { get; set; }
        public string spawnChunkId { get; }

        Vector2 ICombatant.Position => position;
        bool ICombatant.IsAlive => isAlive;

        private GameObject _root;
        private bool _disposed;

        public EnemyRuntime(string enemyId, Vector2 position, string spawnChunkId = null)
        {
            this.enemyId = enemyId;
            this.position = position;
            this.spawnChunkId = spawnChunkId ?? string.Empty;
            isAlive = true;
        }

        public void Initialize()
        {
            if (_disposed) return;

            _root = new GameObject($"Enemy_{enemyId}");
            _root.transform.position = new Vector3(position.x, position.y, 0f);
            view = _root.AddComponent<EnemyView>();
        }

        public void Tick(float dt)
        {
            if (_root == null || view == null || _disposed) return;

            _root.transform.position = new Vector3(position.x, position.y, 0f);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            isAlive = false;
            if (_root != null)
                Object.Destroy(_root);
            _root = null;
            view = null;
        }
    }
}
