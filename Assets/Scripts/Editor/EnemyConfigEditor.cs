using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemyConfig))]
public class EnemyConfigEditor : Editor
{
    const int PreviewSize = 128;
    /// <summary> Во сколько раз отдалить камеру: больше = персонаж мельче в кадре. </summary>
    const float CameraZoomOutFactor = 4f;
    static Texture2D _circleTex;
    static Texture2D _circleTexPurple;
    static Texture2D _circleTexSeparation;
    static RenderTexture _previewRT;
    static int _cachedPreviewPrefabId;
    static GameObject _previewInstance;

    static float GetPrefabBoundsSize(GameObject prefab)
    {
        if (prefab == null) return 2f;
        GameObject tmp = null;
        try
        {
            tmp = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            tmp.SetActive(false);
            var renderers = tmp.GetComponentsInChildren<Renderer>(true);
            Bounds b = new Bounds(tmp.transform.position, Vector3.zero);
            foreach (var r in renderers)
            {
                if (r.bounds.size.sqrMagnitude <= 0.0001f) continue;
                if (b.size.sqrMagnitude < 0.0001f)
                    b = r.bounds;
                else
                    b.Encapsulate(r.bounds);
            }
            float size = b.size.magnitude * 0.5f;
            return size > 0.01f ? size : 2f;
        }
        finally
        {
            if (tmp != null) DestroyImmediate(tmp);
        }
    }

