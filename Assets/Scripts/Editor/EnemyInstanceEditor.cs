using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemyInstance))]
public class EnemyInstanceEditor : Editor
{
    void OnSceneGUI()
    {
        var instance = (EnemyInstance)target;
        if (instance == null || instance.Config == null) return;
        EnemyConfig config = instance.Config;
        Vector3 pos = instance.transform.position;
        float rAgg = config.aggressionRadius;
        float rAtk = config.attackRange;
        float rSep = config.separationRadius;
        // Красный — Aggression Radius
        Handles.color = new Color(1f, 0.2f, 0.2f, 0.35f);
        Handles.DrawSolidDisc(pos, Vector3.forward, rAgg);
        Handles.color = new Color(1f, 0.2f, 0.2f, 1f);
        Handles.DrawWireDisc(pos, Vector3.forward, rAgg);
        // Сиреневый поверх — Attack Range
        Handles.color = new Color(0.7f, 0.3f, 1f, 0.4f);
        Handles.DrawSolidDisc(pos, Vector3.forward, rAtk);
        Handles.color = new Color(0.7f, 0.3f, 1f, 1f);
        Handles.DrawWireDisc(pos, Vector3.forward, rAtk);
        // Жёлтый поверх — Separation Radius (реалтайм в сцене)
        if (rSep > 0f)
        {
            Handles.color = new Color(1f, 0.9f, 0.2f, 0.35f);
            Handles.DrawSolidDisc(pos, Vector3.forward, rSep);
            Handles.color = new Color(1f, 0.9f, 0.2f, 1f);
            Handles.DrawWireDisc(pos, Vector3.forward, rSep);
        }
        EditorGUI.BeginChangeCheck();
        float newRAgg = Handles.RadiusHandle(Quaternion.identity, pos, rAgg);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(config, "Aggression Radius");
            config.aggressionRadius = Mathf.Max(0f, newRAgg);
            EditorUtility.SetDirty(config);
        }
        if (rSep > 0f)
        {
            EditorGUI.BeginChangeCheck();
            float newRSep = Handles.RadiusHandle(Quaternion.identity, pos, rSep);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(config, "Separation Radius");
                config.separationRadius = Mathf.Max(0f, newRSep);
                EditorUtility.SetDirty(config);
            }
        }
    }
}
