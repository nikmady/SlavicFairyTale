using UnityEngine;

namespace Game.Runtime.Interaction
{
    /// <summary>
    /// Query for interactables around a point (e.g. player position).
    /// </summary>
    public struct InteractionRequest
    {
        public Vector2 origin;
        public float radius;
    }
}
