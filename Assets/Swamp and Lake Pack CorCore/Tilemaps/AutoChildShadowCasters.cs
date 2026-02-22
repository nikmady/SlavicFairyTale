using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Reflection;
using System.Collections.Generic;

[ExecuteAlways]
[RequireComponent(typeof(ShadowCaster2D))]
public class RootShadowCasterBuilder : MonoBehaviour
{
    void Start() => Build();

#if UNITY_EDITOR
    void OnValidate()
    {
        UnityEditor.EditorApplication.delayCall += Build;
    }
#endif

    void Build()
    {
        var colliders = GetComponentsInChildren<PolygonCollider2D>();

        List<Vector3> finalPath = new();

        foreach (var col in colliders)
        {
            foreach (var p in col.points)
            {
                Vector3 world = col.transform.TransformPoint(p);
                Vector3 local = transform.InverseTransformPoint(world);
                finalPath.Add(local);
            }
        }

        if (finalPath.Count < 3)
            return;

        var sc = GetComponent<ShadowCaster2D>();

        var t = typeof(ShadowCaster2D);

        t.GetField("m_ShapePath",
            BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(sc, finalPath.ToArray());

        t.GetField("m_ShapePathHash",
            BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(sc, Random.Range(int.MinValue, int.MaxValue));

        t.GetMethod("Awake",
            BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(sc, null);

        Debug.Log("Root shadow rebuilt");
    }
}