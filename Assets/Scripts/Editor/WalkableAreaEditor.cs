using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WalkableArea))]
public class WalkableAreaEditor : Editor
{
    SerializedProperty _points;
    SerializedProperty _segmentStarts;
    SerializedProperty _loop;
    SerializedProperty _obstacles;
    WalkableArea _area;
    Transform _tr;

    static bool _drawMode;
    static WalkableArea _drawTarget;
    static bool _drawObstacle;
    static float _brushRadius = 1f;
    static float _pointSpacing = 0.5f;
    static Vector2? _lastCenterLocal;
    static Vector2? _lastAddedPointLocal;
    static WalkableArea _selectionTarget;
    static int _selectedZonePoint = -1;
    static int _selectedObstacleIndex = -1;
    static int _selectedObstaclePointIndex = -1;

    void OnEnable()
    {
        _area = (WalkableArea)target;
        _tr = _area.transform;
        _points = serializedObject.FindProperty("_points");
        _segmentStarts = serializedObject.FindProperty("_segmentStarts");
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
        Event e = Event.current;
        if (e.type == EventType.KeyDown && (e.keyCode == KeyCode.Delete || e.keyCode == KeyCode.Backspace) && _selectionTarget == _area)
        {
            if (_selectedZonePoint >= 0 && _selectedZonePoint < _points.arraySize)
            {
                serializedObject.Update();
                DeleteZonePoint(_selectedZonePoint);
                serializedObject.ApplyModifiedProperties();
                _selectedZonePoint = -1;
                e.Use();
            }
            else if (_selectedObstacleIndex >= 0 && _selectedObstacleIndex < _obstacles.arraySize)
            {
                var obs = _obstacles.GetArrayElementAtIndex(_selectedObstacleIndex).FindPropertyRelative("points");
                if (_selectedObstaclePointIndex >= 0 && _selectedObstaclePointIndex < obs.arraySize)
                {
                    serializedObject.Update();
                    obs.DeleteArrayElementAtIndex(_selectedObstaclePointIndex);
                    serializedObject.ApplyModifiedProperties();
                    e.Use();
                }
                _selectedObstacleIndex = -1;
                _selectedObstaclePointIndex = -1;
            }
        }
        serializedObject.Update();
        float handleSize = HandleUtility.GetHandleSize(Vector3.zero) * 0.075f;
        float pickSize = handleSize * 2f;
        int c = _points.arraySize;
        for (int i = 0; i < c; i++)
        {
            SerializedProperty elem = _points.GetArrayElementAtIndex(i);
            Vector2 local = elem.vector2Value;
            Vector3 world = _tr.TransformPoint(local);
            bool selected = _selectionTarget == _area && _selectedZonePoint == i;
            Handles.color = selected ? Color.yellow : new Color(0f, 0.8f, 0f);
            Handles.DrawSolidDisc(world, Vector3.forward, handleSize);
            Handles.DrawWireDisc(world, Vector3.forward, handleSize);
            if (Handles.Button(world, Quaternion.identity, handleSize, pickSize, Handles.CircleHandleCap))
            {
                _selectionTarget = _area;
                _selectedZonePoint = i;
                _selectedObstacleIndex = -1;
                _selectedObstaclePointIndex = -1;
                e.Use();
            }
            Handles.color = Color.white;
            if (selected)
            {
                EditorGUI.BeginChangeCheck();
                world = Handles.PositionHandle(world, Quaternion.identity);
                if (EditorGUI.EndChangeCheck()) elem.vector2Value = _tr.InverseTransformPoint(world);
            }
        }
        for (int o = 0; o < _obstacles.arraySize; o++)
        {
            var obs = _obstacles.GetArrayElementAtIndex(o).FindPropertyRelative("points");
            for (int i = 0; i < obs.arraySize; i++)
            {
                Vector2 local = obs.GetArrayElementAtIndex(i).vector2Value;
                Vector3 world = _tr.TransformPoint(local);
                bool selected = _selectionTarget == _area && _selectedObstacleIndex == o && _selectedObstaclePointIndex == i;
                Handles.color = selected ? Color.yellow : new Color(1f, 0.4f, 0.4f);
                Handles.DrawSolidDisc(world, Vector3.forward, handleSize);
                Handles.DrawWireDisc(world, Vector3.forward, handleSize);
                if (Handles.Button(world, Quaternion.identity, handleSize, pickSize, Handles.CircleHandleCap))
                {
                    _selectionTarget = _area;
                    _selectedZonePoint = -1;
                    _selectedObstacleIndex = o;
                    _selectedObstaclePointIndex = i;
                    e.Use();
                }
                Handles.color = Color.white;
                if (selected)
                {
                    EditorGUI.BeginChangeCheck();
                    world = Handles.PositionHandle(world, Quaternion.identity);
                    if (EditorGUI.EndChangeCheck()) obs.GetArrayElementAtIndex(i).vector2Value = _tr.InverseTransformPoint(world);
                }
            }
        }
        serializedObject.ApplyModifiedProperties();
    }

