using UnityEditor;
using UnityEngine;

public class WalkableAreaEditorWindow : EditorWindow
{
    WalkableArea _area;
    SerializedObject _so;
    SerializedProperty _points;
    SerializedProperty _loop;
    Vector2 _scroll;

    [MenuItem("Tools/Walkable Area Editor")]
    static void Open() => GetWindow<WalkableAreaEditorWindow>("Walkable Area");

    void OnEnable()
    {
        Selection.selectionChanged += Repaint;
    }

    void OnDisable()
    {
        Selection.selectionChanged -= Repaint;
    }

    void OnGUI()
    {
        if (Selection.activeGameObject == null) { EditorGUILayout.HelpBox("Select a GameObject with WalkableArea.", MessageType.Info); return; }
        _area = Selection.activeGameObject.GetComponent<WalkableArea>();
        if (_area == null) { EditorGUILayout.HelpBox("Selected object has no WalkableArea.", MessageType.Warning); return; }
        if (_so == null || _so.targetObject == null) _so = new SerializedObject(_area);
        _so.Update();
        _points = _so.FindProperty("_points");
        _loop = _so.FindProperty("_loop");
        EditorGUILayout.PropertyField(_loop);
        _scroll = EditorGUILayout.BeginScrollView(_scroll);
        int c = _points.arraySize;
        for (int i = 0; i < c; i++)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_points.GetArrayElementAtIndex(i), new GUIContent($"Point {i}"));
            if (GUILayout.Button("X", GUILayout.Width(22))) { _points.DeleteArrayElementAtIndex(i); c--; i--; }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
        if (GUILayout.Button("Add Point")) _points.arraySize = c + 1;
        _so.ApplyModifiedProperties();
    }
}
