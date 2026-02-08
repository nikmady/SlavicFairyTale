using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class WalkableAreaSceneDrawer
{
    static WalkableAreaSceneDrawer()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    static void OnSceneGUI(SceneView view)
    {
        var areas = Object.FindObjectsByType<WalkableArea>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var area in areas)
        {
            Transform tr = area.transform;
            if (area.Points != null && area.Points.Count >= 2)
            {
                Handles.color = new Color(0f, 1f, 0f, Selection.Contains(area.gameObject) ? 1f : 0.5f);
                for (int i = 0; i < area.Points.Count; i++)
                {
                    Vector3 a = tr.TransformPoint(area.Points[i]);
                    int j = i + 1;
                    if (j >= area.Points.Count) j = area.Loop ? 0 : -1;
                    if (j >= 0) Handles.DrawLine(a, tr.TransformPoint(area.Points[j]));
                }
            }
            if (area.Obstacles != null)
                foreach (var obs in area.Obstacles)
                    if (obs.points != null && obs.points.Count >= 2)
                    {
                        Handles.color = new Color(1f, 0.3f, 0.3f, Selection.Contains(area.gameObject) ? 1f : 0.5f);
                        for (int i = 0; i < obs.points.Count; i++)
                        {
                            Vector3 a = tr.TransformPoint(obs.points[i]);
                            int j = (i + 1) % obs.points.Count;
                            Handles.DrawLine(a, tr.TransformPoint(obs.points[j]));
                        }
                    }
        }
    }
}
