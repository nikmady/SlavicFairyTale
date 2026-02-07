using UnityEngine;
using Game.Runtime.Interaction;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Runtime.Interaction.Providers
{
    /// <summary>
    /// Stub interactable for NPCs. Registration by ChunkInteractionRegistrar (chunks) or SceneInteractableRegistrar (scene).
    /// </summary>
    public class NpcInteractable : MonoBehaviour, IInteractable
    {
        public float interactionRadius = 2f;

        public Vector2 WorldPosition => transform.position;
        public float InteractionRadius => interactionRadius;

        public bool CanInteract() => true;

        public void Interact()
        {
            UnityEngine.Debug.Log("[Game] Interacted with NPC");
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Handles.color = new Color(0.2f, 0.8f, 0.2f, 0.4f);
            Handles.DrawWireDisc(transform.position, Vector3.forward, interactionRadius);
        }
#endif
    }
}
