using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnPoint : MonoBehaviour
{
    [SerializeField] float _radius = 3f;
    [SerializeField] List<EnemyConfig> _spawnableTypes = new List<EnemyConfig>();

    public float Radius => _radius;
    public IReadOnlyList<EnemyConfig> SpawnableTypes => _spawnableTypes;

    public EnemyConfig GetRandomType()
    {
        if (_spawnableTypes == null || _spawnableTypes.Count == 0) return null;
        return _spawnableTypes[Random.Range(0, _spawnableTypes.Count)];
    }

    public Vector3 GetRandomPosition()
    {
        Vector2 r = Random.insideUnitCircle * _radius;
        return transform.position + new Vector3(r.x, r.y, 0f);
    }

    public GameObject SpawnOne()
    {
        EnemyConfig config = GetRandomType();
        if (config == null || config.prefab == null) return null;
        Vector3 pos = GetRandomPosition();
        GameObject go = Instantiate(config.prefab, pos, Quaternion.identity);
        var instance = go.GetComponent<EnemyInstance>();
        if (instance != null) instance.SetConfig(config);
        return go;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.3f);
        Gizmos.DrawSphere(transform.position, _radius);
    }
}
