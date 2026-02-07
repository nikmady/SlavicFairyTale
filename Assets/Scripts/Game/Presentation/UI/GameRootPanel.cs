using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Game.Bootstrap;
using Game.Runtime.Services;
using Game.Runtime.Contexts;
using Game.Runtime.World;

namespace Game.Presentation.UI
{
    public class GameRootPanel : MonoBehaviour
    {
        [SerializeField] private GameRoot _gameRoot;
        [SerializeField] private Text _elapsedTimeText;
        [SerializeField] private Text _tickCountText;
        [SerializeField] private Text _currentStateText;
        [SerializeField] private Button _resetHeartbeatButton;
        [SerializeField] private Button _printStatusButton;
        [SerializeField] private Button _goBootButton;
        [SerializeField] private Button _goHubButton;
        [SerializeField] private Button _goWorldMapButton;
        [SerializeField] private Button _goLoadLocationButton;
        [SerializeField] private Button _goRunButton;
        [SerializeField] private Button _goRunEndButton;
        [SerializeField] private Text _metaSectionTitleText;
        [SerializeField] private Text _metaPlayerLevelText;
        [SerializeField] private Text _metaTotalXPText;
        [SerializeField] private Text _metaSelectedClassText;
        [SerializeField] private Text _metaCurrencyGoldText;
        [SerializeField] private Button _metaAddXPButton;
        [SerializeField] private Button _metaAddGoldButton;
        [SerializeField] private Button _metaSaveButton;
        [SerializeField] private Button _metaReloadButton;
        [SerializeField] private Button _wipeProgressButton;
        [SerializeField] private Text _worldMapSectionTitleText;
        [SerializeField] private RectTransform _worldMapContent;
        [SerializeField] private Button _worldMapUnlockForestButton;

        private const float WorldMapButtonHeight = 102.64f;
        private const float WorldMapButtonPadding = 8f;

        private bool _worldMapNeedsRefresh;
        private string _lastStateId = "";

        private void Awake()
        {
            if (_gameRoot == null)
                _gameRoot = FindObjectOfType<GameRoot>();

            if (_resetHeartbeatButton != null)
                _resetHeartbeatButton.onClick.AddListener(OnResetHeartbeat);

            if (_printStatusButton != null)
                _printStatusButton.onClick.AddListener(OnPrintStatus);

            if (_goBootButton != null)
                _goBootButton.onClick.AddListener(() => OnGoState("Boot"));
            if (_goHubButton != null)
                _goHubButton.onClick.AddListener(() => OnGoState("MetaHub"));
            if (_goWorldMapButton != null)
                _goWorldMapButton.onClick.AddListener(() => OnGoState("WorldMap"));
            if (_goLoadLocationButton != null)
                _goLoadLocationButton.onClick.AddListener(() => OnGoState("LoadLocation"));
            if (_goRunButton != null)
                _goRunButton.onClick.AddListener(() => OnGoState("RunLocation"));
            if (_goRunEndButton != null)
                _goRunEndButton.onClick.AddListener(() => OnGoState("RunEnd"));

            if (_metaAddXPButton != null)
                _metaAddXPButton.onClick.AddListener(OnMetaAddXP);
            if (_metaAddGoldButton != null)
                _metaAddGoldButton.onClick.AddListener(OnMetaAddGold);
            if (_metaSaveButton != null)
                _metaSaveButton.onClick.AddListener(OnMetaSave);
            if (_metaReloadButton != null)
                _metaReloadButton.onClick.AddListener(OnMetaReload);

            if (_wipeProgressButton != null)
                _wipeProgressButton.onClick.AddListener(OnWipeProgress);

            if (_worldMapUnlockForestButton != null)
                _worldMapUnlockForestButton.onClick.AddListener(OnWorldMapUnlockForest);
        }

        private void Start()
        {
            if (_goWorldMapButton != null && _goWorldMapButton.transform.parent is RectTransform parentRt)
                LayoutRebuilder.ForceRebuildLayoutImmediate(parentRt);
        }

