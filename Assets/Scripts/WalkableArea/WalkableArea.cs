using System.Collections.Generic;
using UnityEngine;

public class WalkableArea : MonoBehaviour
{
    [SerializeField] List<Vector2> _points = new List<Vector2>();
    [SerializeField] List<int> _segmentStarts = new List<int>();
    [SerializeField] bool _loop = true;
    [SerializeField] List<ObstaclePoints> _obstacles = new List<ObstaclePoints>();

    [System.Serializable]
    public class ObstaclePoints { public List<Vector2> points = new List<Vector2>(); }

    List<Vector2[]> _zonePolys;
    List<Vector2[]> _obstacleWorld;

    void Awake() => CacheWorld();

    void CacheWorld()
    {
        _zonePolys = new List<Vector2[]>();
        if (_points != null && _points.Count >= 3)
        {
            var starts = _segmentStarts != null && _segmentStarts.Count > 0 ? _segmentStarts : new List<int> { 0 };
            for (int s = 0; s < starts.Count; s++)
            {
                int start = starts[s];
                int end = s + 1 < starts.Count ? starts[s + 1] : _points.Count;
                if (end - start < 3) continue;
                var w = new Vector2[end - start + (_loop ? 1 : 0)];
                for (int i = 0; i < end - start; i++) w[i] = transform.TransformPoint(_points[start + i]);
                if (_loop) w[end - start] = w[0];
                _zonePolys.Add(w);
            }
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
        if (_zonePolys != null)
            foreach (var w in _zonePolys)
                if (IsInsidePoly(p, w, w.Length))
                {
                    if (_obstacleWorld != null)
                        foreach (var obs in _obstacleWorld)
                            if (IsInsidePoly(p, obs, obs.Length)) return (Vector3)ClosestOnPoly(p, obs);
                    return worldPos;
                }
        Vector2 best = p;
        float bestD = float.MaxValue;
        if (_zonePolys != null)
            foreach (var w in _zonePolys)
            {
                Vector2 q = ClosestOnPoly(p, w);
                float d = (p - q).sqrMagnitude;
                if (d < bestD) { bestD = d; best = q; }
            }
        return new Vector3(best.x, best.y, 0f);
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

    public bool IsInObstacle(Vector2 worldPoint)
    {
        if (_obstacleWorld == null) return false;
        foreach (var obs in _obstacleWorld)
            if (IsInsidePoly(worldPoint, obs, obs.Length)) return true;
        return false;
    }

    public List<Vector2> Points => _points;
    public List<int> SegmentStarts => _segmentStarts ?? (_segmentStarts = new List<int>());
    public List<ObstaclePoints> Obstacles => _obstacles;
    public bool Loop { get => _loop; set => _loop = value; }
    public void SetDirty() { if (Application.isPlaying) CacheWorld(); }
}
