using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemySpawner))]
public class EnemySpawnerEditor : Editor
{
    SerializedProperty _radius;
    SerializedProperty _spawnableTypes;
    SerializedProperty _pointSettings;
    EnemySpawner _spawner;
    static List<EnemyConfig> _cachedConfigs;
    static double _lastCacheTime;

    void OnEnable()
    {
        _spawner = target as EnemySpawner;
        if (_spawner == null) return;
        _radius = serializedObject.FindProperty("_radius");
        _spawnableTypes = serializedObject.FindProperty("_spawnableTypes");
        _pointSettings = serializedObject.FindProperty("_pointSettings");
    }

    static List<EnemyConfig> GetAllConfigs()
    {
        if (_cachedConfigs != null && EditorApplication.timeSinceStartup - _lastCacheTime < 2) return _cachedConfigs;
        _cachedConfigs = new List<EnemyConfig>();
        var guids = AssetDatabase.FindAssets("t:EnemyConfig");
        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var config = AssetDatabase.LoadAssetAtPath<EnemyConfig>(path);
            if (config != null) _cachedConfigs.Add(config);
        }
        _lastCacheTime = EditorApplication.timeSinceStartup;
        return _cachedConfigs;
    }

    public override void OnInspectorGUI()
    {
        if (_spawner == null) return;
        serializedObject.Update();
        EditorGUILayout.LabelField("Spawn Points (children)", EditorStyles.boldLabel);
        int n = _spawner.transform.childCount;
        EnemySpawnerSceneDrawer.HighlightedSpawner = null;
        EnemySpawnerSceneDrawer.HighlightedPointIndex = -1;
        if (n == 0)
        {
            EditorGUILayout.HelpBox("Add child GameObjects to use as spawn points.", MessageType.Info);
        }
        else
        {
            while (_pointSettings.arraySize < n)
            {
                _pointSettings.arraySize++;
                var elem = _pointSettings.GetArrayElementAtIndex(_pointSettings.arraySize - 1);
                elem.FindPropertyRelative("radius").floatValue = _radius.floatValue;
            }
            if (_pointSettings.arraySize > n) _pointSettings.arraySize = n;
            for (int i = 0; i < n; i++)
            {
                string name = _spawner.transform.GetChild(i).name;
                var elem = _pointSettings.GetArrayElementAtIndex(i);
                elem.isExpanded = EditorGUILayout.Foldout(elem.isExpanded, $"{i}. {name}", true);
                if (elem.isExpanded)
                {
                    EnemySpawnerSceneDrawer.HighlightedSpawner = _spawner;
                    EnemySpawnerSceneDrawer.HighlightedPointIndex = i;
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(elem.FindPropertyRelative("count"), new GUIContent("Count"));
                    EditorGUILayout.PropertyField(elem.FindPropertyRelative("radius"));
                    EditorGUILayout.PropertyField(elem.FindPropertyRelative("minDistanceBetween"), new GUIContent("Min distance between"));
                    EditorGUILayout.PropertyField(elem.FindPropertyRelative("maxDistanceBetween"), new GUIContent("Max distance between (0 = any)"));
                    DrawConfigToggles(elem.FindPropertyRelative("spawnableTypes"), "Enemy types");
                    EditorGUI.indentLevel--;
                }
            }
        }
        serializedObject.ApplyModifiedProperties();
        SceneView.RepaintAll();
    }

    void DrawConfigToggles(SerializedProperty listProp, string header)
    {
        var configs = GetAllConfigs();
        if (configs.Count == 0) { EditorGUILayout.HelpBox("Create EnemyConfig assets (Create → Enemy → Config).", MessageType.None); return; }
        EditorGUILayout.LabelField(header);
        var list = GetObjectRefs(listProp);
        EditorGUI.indentLevel++;
        for (int c = 0; c < configs.Count; c++)
        {
            bool on = list.Contains(configs[c]);
            bool newOn = EditorGUILayout.Toggle(configs[c].name, on);
            if (newOn != on)
            {
                if (newOn) { listProp.arraySize++; listProp.GetArrayElementAtIndex(listProp.arraySize - 1).objectReferenceValue = configs[c]; }
                else
                {
                    for (int j = listProp.arraySize - 1; j >= 0; j--)
                        if (listProp.GetArrayElementAtIndex(j).objectReferenceValue == configs[c])
                        { listProp.DeleteArrayElementAtIndex(j); break; }
                }
            }
        }
        EditorGUI.indentLevel--;
    }

    static HashSet<EnemyConfig> GetObjectRefs(SerializedProperty listProp)
    {
        var set = new HashSet<EnemyConfig>();
        if (listProp == null) return set;
        for (int i = 0; i < listProp.arraySize; i++)
        {
            var v = listProp.GetArrayElementAtIndex(i).objectReferenceValue as EnemyConfig;
            if (v != null) set.Add(v);
        }
        return set;
    }
}