        private void Update()
        {
            if (_gameRoot == null) return;

            if (_elapsedTimeText != null)
                _elapsedTimeText.text = $"Elapsed: {_gameRoot.ElapsedTime:F2}s";

            if (_tickCountText != null)
                _tickCountText.text = $"Ticks: {_gameRoot.TotalTicks}";

            if (_currentStateText != null)
                _currentStateText.text = $"Current State: {_gameRoot.CurrentStateId}";

            UpdateMetaTexts();
            UpdateStateButtonsInteractable();

            if (_gameRoot.CurrentStateId != _lastStateId)
            {
                if (_gameRoot.CurrentStateId == "WorldMap")
                    _worldMapNeedsRefresh = true;
                _lastStateId = _gameRoot.CurrentStateId;
            }
            if (_gameRoot.CurrentStateId == "WorldMap" && _gameRoot.CurrentWorldGraph != null && _worldMapNeedsRefresh)
            {
                RefreshWorldMap();
                _worldMapNeedsRefresh = false;
            }
        }

        private void UpdateMetaTexts()
        {
            MetaContext meta = _gameRoot.Meta;
            if (meta == null) return;

            if (_metaSectionTitleText != null)
                _metaSectionTitleText.text = "META STATE";

            if (_metaPlayerLevelText != null)
                _metaPlayerLevelText.text = $"Player Level: {meta.progression.playerLevel}";

            if (_metaTotalXPText != null)
                _metaTotalXPText.text = $"Total XP: {meta.progression.totalXP}";

            if (_metaSelectedClassText != null)
                _metaSelectedClassText.text = $"Selected Class: {(string.IsNullOrEmpty(meta.progression.selectedClassId) ? "-" : meta.progression.selectedClassId)}";

            if (_metaCurrencyGoldText != null)
                _metaCurrencyGoldText.text = $"Currency GOLD: {meta.economy.GetAmount("GOLD")}";
        }

        private void UpdateStateButtonsInteractable()
        {
            if (_gameRoot?.StateMachine == null) return;
            string current = _gameRoot.CurrentStateId;
            SetButtonInteractable(_goBootButton, current != "Boot");
            SetButtonInteractable(_goHubButton, current != "MetaHub");
            SetButtonInteractable(_goWorldMapButton, current != "WorldMap");
            SetButtonInteractable(_goLoadLocationButton, current != "LoadLocation");
            SetButtonInteractable(_goRunButton, current != "RunLocation");
            SetButtonInteractable(_goRunEndButton, current != "RunEnd");
        }

        private static void SetButtonInteractable(Button button, bool interactable)
        {
            if (button != null)
                button.interactable = interactable;
        }

        private void OnGoState(string stateId)
        {
            if (_gameRoot?.StateMachine != null)
                _gameRoot.StateMachine.SwitchState(stateId);
        }

        private void OnResetHeartbeat()
        {
            if (_gameRoot != null)
            {
                _gameRoot.ResetHeartbeat();
                Log.Info("Heartbeat reset.");
            }
        }

        private void OnPrintStatus()
        {
            if (_gameRoot != null)
                Log.Info(_gameRoot.GetHeartbeatStatus());
        }

        private void OnMetaAddXP()
        {
            if (_gameRoot?.Meta == null) return;
            _gameRoot.Meta.progression.AddXP(100);
        }

        private void OnMetaAddGold()
        {
            if (_gameRoot?.Meta == null) return;
            _gameRoot.Meta.economy.AddCurrency("GOLD", 50);
        }

        private void OnMetaSave()
        {
            if (_gameRoot?.Meta == null || _gameRoot.Save == null) return;
            _gameRoot.Save.SaveMeta(_gameRoot.Meta);
        }

        private void OnMetaReload()
        {
            if (_gameRoot == null) return;
            _gameRoot.ReloadMetaFromDisk();
        }

