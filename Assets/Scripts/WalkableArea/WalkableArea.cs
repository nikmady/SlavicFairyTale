using System.Collections.Generic;
using UnityEngine;

public class WalkableArea : MonoBehaviour
{
    [SerializeField] List<Vector2> _points = new List<Vector2>();
    [SerializeField] bool _loop = true;
    [SerializeField] List<ObstaclePoints> _obstacles = new List<ObstaclePoints>();

    [System.Serializable]
    public class ObstaclePoints { public List<Vector2> points = new List<Vector2>(); }

    Vector2[] _world;
    int _count;
    List<Vector2[]> _obstacleWorld;

    void Awake() => CacheWorld();

    void CacheWorld()
    {
        if (_points == null || _points.Count < 3) { _world = null; _count = 0; }
        else
        {
            _count = _points.Count + (_loop ? 1 : 0);
            _world = new Vector2[_count];
            for (int i = 0; i < _points.Count; i++)
                _world[i] = transform.TransformPoint(_points[i]);
            if (_loop) _world[_points.Count] = _world[0];
        }
        _obstacleWorld = new List<Vector2[]>();
        if (_obstacles != null)
            foreach (var obs in _obstacles)
                if (obs.points != null && obs.points.Count >= 3)
                {
                    var arr = new Vector2[obs.points.Count + 1];
                    for (int i = 0; i < obs.points.Count; i++) arr[i] = transform.TransformPoint(obs.points[i]);
                    arr[obs.points.Count] = arr[0];
                    _obstacleWorld.Add(arr);
                }
    }

    public Vector3 ClampToBounds(Vector3 worldPos)
    {
        Vector2 p = worldPos;
        if (_world != null && _count >= 3 && IsInsidePoly(p, _world, _count))
        {
            if (_obstacleWorld != null)
                foreach (var obs in _obstacleWorld)
                    if (IsInsidePoly(p, obs, obs.Length)) return (Vector3)ClosestOnPoly(p, obs);
            return worldPos;
        }
        if (_world != null && _count >= 3) return (Vector3)ClosestOnPoly(p, _world);
        return worldPos;
    }

    static bool IsInsidePoly(Vector2 p, Vector2[] w, int count)
    {
        int n = count - 1;
        bool inside = false;
        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            if ((w[i].y > p.y) == (w[j].y > p.y)) continue;
            float t = (p.y - w[i].y) / (w[j].y - w[i].y);
            if (p.x < w[i].x + t * (w[j].x - w[i].x)) inside = !inside;
        }
        return inside;
    }

    static Vector2 ClosestOnPoly(Vector2 p, Vector2[] w)
    {
        int n = w.Length - 1;
        float best = float.MaxValue;
        Vector2 bestPoint = p;
        for (int i = 0; i < n; i++)
        {
            int j = i + 1;
            Vector2 a = w[i], b = w[j];
            Vector2 ab = b - a;
            float len = ab.sqrMagnitude;
            float t = len > 0.0001f ? Mathf.Clamp01(Vector2.Dot(p - a, ab) / len) : 0f;
            Vector2 q = a + t * ab;
            float d = (p - q).sqrMagnitude;
            if (d < best) { best = d; bestPoint = q; }
        }
        return bestPoint;
    }

    public List<Vector2> Points => _points;
    public List<ObstaclePoints> Obstacles => _obstacles;
    public bool Loop { get => _loop; set => _loop = value; }
    public void SetDirty() { if (Application.isPlaying) CacheWorld(); }
}
