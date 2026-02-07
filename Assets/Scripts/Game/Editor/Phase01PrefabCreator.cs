using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using Game.Bootstrap;
using Game.Presentation.UI;

namespace Game.Editor
{
    public static class Phase01PrefabCreator
    {
        private const string PrefabsPath = "Assets/Prefabs";
        private const string ResourcesPath = "Assets/Resources";
        private const string GameRootPrefabName = "GameRoot";
        private const string CanvasPrefabPath = "Assets/Prefabs/UI/GameRootCanvas.prefab";

        [MenuItem("Game/Create Phase 1 Prefabs")]
        public static void CreateAll()
        {
            CreateGameRootPrefab();
            CreateGameRootCanvasPrefab();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Game] Prefabs created (Phases 1–6). Setup: PHASE_01_TESTING.md. Phase 6 check: PHASE_06_TESTING.md.");
        }

        private static void CreateGameRootPrefab()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            if (!AssetDatabase.IsValidFolder(ResourcesPath))
                AssetDatabase.CreateFolder("Assets", "Resources");

            GameObject root = new GameObject(GameRootPrefabName);
            root.AddComponent<GameRoot>();

            string prefabPath = $"{PrefabsPath}/{GameRootPrefabName}.prefab";
            string resourcesPath = $"{ResourcesPath}/{GameRootPrefabName}.prefab";
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            PrefabUtility.SaveAsPrefabAsset(root, resourcesPath);
            Object.DestroyImmediate(root);
        }

        private static void CreateGameRootCanvasPrefab()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs/UI"))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                    AssetDatabase.CreateFolder("Assets", "Prefabs");
                AssetDatabase.CreateFolder("Assets/Prefabs", "UI");
            }

            GameObject canvasGo = new GameObject("Canvas");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();

            GameObject panelGo = new GameObject("Panel");
            panelGo.transform.SetParent(canvasGo.transform, false);
            RectTransform panelRect = panelGo.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 1);
            panelRect.anchorMax = new Vector2(0, 1);
            panelRect.pivot = new Vector2(0, 1);
            panelRect.anchoredPosition = new Vector2(10, -10);
            panelRect.sizeDelta = new Vector2(280, 720);

            const float padding = 10f;
            const float spacing = 8f;
            const float textHeight = 24f;
            const float buttonHeight = 102.64f;
            const float width = 260f;
            float y = -padding;

            CreateText(panelGo.transform, "Title", "Game Debug Panel", 18, width, textHeight, ref y, spacing);
            CreateText(panelGo.transform, "ElapsedTime", "Elapsed: 0.00s", 14, width, textHeight, ref y, spacing);
            CreateText(panelGo.transform, "TickCount", "Ticks: 0", 14, width, textHeight, ref y, spacing);
            CreateText(panelGo.transform, "CurrentState", "Current State: Boot", 14, width, textHeight, ref y, spacing);
            CreateButton(panelGo.transform, "Reset Heartbeat", width, buttonHeight, ref y, spacing);
            CreateButton(panelGo.transform, "Print Status", width, buttonHeight, ref y, spacing);
            CreateButton(panelGo.transform, "Go Boot", width, buttonHeight, ref y, spacing);
            CreateButton(panelGo.transform, "Go Hub", width, buttonHeight, ref y, spacing);
            CreateButton(panelGo.transform, "Go WorldMap", width, buttonHeight, ref y, spacing);
            CreateButton(panelGo.transform, "Go LoadLocation", width, buttonHeight, ref y, spacing);
            CreateButton(panelGo.transform, "Go Run", width, buttonHeight, ref y, spacing);
            CreateButton(panelGo.transform, "Go RunEnd", width, buttonHeight, ref y, spacing);
            CreateText(panelGo.transform, "MetaSectionTitle", "META STATE", 16, width, textHeight, ref y, spacing);
            CreateText(panelGo.transform, "MetaPlayerLevel", "Player Level: 0", 14, width, textHeight, ref y, spacing);
            CreateText(panelGo.transform, "MetaTotalXP", "Total XP: 0", 14, width, textHeight, ref y, spacing);
            CreateText(panelGo.transform, "MetaSelectedClass", "Selected Class: -", 14, width, textHeight, ref y, spacing);
            CreateText(panelGo.transform, "MetaCurrencyGold", "Currency GOLD: 0", 14, width, textHeight, ref y, spacing);
            CreateButton(panelGo.transform, "Add XP (+100)", width, buttonHeight, ref y, spacing);
            CreateButton(panelGo.transform, "Add Gold (+50)", width, buttonHeight, ref y, spacing);
            CreateButton(panelGo.transform, "Save Meta", width, buttonHeight, ref y, spacing);
            CreateButton(panelGo.transform, "Reload Meta", width, buttonHeight, ref y, spacing);
            CreateButton(panelGo.transform, "Wipe Progress", width, buttonHeight, ref y, spacing);
            CreateText(panelGo.transform, "WorldMapSectionTitle", "WORLD MAP", 16, width, textHeight, ref y, spacing);
            GameObject worldMapContent = new GameObject("WorldMapContent");
            worldMapContent.transform.SetParent(panelGo.transform, false);
            RectTransform wmcRect = worldMapContent.AddComponent<RectTransform>();
            wmcRect.anchorMin = new Vector2(0, 1);
            wmcRect.anchorMax = new Vector2(0, 1);
            wmcRect.pivot = new Vector2(0.5f, 1f);
            wmcRect.anchoredPosition = new Vector2(width * 0.5f, y);
            wmcRect.sizeDelta = new Vector2(width, 400);
            y -= 400 + spacing;
            CreateButton(panelGo.transform, "Unlock Forest", width, buttonHeight, ref y, spacing);

            GameRootPanel panel = panelGo.AddComponent<GameRootPanel>();
            SerializedObject so = new SerializedObject(panel);
            Text[] texts = panelGo.GetComponentsInChildren<Text>(true);
            Button[] buttons = panelGo.GetComponentsInChildren<Button>(true);
            RectTransform[] rects = panelGo.GetComponentsInChildren<RectTransform>(true);
            RectTransform worldMapContentRect = null;
            foreach (RectTransform r in rects)
            {
                if (r.gameObject.name == "WorldMapContent") { worldMapContentRect = r; break; }
            }
            if (texts.Length >= 10)
            {
                so.FindProperty("_elapsedTimeText").objectReferenceValue = texts[1];
                so.FindProperty("_tickCountText").objectReferenceValue = texts[2];
                so.FindProperty("_currentStateText").objectReferenceValue = texts[3];
                so.FindProperty("_metaSectionTitleText").objectReferenceValue = texts[4];
                so.FindProperty("_metaPlayerLevelText").objectReferenceValue = texts[5];
                so.FindProperty("_metaTotalXPText").objectReferenceValue = texts[6];
                so.FindProperty("_metaSelectedClassText").objectReferenceValue = texts[7];
                so.FindProperty("_metaCurrencyGoldText").objectReferenceValue = texts[8];
                so.FindProperty("_worldMapSectionTitleText").objectReferenceValue = texts[9];
            }
            if (worldMapContentRect != null)
                so.FindProperty("_worldMapContent").objectReferenceValue = worldMapContentRect;
            if (buttons.Length >= 14)
            {
                so.FindProperty("_resetHeartbeatButton").objectReferenceValue = buttons[0];
                so.FindProperty("_printStatusButton").objectReferenceValue = buttons[1];
                so.FindProperty("_goBootButton").objectReferenceValue = buttons[2];
                so.FindProperty("_goHubButton").objectReferenceValue = buttons[3];
                so.FindProperty("_goWorldMapButton").objectReferenceValue = buttons[4];
                so.FindProperty("_goLoadLocationButton").objectReferenceValue = buttons[5];
                so.FindProperty("_goRunButton").objectReferenceValue = buttons[6];
                so.FindProperty("_goRunEndButton").objectReferenceValue = buttons[7];
                so.FindProperty("_metaAddXPButton").objectReferenceValue = buttons[8];
                so.FindProperty("_metaAddGoldButton").objectReferenceValue = buttons[9];
                so.FindProperty("_metaSaveButton").objectReferenceValue = buttons[10];
                so.FindProperty("_metaReloadButton").objectReferenceValue = buttons[11];
                so.FindProperty("_wipeProgressButton").objectReferenceValue = buttons[12];
                so.FindProperty("_worldMapUnlockForestButton").objectReferenceValue = buttons[13];
            }
            so.ApplyModifiedPropertiesWithoutUndo();

            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            eventSystem.transform.SetParent(canvasGo.transform);

            GameObject bootstrapGo = new GameObject("GameBootstrap");
            bootstrapGo.transform.SetParent(canvasGo.transform);
            bootstrapGo.AddComponent<GameRootInstaller>();

            PrefabUtility.SaveAsPrefabAsset(canvasGo, CanvasPrefabPath);
            Object.DestroyImmediate(canvasGo);
        }

        private static GameObject CreateText(Transform parent, string name, string content, int fontSize, float width, float height, ref float y, float spacing)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(width * 0.5f, y);
            rect.sizeDelta = new Vector2(width, height);
            y -= height + spacing;
            Text text = go.AddComponent<Text>();
            text.text = content;
            text.fontSize = fontSize;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return go;
        }

        private static GameObject CreateButton(Transform parent, string label, float width, float height, ref float y, float spacing)
        {
            GameObject go = new GameObject("Button_" + label.Replace(" ", ""));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(width * 0.5f, y);
            rect.sizeDelta = new Vector2(width, height);
            y -= height + spacing;
            Image image = go.AddComponent<Image>();
            image.color = new Color(0.2f, 0.5f, 0.8f);
            Button button = go.AddComponent<Button>();

            GameObject textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            RectTransform textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            Text text = textGo.AddComponent<Text>();
            text.text = label;
            text.fontSize = 14;
            text.alignment = TextAnchor.MiddleCenter;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.color = Color.white;

            return go;
        }
    }
}
