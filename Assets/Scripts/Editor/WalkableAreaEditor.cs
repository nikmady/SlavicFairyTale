using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WalkableArea))]
public class WalkableAreaEditor : Editor
{
    SerializedProperty _points;
    SerializedProperty _loop;
    SerializedProperty _obstacles;
    WalkableArea _area;
    Transform _tr;

    static bool _drawMode;
    static WalkableArea _drawTarget;
    static bool _drawObstacle;
    static float _brushRadius = 1f;
    static float _pointSpacing = 0.5f;
    static List<Vector2> _strokePath = new List<Vector2>();
    static float _pathStep = 0.03f;

    void OnEnable()
    {
        _area = (WalkableArea)target;
        _tr = _area.transform;
        _points = serializedObject.FindProperty("_points");
        _loop = serializedObject.FindProperty("_loop");
        _obstacles = serializedObject.FindProperty("_obstacles");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(_loop);
        EditorGUILayout.Space(4);
        _drawMode = EditorGUILayout.Toggle("Draw", _drawMode);
        if (_drawMode) _drawTarget = _area;
        else _drawTarget = null;
        EditorGUI.BeginDisabledGroup(!_drawMode);
        _drawObstacle = EditorGUILayout.Toggle("Draw Obstacle (hole)", _drawObstacle);
        _brushRadius = Mathf.Max(0.1f, EditorGUILayout.FloatField("Brush Radius", _brushRadius));
        _pointSpacing = Mathf.Max(0.3f, EditorGUILayout.FloatField("Point Spacing", _pointSpacing));
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("Points", EditorStyles.boldLabel);
        int c = _points.arraySize;
        for (int i = 0; i < c; i++)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_points.GetArrayElementAtIndex(i), new GUIContent($"Point {i}"));
            if (GUILayout.Button("X", GUILayout.Width(22))) { _points.DeleteArrayElementAtIndex(i); c--; i--; }
            EditorGUILayout.EndHorizontal();
        }
        if (GUILayout.Button("Add Point")) _points.arraySize = c + 1;
        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("Obstacles", EditorStyles.boldLabel);
        for (int o = 0; o < _obstacles.arraySize; o++)
        {
            var obs = _obstacles.GetArrayElementAtIndex(o).FindPropertyRelative("points");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Obstacle {o} ({obs.arraySize} pts)");
            if (GUILayout.Button("X", GUILayout.Width(22))) { _obstacles.DeleteArrayElementAtIndex(o); o--; }
            EditorGUILayout.EndHorizontal();
        }
        serializedObject.ApplyModifiedProperties();
    }

    void OnSceneGUI()
    {
        if (_drawMode && _drawTarget == _area)
        {
            DrawModeSceneGUI();
            return;
        }
        int c = _points.arraySize;
        if (c > 0)
        {
            serializedObject.Update();
            for (int i = 0; i < c; i++)
            {
                SerializedProperty elem = _points.GetArrayElementAtIndex(i);
                Vector2 local = elem.vector2Value;
                Vector3 world = _tr.TransformPoint(local);
                EditorGUI.BeginChangeCheck();
                world = Handles.PositionHandle(world, Quaternion.identity);
                if (EditorGUI.EndChangeCheck()) elem.vector2Value = _tr.InverseTransformPoint(world);
            }
            serializedObject.ApplyModifiedProperties();
        }
    }

    void DrawModeSceneGUI()
    {
        Event e = Event.current;
        Vector2? localOpt = MouseToLocal(e.mousePosition);
        if (localOpt == null) return;

        Vector2 centerLocal = localOpt.Value;
        Vector3 world = _tr.TransformPoint(centerLocal);
        Handles.color = new Color(0f, 1f, 0f, 0.25f);
        Handles.DrawSolidDisc(world, Vector3.forward, _brushRadius);
        Handles.color = _drawObstacle ? Color.red : Color.green;
        Handles.DrawWireDisc(world, Vector3.forward, _brushRadius);

        if (e.type == EventType.MouseDown && e.button == 0)
        {
            _strokePath.Clear();
            _strokePath.Add(centerLocal);
            e.Use();
        }
        else if (e.type == EventType.MouseDrag && e.button == 0)
        {
            float sq = _pathStep * _pathStep;
            if (_strokePath.Count == 0 || (centerLocal - _strokePath[_strokePath.Count - 1]).sqrMagnitude >= sq)
                _strokePath.Add(centerLocal);
            e.Use();
        }
        else if (e.type == EventType.MouseUp && e.button == 0)
        {
            if (_strokePath.Count >= 2)
            {
                List<Vector2> outline = StrokeToOutline(_strokePath, _brushRadius);
                outline = SimplifyPolygon(outline, _pointSpacing);
                outline = RemovePointsInside(outline);
                if (outline.Count >= 3)
                {
                    serializedObject.Update();
                    if (_drawObstacle)
                    {
                        int idx = _obstacles.arraySize;
                        _obstacles.arraySize++;
                        var obs = _obstacles.GetArrayElementAtIndex(idx).FindPropertyRelative("points");
                        obs.ClearArray();
                        for (int i = 0; i < outline.Count; i++) { obs.arraySize++; obs.GetArrayElementAtIndex(i).vector2Value = outline[i]; }
                    }
                    else
                    {
                        _points.ClearArray();
                        for (int i = 0; i < outline.Count; i++) { _points.arraySize++; _points.GetArrayElementAtIndex(i).vector2Value = outline[i]; }
                    }
                    serializedObject.ApplyModifiedProperties();
                }
            }
            _strokePath.Clear();
            e.Use();
        }

        if (_strokePath.Count >= 2)
        {
            Handles.color = _drawObstacle ? new Color(1f, 0.3f, 0.3f, 0.8f) : new Color(0.3f, 1f, 0.3f, 0.8f);
            for (int i = 0; i < _strokePath.Count - 1; i++)
                Handles.DrawLine(_tr.TransformPoint(_strokePath[i]), _tr.TransformPoint(_strokePath[i + 1]), 3f);
        }
        SceneView.RepaintAll();
    }

    static List<Vector2> StrokeToOutline(List<Vector2> path, float radius)
    {
        var left = new List<Vector2>();
        var right = new List<Vector2>();
        for (int i = 0; i < path.Count - 1; i++)
        {
            Vector2 d = (path[i + 1] - path[i]).normalized;
            Vector2 perp = new Vector2(-d.y, d.x);
            left.Add(path[i] + perp * radius);
            right.Add(path[i] - perp * radius);
        }
        int n = path.Count - 1;
        Vector2 lastD = (path[n] - path[n - 1]).normalized;
        Vector2 lastPerp = new Vector2(-lastD.y, lastD.x);
        left.Add(path[n] + lastPerp * radius);
        right.Add(path[n] - lastPerp * radius);
        var outList = new List<Vector2>();
        for (int i = 0; i < left.Count; i++) outList.Add(left[i]);
        for (int i = right.Count - 1; i >= 0; i--) outList.Add(right[i]);
        return outList;
    }

    static List<Vector2> SimplifyPolygon(List<Vector2> poly, float spacing)
    {
        if (poly == null || poly.Count < 3) return poly;
        var result = new List<Vector2> { poly[0] };
        float totalLen = 0f;
        float nextAt = spacing;
        for (int i = 0; i < poly.Count; i++)
        {
            int j = (i + 1) % poly.Count;
            float len = (poly[j] - poly[i]).magnitude;
            if (len < 0.0001f) continue;
            while (totalLen + len >= nextAt)
            {
                float t = (nextAt - totalLen) / len;
                result.Add(Vector2.Lerp(poly[i], poly[j], t));
                nextAt += spacing;
            }
            totalLen += len;
        }
        return result.Count >= 3 ? result : poly;
    }

    static bool PointInPolygon(Vector2 p, List<Vector2> poly)
    {
        int n = poly.Count;
        bool inside = false;
        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            if ((poly[i].y > p.y) == (poly[j].y > p.y)) continue;
            float t = (p.y - poly[i].y) / (poly[j].y - poly[i].y);
            if (p.x < poly[i].x + t * (poly[j].x - poly[i].x)) inside = !inside;
        }
        return inside;
    }

    static List<Vector2> RemovePointsInside(List<Vector2> poly)
    {
        if (poly == null || poly.Count <= 3) return poly;
        var list = new List<Vector2>(poly);
        bool changed = true;
        while (changed && list.Count > 3)
        {
            changed = false;
            for (int i = 0; i < list.Count; i++)
            {
                var without = new List<Vector2>(list.Count - 1);
                for (int k = 0; k < list.Count; k++) if (k != i) without.Add(list[k]);
                if (PointInPolygon(list[i], without))
                {
                    list.RemoveAt(i);
                    changed = true;
                    break;
                }
            }
        }
        return list;
    }

    static Vector2? MouseToLocal(Vector2 guiPos)
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(guiPos);
        if (Mathf.Abs(ray.direction.z) < 0.0001f) return null;
        float t = -ray.origin.z / ray.direction.z;
        Vector3 world = ray.origin + t * ray.direction;
        if (_drawTarget == null) return new Vector2(world.x, world.y);
        Vector3 local = _drawTarget.transform.InverseTransformPoint(world);
        return new Vector2(local.x, local.y);
    }
}
