using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] float _radius = 3f;
    [SerializeField] List<EnemyConfig> _spawnableTypes = new List<EnemyConfig>();
    [SerializeField] List<SpawnPointSetting> _pointSettings = new List<SpawnPointSetting>();

    [System.Serializable]
    public class SpawnPointSetting
    {
        [UnityEngine.Min(0)] public int count = 1;
        public float radius = 3f;
        [UnityEngine.Min(0)] public float minDistanceBetween = 0f;
        [UnityEngine.Min(0)] public float maxDistanceBetween = 0f;
        public List<EnemyConfig> spawnableTypes = new List<EnemyConfig>();
    }

    List<Transform> _points;

    void Start()
    {
        RefreshPoints();
        SpawnAtAll();
    }

    public void RefreshPoints()
    {
        _points = new List<Transform>();
        for (int i = 0; i < transform.childCount; i++)
            _points.Add(transform.GetChild(i));
    }

    public void SpawnAtAll()
    {
        if (_points == null) RefreshPoints();
        for (int i = 0; i < _points.Count; i++)
            if (_points[i] != null) SpawnAt(_points[i], i);
    }

    public void SpawnAt(int index)
    {
        if (_points == null) RefreshPoints();
        if (index >= 0 && index < _points.Count) SpawnAt(_points[index], index);
    }

    void SpawnAt(Transform point, int index)
    {
        int count = 1;
        float r = _radius;
        float minDist = 0f, maxDist = 0f;
        List<EnemyConfig> types = _spawnableTypes;
        if (index < _pointSettings.Count && _pointSettings[index] != null)
        {
            var s = _pointSettings[index];
            count = Mathf.Max(0, s.count);
            r = s.radius;
            minDist = Mathf.Max(0f, s.minDistanceBetween);
            maxDist = Mathf.Max(0f, s.maxDistanceBetween);
            if (s.spawnableTypes != null && s.spawnableTypes.Count > 0) types = s.spawnableTypes;
        }
        Vector2 center = point.position;
        var placed = new List<Vector2>(count);
        for (int i = 0; i < count; i++)
        {
            EnemyConfig config = GetRandomType(types);
            if (config == null || config.prefab == null) continue;
            Vector2 offset = PickPosition(center, r, placed, minDist, maxDist);
            placed.Add(offset);
            Vector3 pos = point.position + new Vector3(offset.x, offset.y, 0f);
            GameObject go = Instantiate(config.prefab, pos, Quaternion.identity);
            var instance = go.GetComponent<EnemyInstance>();
            if (instance != null) instance.SetConfig(config);
            var ai = go.GetComponent<EnemyAI>();
            if (ai != null) ai.SetSpawnArea(point.position, r);
        }
    }

    static Vector2 PickPosition(Vector2 center, float radius, List<Vector2> placed, float minDist, float maxDist)
    {
        const int tries = 50;
        for (int t = 0; t < tries; t++)
        {
            Vector2 offset = (Vector2)Random.insideUnitCircle * radius;
            Vector2 pos = center + offset;
            if (placed.Count == 0) return offset;
            float nearest = float.MaxValue;
            bool tooClose = false;
            foreach (var p in placed)
            {
                float d = Vector2.Distance(pos, p);
                if (d < minDist) tooClose = true;
                if (d < nearest) nearest = d;
            }
            if (tooClose) continue;
            if (maxDist > 0f && nearest > maxDist) continue;
            return offset;
        }
        return (Vector2)Random.insideUnitCircle * radius;
    }

    static EnemyConfig GetRandomType(List<EnemyConfig> list)
    {
        if (list == null || list.Count == 0) return null;
        return list[Random.Range(0, list.Count)];
    }

    public float GetRadiusForPoint(int index)
    {
        if (index >= 0 && index < _pointSettings.Count && _pointSettings[index] != null)
            return _pointSettings[index].radius;
        return _radius;
    }

    public int PointCount => _pointSettings != null ? _pointSettings.Count : 0;
}
