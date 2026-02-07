using UnityEngine;
using Game.Runtime.Services;

namespace Game.Bootstrap
{
    [DefaultExecutionOrder(-10000)]
    public class GameRootInstaller : MonoBehaviour
    {
        private static readonly string PrefabPath = "GameRoot";

        private void Awake()
        {
            if (FindObjectOfType<GameRoot>() != null)
                return;

            GameObject prefab = Resources.Load<GameObject>(PrefabPath);
            if (prefab == null)
            {
                Log.Error($"GameRoot prefab not found at Resources/{PrefabPath}. Place GameRoot.prefab in Assets/Resources/.");
                return;
            }

            Instantiate(prefab);
            Log.Info("GameRoot instantiated from prefab.");
        }
    }
}
