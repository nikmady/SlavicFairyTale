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
                var starts = area.SegmentStarts;
                int segCount = starts != null && starts.Count > 0 ? starts.Count : 1;
                Handles.color = new Color(0f, 1f, 0f, Selection.Contains(area.gameObject) ? 1f : 0.5f);
                for (int s = 0; s < segCount; s++)
                {
                    int start = starts != null && s < starts.Count ? starts[s] : 0;
                    int end = s + 1 < segCount && starts != null && s + 1 < starts.Count ? starts[s + 1] : area.Points.Count;
                    for (int i = start; i < end - 1; i++)
                        Handles.DrawLine(tr.TransformPoint(area.Points[i]), tr.TransformPoint(area.Points[i + 1]));
                    if (area.Loop && end - start >= 3)
                        Handles.DrawLine(tr.TransformPoint(area.Points[end - 1]), tr.TransformPoint(area.Points[start]));
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