        public void RefreshWorldMap()
        {
            if (_gameRoot?.CurrentWorldGraph == null || _worldMapContent == null) return;
            for (int i = _worldMapContent.childCount - 1; i >= 0; i--)
                Destroy(_worldMapContent.GetChild(i).gameObject);

            if (_worldMapSectionTitleText != null)
                _worldMapSectionTitleText.text = "WORLD MAP";

            var graph = _gameRoot.CurrentWorldGraph;
            MetaContext meta = _gameRoot.Meta;
            string currentId = meta?.GetCurrentNodeId() ?? "";
            IReadOnlyList<WorldNodeData> nodes = graph.GetAllNodes();
            if (nodes == null) return;

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            float yOffset = 0f;
            for (int i = 0; i < nodes.Count; i++)
            {
                WorldNodeData node = nodes[i];
                if (node == null) continue;
                bool isUnlocked = meta != null && meta.HasNodeUnlocked(node.nodeId);
                bool isReachable = meta != null && graph.IsNodeReachable(meta.GetCurrentNodeId(), node.nodeId);
                bool isCurrent = node.nodeId == currentId;

                string label;
                bool interactable;
                if (!isUnlocked)
                {
                    label = "???";
                    interactable = false;
                }
                else if (!isReachable)
                {
                    label = node.displayName + " (locked path)";
                    interactable = false;
                }
                else
                {
                    label = node.displayName ?? node.nodeId;
                    interactable = true;
                }
                if (isCurrent)
                    label += " [CURRENT]";

                Button btn = CreateWorldMapNodeButton(_worldMapContent, label, interactable, font, yOffset);
                yOffset -= WorldMapButtonHeight + WorldMapButtonPadding;
                string nodeId = node.nodeId;
                btn.onClick.AddListener(() => OnSelectNode(nodeId));
            }
        }

        private static Button CreateWorldMapNodeButton(RectTransform parent, string label, bool interactable, Font font, float yOffset)
        {
            const float width = 260f;
            GameObject go = new GameObject("Node_" + label.Replace(" ", "_"));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(width * 0.5f, yOffset);
            rect.sizeDelta = new Vector2(width, WorldMapButtonHeight);
            Image img = go.AddComponent<Image>();
            img.color = interactable ? new Color(0.25f, 0.6f, 0.3f) : new Color(0.3f, 0.3f, 0.3f);
            Button btn = go.AddComponent<Button>();
            btn.interactable = interactable;

            GameObject textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            RectTransform textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            Text text = textGo.AddComponent<Text>();
            text.text = label;
            text.fontSize = 13;
            text.alignment = TextAnchor.MiddleCenter;
            text.font = font;
            text.color = Color.white;
            return btn;
        }

        public void OnSelectNode(string nodeId)
        {
            if (_gameRoot == null || string.IsNullOrEmpty(nodeId)) return;
            var graph = _gameRoot.CurrentWorldGraph;
            var meta = _gameRoot.Meta;
            if (graph == null || meta == null) return;
            if (!graph.IsNodeReachable(meta.GetCurrentNodeId(), nodeId))
            {
                Log.Warn($"Cannot select node '{nodeId}': not reachable from current node.");
                return;
            }
            meta.SetCurrentNode(nodeId);
            _gameRoot.SelectedWorldNodeId = nodeId;
            if (_gameRoot.StateMachine != null)
                _gameRoot.StateMachine.SwitchState("LoadLocation");
        }

        private void OnWorldMapUnlockForest()
        {
            if (_gameRoot?.Meta?.unlocks == null) return;
            _gameRoot.Meta.unlocks.UnlockNode("forest");
            _worldMapNeedsRefresh = true;
        }

        private void OnWipeProgress()
        {
            if (_gameRoot == null) return;
            _gameRoot.WipeProgress();
            _worldMapNeedsRefresh = true;
        }
    }
}