    void DeleteZonePoint(int index)
    {
        _points.DeleteArrayElementAtIndex(index);
        for (int j = _segmentStarts.arraySize - 1; j >= 0; j--)
            if (_segmentStarts.GetArrayElementAtIndex(j).intValue == index)
                _segmentStarts.DeleteArrayElementAtIndex(j);
        for (int j = 0; j < _segmentStarts.arraySize; j++)
            if (_segmentStarts.GetArrayElementAtIndex(j).intValue > index)
                _segmentStarts.GetArrayElementAtIndex(j).intValue--;
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
            serializedObject.Update();
            if (_drawObstacle)
            {
                _obstacles.arraySize++;
                _obstacles.GetArrayElementAtIndex(_obstacles.arraySize - 1).FindPropertyRelative("points").ClearArray();
            }
            else
            {
                if (_segmentStarts.arraySize == 0) { _segmentStarts.arraySize = 1; _segmentStarts.GetArrayElementAtIndex(0).intValue = 0; }
                else { _segmentStarts.arraySize++; _segmentStarts.GetArrayElementAtIndex(_segmentStarts.arraySize - 1).intValue = _points.arraySize; }
            }
            serializedObject.ApplyModifiedProperties();
            _lastCenterLocal = centerLocal;
            _lastAddedPointLocal = null;
            e.Use();
        }
        else if (e.type == EventType.MouseDrag && e.button == 0)
        {
            Vector2 delta = centerLocal - (_lastCenterLocal ?? centerLocal);
            if (delta.sqrMagnitude > 0.0001f)
            {
                Vector2 dir = delta.normalized;
                Vector2 pointOnCircle = centerLocal - dir * _brushRadius;
                float spacingSq = _pointSpacing * _pointSpacing;
                bool add = !_lastAddedPointLocal.HasValue || (pointOnCircle - _lastAddedPointLocal.Value).sqrMagnitude >= spacingSq;
                if (add)
                {
                    serializedObject.Update();
                    if (_drawObstacle)
                    {
                        var obs = _obstacles.GetArrayElementAtIndex(_obstacles.arraySize - 1).FindPropertyRelative("points");
                        obs.arraySize++;
                        obs.GetArrayElementAtIndex(obs.arraySize - 1).vector2Value = pointOnCircle;
                    }
                    else
                    {
                        _points.arraySize++;
                        _points.GetArrayElementAtIndex(_points.arraySize - 1).vector2Value = pointOnCircle;
                    }
                    serializedObject.ApplyModifiedProperties();
                    _lastAddedPointLocal = pointOnCircle;
                }
                _lastCenterLocal = centerLocal;
            }
            e.Use();
        }
        else if (e.type == EventType.MouseUp && e.button == 0)
        {
            serializedObject.Update();
            if (_drawObstacle && _obstacles.arraySize > 0)
            {
                var obs = _obstacles.GetArrayElementAtIndex(_obstacles.arraySize - 1).FindPropertyRelative("points");
                if (obs.arraySize < 3) _obstacles.DeleteArrayElementAtIndex(_obstacles.arraySize - 1);
            }
            serializedObject.ApplyModifiedProperties();
            _lastCenterLocal = null;
            _lastAddedPointLocal = null;
            e.Use();
        }
        SceneView.RepaintAll();
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