    static Texture2D GetCircleTex()
    {
        if (_circleTex != null) return _circleTex;
        int s = 256;
        _circleTex = new Texture2D(s, s);
        float c = s * 0.5f;
        for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float d = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c));
                float a = d <= c - 0.5f ? 0.4f : (d <= c + 0.5f ? 0.8f : 0f);
                _circleTex.SetPixel(x, y, new Color(1f, 0.2f, 0.2f, a));
            }
        _circleTex.Apply();
        return _circleTex;
    }

    static Texture2D GetCircleTexPurple()
    {
        if (_circleTexPurple != null) return _circleTexPurple;
        int s = 256;
        _circleTexPurple = new Texture2D(s, s);
        float c = s * 0.5f;
        Color purple = new Color(0.7f, 0.3f, 1f);
        for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float d = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c));
                float a = d <= c - 0.5f ? 0.4f : (d <= c + 0.5f ? 0.8f : 0f);
                _circleTexPurple.SetPixel(x, y, new Color(purple.r, purple.g, purple.b, a));
            }
        _circleTexPurple.Apply();
        return _circleTexPurple;
    }

    static Texture2D GetCircleTexSeparation()
    {
        if (_circleTexSeparation != null) return _circleTexSeparation;
        int s = 256;
        _circleTexSeparation = new Texture2D(s, s);
        float c = s * 0.5f;
        Color yellow = new Color(1f, 0.9f, 0.2f);
        for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float d = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c));
                float a = d <= c - 0.5f ? 0.35f : (d <= c + 0.5f ? 0.7f : 0f);
                _circleTexSeparation.SetPixel(x, y, new Color(yellow.r, yellow.g, yellow.b, a));
            }
        _circleTexSeparation.Apply();
        return _circleTexSeparation;
    }

    /// <summary> Рендер префаба в RT своей камерой с отдалённым orthographic size (персонаж мельче). </summary>
    static Texture GetPrefabPreviewWithCamera(GameObject prefab)
    {
        if (prefab == null) return null;
        int id = prefab.GetInstanceID();
        if (_previewRT != null && _cachedPreviewPrefabId == id) return _previewRT;

        CleanupPreview();
        float prefabSize = GetPrefabBoundsSize(prefab);
        if (prefabSize < 0.01f) return null;

        _previewRT = new RenderTexture(PreviewSize, PreviewSize, 16);
        _previewRT.Create();
        _previewInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        if (_previewInstance == null) { _previewRT.Release(); _previewRT = null; return null; }
        _previewInstance.SetActive(true);
        _previewInstance.transform.position = Vector3.zero;
        _previewInstance.transform.rotation = Quaternion.identity;
        _previewInstance.transform.localScale = Vector3.one;

        var camGo = new GameObject("EnemyConfigPreviewCam");
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = prefabSize * CameraZoomOutFactor;
        cam.transform.position = new Vector3(0f, 0f, -10f);
        cam.transform.rotation = Quaternion.identity;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.22f, 0.22f, 0.22f);
        cam.targetTexture = _previewRT;
        cam.Render();
        cam.targetTexture = null;

        DestroyImmediate(camGo);
        DestroyImmediate(_previewInstance);
        _previewInstance = null;
        _cachedPreviewPrefabId = id;
        return _previewRT;
    }

    static void CleanupPreview()
    {
        if (_previewInstance != null) { DestroyImmediate(_previewInstance); _previewInstance = null; }
        if (_previewRT != null) { _previewRT.Release(); _previewRT = null; }
        _cachedPreviewPrefabId = 0;
    }

    void OnDisable() => CleanupPreview();

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        SerializedProperty prefabProp = serializedObject.FindProperty("prefab");
        SerializedProperty aggRadiusProp = serializedObject.FindProperty("aggressionRadius");
        SerializedProperty attackRangeProp = serializedObject.FindProperty("attackRange");
        SerializedProperty separationRadiusProp = serializedObject.FindProperty("separationRadius");

        DrawProperty(prefabProp);
        DrawHeader("Stats");
        DrawProperty(serializedObject.FindProperty("displayName"));
        DrawProperty(serializedObject.FindProperty("maxHp"));
        DrawProperty(serializedObject.FindProperty("maxMana"));

        DrawHeader("AI");

        Rect previewRect = GUILayoutUtility.GetRect(PreviewSize, PreviewSize);
        GameObject prefab = prefabProp.objectReferenceValue as GameObject;
        Texture previewTex = prefab != null ? GetPrefabPreviewWithCamera(prefab) : null;
        if (previewTex != null)
        {
            GUI.DrawTexture(previewRect, previewTex, ScaleMode.ScaleToFit);
            float prefabSize = GetPrefabBoundsSize(prefab);
            float orthoSize = Mathf.Max(0.01f, prefabSize * CameraZoomOutFactor);
            float pxPerUnit = PreviewSize / (2f * orthoSize);
            float fitScale = Mathf.Min(previewRect.width, previewRect.height) / (float)PreviewSize;
            // Красный круг — Aggression Radius
            float rAgg = aggRadiusProp.floatValue;
            float rAggPx = Mathf.Max(4f, rAgg * pxPerUnit * fitScale);
            Rect rectAgg = new Rect(previewRect.center.x - rAggPx, previewRect.center.y - rAggPx, rAggPx * 2f, rAggPx * 2f);
            GUI.DrawTexture(rectAgg, GetCircleTex(), ScaleMode.ScaleToFit);
            // Сиреневый круг поверх — Attack Range
            float rAtk = attackRangeProp.floatValue;
            float rAtkPx = Mathf.Max(4f, rAtk * pxPerUnit * fitScale);
            Rect rectAtk = new Rect(previewRect.center.x - rAtkPx, previewRect.center.y - rAtkPx, rAtkPx * 2f, rAtkPx * 2f);
            GUI.DrawTexture(rectAtk, GetCircleTexPurple(), ScaleMode.ScaleToFit);
            // Жёлтый круг поверх — Separation Radius (реалтайм в превью)
            float rSep = separationRadiusProp.floatValue;
            if (rSep > 0f)
            {
                float rSepPx = Mathf.Max(4f, rSep * pxPerUnit * fitScale);
                Rect rectSep = new Rect(previewRect.center.x - rSepPx, previewRect.center.y - rSepPx, rSepPx * 2f, rSepPx * 2f);
                GUI.DrawTexture(rectSep, GetCircleTexSeparation(), ScaleMode.ScaleToFit);
            }
        }
        else
        {
            EditorGUI.DrawRect(previewRect, new Color(0.2f, 0.2f, 0.2f));
            var style = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter };
            EditorGUI.LabelField(previewRect, "Assign prefab\nfor preview", style);
        }

        EditorGUILayout.PropertyField(aggRadiusProp, new GUIContent("Aggression Radius"));
        DrawProperty(serializedObject.FindProperty("moveSpeed"));
        DrawProperty(serializedObject.FindProperty("runSpeed"));
        DrawProperty(serializedObject.FindProperty("attackRange"));
        DrawProperty(serializedObject.FindProperty("attackDamage"));
        DrawProperty(serializedObject.FindProperty("attackInterval"));
        EditorGUILayout.PropertyField(separationRadiusProp, new GUIContent("Separation Radius"));
        DrawProperty(serializedObject.FindProperty("separationStrength"));
        DrawProperty(serializedObject.FindProperty("separationEnemyLayer"));
        DrawProperty(serializedObject.FindProperty("restDuration"));
        DrawProperty(serializedObject.FindProperty("restChance"));

        DrawHeader("Reward (on kill)");
        DrawProperty(serializedObject.FindProperty("money"));
        DrawProperty(serializedObject.FindProperty("exp"));
        DrawProperty(serializedObject.FindProperty("deathAnimationDelay"));

        serializedObject.ApplyModifiedProperties();
    }

    void DrawHeader(string label)
    {
        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
    }

    void DrawProperty(SerializedProperty p)
    {
        if (p != null) EditorGUILayout.PropertyField(p);
    }
}
