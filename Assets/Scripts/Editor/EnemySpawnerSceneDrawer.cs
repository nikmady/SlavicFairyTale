using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class EnemySpawnerSceneDrawer
{
    public static EnemySpawner HighlightedSpawner;
    public static int HighlightedPointIndex = -1;

    static EnemySpawnerSceneDrawer()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    static void OnSceneGUI(SceneView view)
    {
        var spawners = Object.FindObjectsByType<EnemySpawner>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var spawner in spawners)
        {
            if (spawner == null || spawner.Equals(null)) continue;
            var so = new SerializedObject(spawner);
            float defaultR = so.FindProperty("_radius").floatValue;
            var pointSettings = so.FindProperty("_pointSettings");
            for (int i = 0; i < spawner.transform.childCount; i++)
            {
                float r = GetRadiusForPoint(pointSettings, defaultR, i);
                Vector3 pos = spawner.transform.GetChild(i).position;
                bool highlighted = HighlightedSpawner == spawner && HighlightedPointIndex == i;
                if (highlighted)
                {
                    Handles.color = new Color(0.2f, 1f, 0.2f, 0.5f);
                    Handles.DrawSolidDisc(pos, Vector3.forward, r);
                    Handles.color = new Color(0.2f, 1f, 0.2f, 1f);
                    Handles.DrawWireDisc(pos, Vector3.forward, r);
                }
                else
                {
                    bool selected = Selection.activeGameObject != null && (Selection.activeGameObject == spawner.gameObject || Selection.activeGameObject.transform.IsChildOf(spawner.transform));
                    Handles.color = new Color(1f, 0.3f, 0.3f, selected ? 0.6f : 0.35f);
                    Handles.DrawWireDisc(pos, Vector3.forward, r);
                }
            }
        }
    }

    static float GetRadiusForPoint(SerializedProperty pointSettings, float defaultR, int index)
    {
        if (pointSettings == null || index >= pointSettings.arraySize) return defaultR;
        var elem = pointSettings.GetArrayElementAtIndex(index);
        var r = elem.FindPropertyRelative("radius");
        return r != null ? r.floatValue : defaultR;
    }
}
