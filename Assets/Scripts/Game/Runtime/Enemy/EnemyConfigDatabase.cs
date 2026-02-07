using System.Collections.Generic;
using UnityEngine;

namespace Game.Runtime.Enemy
{
    // -------------------------------------------------------------------------
    // TEST SETUP (PHASE 11C)
    // -------------------------------------------------------------------------
    // 1) Create EnemyConfig asset: Right-click in Project → Create → Game → Enemy Config.
    //    Set enemyId (e.g. "enemy_skeleton"), maxHealth, and assign enemyViewPrefab.
    // 2) EnemyView prefab: Create a prefab with root GameObject that has EnemyView component
    //    (and optionally SpriteRenderer as child or on same object). Assign this prefab to
    //    EnemyConfig.enemyViewPrefab.
    // 3) Link with EnemySpawnPoint: Set EnemySpawnPoint.enemyId (on spawn point in chunk)
    //    to the same string as EnemyConfig.enemyId (e.g. "enemy_skeleton").
    // 4) Put EnemyConfigDatabase in a Resources folder: Create asset via Create → Game →
    //    Enemy Config Database, add your EnemyConfig entries to configs list, save the asset
    //    as "EnemyConfigDatabase" inside any Resources folder (e.g. Assets/Resources/).
    // -------------------------------------------------------------------------

    /// <summary>
    /// Database of enemy configs. Load via Resources (e.g. Resources.Load&lt;EnemyConfigDatabase&gt;("EnemyConfigDatabase")).
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyConfigDatabase", menuName = "Game/Enemy Config Database", order = 1)]
    public class EnemyConfigDatabase : ScriptableObject
    {
        public List<EnemyConfig> configs = new List<EnemyConfig>();

        public EnemyConfig GetById(string enemyId)
        {
            if (string.IsNullOrEmpty(enemyId) || configs == null) return null;
            foreach (var c in configs)
            {
                if (c != null && c.enemyId == enemyId)
                    return c;
            }
            return null;
        }
    }
}
