using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using UnityEngine.Rendering;
using System.Collections.Generic;

public class IsometricLayeredGeneratorWindow : EditorWindow
{
    [System.Serializable]
    public class PropData
    {
        public Sprite sprite;
        [Range(0, 100)]
        public float spawnPercent = 5f;
        public bool useSpacing = false;
        [Range(1, 20)]
        public int spacingRadius = 3;
        public bool randomFlipX = false;
    }

    [System.Serializable]
    public class LayerData
    {
        public string layerName = "Layer";
        public RuleTile ruleTile;
        public Tilemap targetTilemap;

        public bool isWater = false;
        public float fillPercent = 100f;

        public int riverCount = 1;
        [Range(1, 3)]
        public int pathWidth = 2;
        [Range(0, 10)]
        public int bridgesPerRiver = 1;
        [Range(2, 50)]
        public int maxBridgeLength = 12;
        public Sprite bridgeSpriteHorizontal;
        public Sprite bridgeSpriteVertical;
        [Range(0f, 3f)]
        public float bridgeScale = 1f;

        public bool propsFoldout = false;
        [Range(0, 10)]
        public int propEdgePadding = 1;
        public List<PropData> props = new List<PropData>();
    }

    [System.Serializable]
    private class SerializablePropData
    {
        public string spriteId;
        public float spawnPercent;
        public bool useSpacing;
        public int spacingRadius;
        public bool randomFlipX;
    }

    [System.Serializable]
    private class SerializableLayerData
    {
        public string layerName;
        public string ruleTileId;
        public string tilemapId;
        public bool isWater;
        public float fillPercent;
        public int riverCount;
        public int pathWidth;
        public int bridgesPerRiver;
        public int maxBridgeLength;
        public string bridgeSpriteHorizontalId;
        public string bridgeSpriteVerticalId;
        public float bridgeScale;
        public bool propsFoldout;
        public int propEdgePadding;
        public List<SerializablePropData> props = new List<SerializablePropData>();
    }

    [System.Serializable]
    private class SaveData
    {
        public int width = 40;
        public int height = 40;
        public bool debugRivers = true;
        public bool debugLayers = false;
        public bool debugBridges = false;
        public List<SerializableLayerData> layers = new List<SerializableLayerData>();
    }

    private const string SAVE_KEY = "IsoLayeredGen_Save";

    private List<LayerData> layers = new List<LayerData>();

    private int width = 40;
    private int height = 40;

    private bool debugRivers = true;
    private bool debugLayers = false;
    private bool debugBridges = false;

    private static readonly Color[] debugLayerColors = new Color[]
    {
        new Color(1f, 0.25f, 0.25f, 0.35f),
        new Color(0.25f, 0.5f, 1f, 0.35f),
        new Color(0.25f, 1f, 0.35f, 0.35f),
        new Color(1f, 1f, 0.2f, 0.35f),
        new Color(1f, 0.25f, 1f, 0.35f),
        new Color(0.2f, 1f, 1f, 0.35f),
        new Color(1f, 0.6f, 0.2f, 0.35f),
        new Color(0.6f, 0.2f, 1f, 0.35f),
    };

    private List<List<Vector3>> debugPaths = new List<List<Vector3>>();
    private List<HashSet<Vector2Int>> debugLayerCells = new List<HashSet<Vector2Int>>();
    private List<List<List<Vector2Int>>> riverPathCellsByLayer = new List<List<List<Vector2Int>>>();
    private List<(int layerIndex, List<Vector2Int> cells)> debugBridgesData = new List<(int, List<Vector2Int>)>();
    private Transform propsRoot;
    private Transform bridgesRoot;
    private Vector2 scrollPos;

    private static readonly Vector2Int[] Dir8 =
    {
        new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(0, 1), new Vector2Int(-1, 1),
        new Vector2Int(-1, 0), new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1)
    };

    [MenuItem("Tools/Isometric Layered Generator")]
    public static void Open()
    {
        GetWindow<IsometricLayeredGeneratorWindow>("Iso Generator");
    }

    private void OnEnable()
    {
        LoadSettings();
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SaveSettings();
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        EditorGUI.BeginChangeCheck();

        GUILayout.Label("Map Size", EditorStyles.boldLabel);
        width = EditorGUILayout.IntField("Width", width);
        height = EditorGUILayout.IntField("Height", height);

        GUILayout.Space(6);

        debugRivers = EditorGUILayout.Toggle("Show Paths", debugRivers);
        debugLayers = EditorGUILayout.Toggle("Debug Layers", debugLayers);
        debugBridges = EditorGUILayout.Toggle("Bridges", debugBridges);

        GUILayout.Space(10);

        bool needsSave = false;

        if (GUILayout.Button("Add Layer"))
        {
            layers.Add(new LayerData());
            needsSave = true;
        }

        for (int li = 0; li < layers.Count; li++)
        {
            var layer = layers[li];
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();
            layer.layerName = EditorGUILayout.TextField("Layer Name", layer.layerName);
            if (GUILayout.Button("X", GUILayout.Width(22)))
            {
                layers.RemoveAt(li);
                li--;
                needsSave = true;
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                continue;
            }
            EditorGUILayout.EndHorizontal();

            layer.ruleTile = (RuleTile)EditorGUILayout.ObjectField("Rule Tile", layer.ruleTile, typeof(RuleTile), false);
            layer.targetTilemap = (Tilemap)EditorGUILayout.ObjectField("Target Tilemap", layer.targetTilemap, typeof(Tilemap), true);

            layer.isWater = EditorGUILayout.Toggle("Water Mode", layer.isWater);

            if (!layer.isWater)
                layer.fillPercent = EditorGUILayout.Slider("Fill %", layer.fillPercent, 0, 100);
            else
            {
                layer.riverCount = EditorGUILayout.IntField("Path Count", layer.riverCount);
                layer.pathWidth = EditorGUILayout.IntSlider("Path Width", layer.pathWidth, 1, 3);
                layer.bridgesPerRiver = EditorGUILayout.IntSlider("Bridges Per River", layer.bridgesPerRiver, 0, 10);
                layer.maxBridgeLength = EditorGUILayout.IntSlider("Max Bridge Length", layer.maxBridgeLength, 2, 50);
                layer.bridgeSpriteHorizontal = (Sprite)EditorGUILayout.ObjectField("Bridge Sprite (Horizontal)", layer.bridgeSpriteHorizontal, typeof(Sprite), false);
                layer.bridgeSpriteVertical = (Sprite)EditorGUILayout.ObjectField("Bridge Sprite (Vertical)", layer.bridgeSpriteVertical, typeof(Sprite), false);
                float rawScale = EditorGUILayout.Slider("Bridge Scale", layer.bridgeScale, 0f, 3f);
                layer.bridgeScale = Mathf.Clamp(Mathf.Round(rawScale * 2f) / 2f, 0f, 3f);
            }

            layer.propsFoldout = EditorGUILayout.Foldout(layer.propsFoldout, "Props (" + layer.props.Count + ")", true);
            if (layer.propsFoldout)
            {
                EditorGUI.indentLevel++;
                layer.propEdgePadding = EditorGUILayout.IntSlider("Edge Padding", layer.propEdgePadding, 0, 10);
                for (int pi = 0; pi < layer.props.Count; pi++)
                {
                    var prop = layer.props[pi];
                    EditorGUILayout.BeginVertical("helpbox");

                    EditorGUILayout.BeginHorizontal();
                    var preview = prop.sprite != null ? AssetPreview.GetAssetPreview(prop.sprite) : null;
                    if (preview != null)
                    {
                        GUILayout.Label(preview, GUILayout.Width(48), GUILayout.Height(48));
                    }
                    else
                    {
                        GUILayout.Label("No Sprite", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(48), GUILayout.Height(48));
                    }
                    EditorGUILayout.BeginVertical();
                    prop.sprite = (Sprite)EditorGUILayout.ObjectField(prop.sprite, typeof(Sprite), false);
                    prop.spawnPercent = EditorGUILayout.Slider("Spawn %", prop.spawnPercent, 0, 100);
                    prop.useSpacing = EditorGUILayout.Toggle("Spacing", prop.useSpacing);
                    if (prop.useSpacing)
                        prop.spacingRadius = EditorGUILayout.IntSlider("Min Distance", prop.spacingRadius, 1, 20);
                    prop.randomFlipX = EditorGUILayout.Toggle("Random Flip X", prop.randomFlipX);
                    EditorGUILayout.EndVertical();
                    if (GUILayout.Button("X", GUILayout.Width(22), GUILayout.Height(48)))
                    {
                        layer.props.RemoveAt(pi);
                        pi--;
                        needsSave = true;
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                        continue;
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.EndVertical();
                }
                if (GUILayout.Button("+ Add Prop"))
                {
                    layer.props.Add(new PropData());
                    needsSave = true;
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
        }

        if (EditorGUI.EndChangeCheck() || needsSave)
            SaveSettings();

        GUILayout.Space(6);

        if (GUILayout.Button("Generate"))
            Generate();

        if (GUILayout.Button("Delete Generated"))
            DeleteGenerated();

        EditorGUILayout.EndScrollView();
    }

    private void Generate()
    {
        debugPaths.Clear();
        debugLayerCells.Clear();
        debugBridgesData.Clear();
        riverPathCellsByLayer.Clear();
        for (int i = 0; i < layers.Count; i++)
            riverPathCellsByLayer.Add(new List<List<Vector2Int>>());

        foreach (var layer in layers)
        {
            if (layer.targetTilemap == null) continue;
            layer.targetTilemap.ClearAllTiles();
        }

        for (int li = 0; li < layers.Count; li++)
        {
            var layer = layers[li];
            var cells = new HashSet<Vector2Int>();

            if (layer.ruleTile == null || layer.targetTilemap == null)
            {
                debugLayerCells.Add(cells);
                continue;
            }

            if (!layer.isWater)
                GenerateFill(layer, cells);
            else
                GenerateRivers(layer, li, cells);

            debugLayerCells.Add(cells);
        }

        ComputeBridges();

        foreach (var layer in layers)
        {
            if (layer.targetTilemap == null) continue;
            layer.targetTilemap.RefreshAllTiles();
        }

        DestroyProps();
        DestroyBridges();
        bridgesRoot = new GameObject("__GeneratedBridges__").transform;
        Undo.RegisterCreatedObjectUndo(bridgesRoot.gameObject, "Generate Bridges");
        SpawnBridgeSprites();

        propsRoot = new GameObject("__GeneratedProps__").transform;
        Undo.RegisterCreatedObjectUndo(propsRoot.gameObject, "Generate Props");

        var occupiedCells = new HashSet<Vector2Int>();
        for (int li = 0; li < layers.Count; li++)
        {
            var layer = layers[li];
            if (layer.targetTilemap == null || layer.props.Count == 0) continue;
            SpawnProps(layer, li, occupiedCells);
        }

        SceneView.RepaintAll();
    }

    private void SpawnProps(LayerData layer, int layerIndex, HashSet<Vector2Int> occupiedCells)
    {
        var tm = layer.targetTilemap;
        var tmr = tm.GetComponent<TilemapRenderer>();
        int baseSortOrder = tmr != null ? tmr.sortingOrder + 1 : 1;
        string sortLayer = tmr != null ? tmr.sortingLayerName : "Default";

        var cells = ErodeCells(debugLayerCells[layerIndex], layer.propEdgePadding);
        int exclusionRadius = Mathf.Max(layer.propEdgePadding, 1);
        cells = ExcludeNearOtherLayers(cells, layerIndex, exclusionRadius);

        foreach (var prop in layer.props)
        {
            if (prop.sprite == null) continue;

            HashSet<Vector2Int> spacingBlocked = prop.useSpacing ? new HashSet<Vector2Int>() : null;

            foreach (var cell in cells)
            {
                if (occupiedCells.Contains(cell)) continue;
                if (spacingBlocked != null && spacingBlocked.Contains(cell)) continue;
                if (Random.value * 100 > prop.spawnPercent) continue;

                occupiedCells.Add(cell);

                if (spacingBlocked != null)
                {
                    int r = prop.spacingRadius;
                    for (int dx = -r; dx <= r; dx++)
                        for (int dy = -r; dy <= r; dy++)
                            if (dx * dx + dy * dy <= r * r)
                                spacingBlocked.Add(cell + new Vector2Int(dx, dy));
                }

                var worldPos = tm.GetCellCenterWorld(new Vector3Int(cell.x, cell.y, 0));
                var go = new GameObject(prop.sprite.name);
                go.transform.SetParent(propsRoot);
                go.transform.position = worldPos;

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = prop.sprite;
                sr.sortingLayerName = sortLayer;
                sr.sortingOrder = baseSortOrder + (width + height) - (cell.x + cell.y);
                if (prop.randomFlipX)
                    sr.flipX = Random.value > 0.5f;
            }
        }
    }

    private HashSet<Vector2Int> ErodeCells(HashSet<Vector2Int> source, int padding)
    {
        if (padding <= 0) return source;

        var current = new HashSet<Vector2Int>(source);
        for (int step = 0; step < padding; step++)
        {
            var next = new HashSet<Vector2Int>();
            foreach (var cell in current)
            {
                bool isEdge = false;
                foreach (var dir in Dir8)
                {
                    if (!current.Contains(cell + dir))
                    {
                        isEdge = true;
                        break;
                    }
                }
                if (!isEdge)
                    next.Add(cell);
            }
            current = next;
        }
        return current;
    }

    private HashSet<Vector2Int> ExcludeNearOtherLayers(HashSet<Vector2Int> cells, int layerIndex, int radius)
    {
        if (radius <= 0 || debugLayerCells.Count <= 1) return cells;

        var otherCells = new HashSet<Vector2Int>();
        for (int i = 0; i < debugLayerCells.Count; i++)
        {
            if (i == layerIndex) continue;
            otherCells.UnionWith(debugLayerCells[i]);
        }

        if (otherCells.Count == 0) return cells;

        var result = new HashSet<Vector2Int>();
        foreach (var cell in cells)
        {
            bool tooClose = false;
            for (int dx = -radius; dx <= radius && !tooClose; dx++)
            {
                for (int dy = -radius; dy <= radius && !tooClose; dy++)
                {
                    if (otherCells.Contains(cell + new Vector2Int(dx, dy)))
                        tooClose = true;
                }
            }
            if (!tooClose)
                result.Add(cell);
        }
        return result;
    }

    private void ComputeBridges()
    {
        for (int li = 0; li < layers.Count; li++)
        {
            var layer = layers[li];
            if (!layer.isWater || layer.bridgesPerRiver <= 0) continue;
            if (li >= riverPathCellsByLayer.Count) continue;

            var riverCells = debugLayerCells[li];
            var paths = riverPathCellsByLayer[li];
            var placedBridgeCells = new HashSet<Vector2Int>();
            const int minBridgeDistance = 4;

            foreach (var path in paths)
            {
                if (path.Count < 3) continue;

                int n = Mathf.Min(layer.bridgesPerRiver, path.Count - 2);
                int maxLen = layer.maxBridgeLength;

                for (int b = 0; b < n; b++)
                {
                    int startIndex = 1 + (b + 1) * (path.Count - 2) / (n + 1);
                    bool placed = false;

                    for (int k = 0; k < path.Count - 2 && !placed; k++)
                    {
                        int index = 1 + (startIndex - 1 + k) % (path.Count - 2);
                        Vector2Int cell = path[index];

                        int dx = path[index + 1].x - path[index - 1].x;
                        int dy = path[index + 1].y - path[index - 1].y;

                        Vector2Int perp1, perp2;
                        if (Mathf.Abs(dx) >= Mathf.Abs(dy))
                        {
                            perp1 = new Vector2Int(0, 1);
                            perp2 = new Vector2Int(0, -1);
                        }
                        else
                        {
                            perp1 = new Vector2Int(1, 0);
                            perp2 = new Vector2Int(-1, 0);
                        }

                        Vector2Int shore1 = FindShore(cell, perp1, riverCells);
                        Vector2Int shore2 = FindShore(cell, perp2, riverCells);
                        var fullLine = LineCellsStraight(shore1, shore2);
                        var bridgeCells = new List<Vector2Int>();
                        foreach (var c in fullLine)
                            if (riverCells.Contains(c))
                                bridgeCells.Add(c);
                        if (bridgeCells.Count > 0 && bridgeCells.Count <= maxLen &&
                            !BridgeTooCloseToPlaced(bridgeCells, placedBridgeCells, minBridgeDistance))
                        {
                            debugBridgesData.Add((li, bridgeCells));
                            foreach (var c in bridgeCells)
                                placedBridgeCells.Add(c);
                            placed = true;
                        }
                    }
                }
            }
        }
    }

    private bool BridgeTooCloseToPlaced(List<Vector2Int> bridgeCells, HashSet<Vector2Int> placedCells, int minDistance)
    {
        int radius = minDistance - 1;
        foreach (var c in bridgeCells)
        {
            for (int dx = -radius; dx <= radius; dx++)
                for (int dy = -radius; dy <= radius; dy++)
                    if (placedCells.Contains(c + new Vector2Int(dx, dy)))
                        return true;
        }
        return false;
    }

    private Vector2Int FindShore(Vector2Int from, Vector2Int step, HashSet<Vector2Int> riverCells)
    {
        Vector2Int p = from;
        for (int i = 0; i < width + height; i++)
        {
            p += step;
            if (!Inside(p))
                return Inside(p - step) ? p - step : from;
            if (!riverCells.Contains(p)) return p;
        }
        return from;
    }

    private List<Vector2Int> LineCellsStraight(Vector2Int a, Vector2Int b)
    {
        var list = new List<Vector2Int>();
        if (a.x == b.x)
        {
            int yMin = Mathf.Min(a.y, b.y), yMax = Mathf.Max(a.y, b.y);
            for (int y = yMin; y <= yMax; y++)
            {
                var p = new Vector2Int(a.x, y);
                if (Inside(p)) list.Add(p);
            }
        }
        else if (a.y == b.y)
        {
            int xMin = Mathf.Min(a.x, b.x), xMax = Mathf.Max(a.x, b.x);
            for (int x = xMin; x <= xMax; x++)
            {
                var p = new Vector2Int(x, a.y);
                if (Inside(p)) list.Add(p);
            }
        }
        return list;
    }

    private void SpawnBridgeSprites()
    {
        for (int i = 0; i < debugBridgesData.Count; i++)
        {
            var (li, cells) = debugBridgesData[i];
            if (li < 0 || li >= layers.Count || cells.Count == 0) continue;
            var layer = layers[li];
            var tm = layer.targetTilemap;
            if (tm == null) continue;

            bool isVertical = cells[0].x == cells[cells.Count - 1].x;
            Sprite sprite = isVertical ? layer.bridgeSpriteVertical : layer.bridgeSpriteHorizontal;
            if (sprite == null) continue;

            var tmr = tm.GetComponent<TilemapRenderer>();
            int sortOrder = tmr != null ? tmr.sortingOrder + 1 : 1;
            string sortLayerName = tmr != null ? tmr.sortingLayerName : "Default";
            float scale = layer.bridgeScale <= 0.001f ? 1f : layer.bridgeScale;
            int segmentsPerCell = Mathf.Max(1, Mathf.RoundToInt(1f / scale));

            Vector3 dirWorld;
            float cellLength;
            if (cells.Count >= 2)
            {
                Vector3 firstCenter = tm.GetCellCenterWorld(new Vector3Int(cells[0].x, cells[0].y, 0));
                Vector3 lastCenter = tm.GetCellCenterWorld(new Vector3Int(cells[cells.Count - 1].x, cells[cells.Count - 1].y, 0));
                dirWorld = (lastCenter - firstCenter).normalized;
                cellLength = (lastCenter - firstCenter).magnitude / (cells.Count - 1);
            }
            else
            {
                dirWorld = Vector3.right;
                cellLength = tm.cellSize.x;
            }

            float step = cellLength / segmentsPerCell;

            foreach (var cell in cells)
            {
                Vector3 cellCenter = tm.GetCellCenterWorld(new Vector3Int(cell.x, cell.y, 0));
                int baseOrder = sortOrder + (width + height) - (cell.x + cell.y);

                for (int s = 0; s < segmentsPerCell; s++)
                {
                    float t = (s - (segmentsPerCell - 1) * 0.5f) * step;
                    Vector3 pos = cellCenter + dirWorld * t;

                    var go = new GameObject("Bridge");
                    go.transform.SetParent(bridgesRoot);
                    go.transform.position = pos;
                    go.transform.localScale = new Vector3(scale, scale, 1f);

                    var sr = go.AddComponent<SpriteRenderer>();
                    sr.sprite = sprite;
                    sr.sortingLayerName = sortLayerName;
                    sr.sortingOrder = baseOrder;
                }
            }
        }
    }

    private void DestroyBridges()
    {
        if (bridgesRoot != null)
            DestroyImmediate(bridgesRoot.gameObject);
        var old = GameObject.Find("__GeneratedBridges__");
        if (old != null)
            DestroyImmediate(old);
    }

    private void DestroyProps()
    {
        if (propsRoot != null)
            DestroyImmediate(propsRoot.gameObject);

        var old = GameObject.Find("__GeneratedProps__");
        if (old != null)
            DestroyImmediate(old);
    }

    private void GenerateFill(LayerData layer, HashSet<Vector2Int> cells)
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (Random.value * 100 <= layer.fillPercent)
                {
                    layer.targetTilemap.SetTile(new Vector3Int(x, y, 0), layer.ruleTile);
                    cells.Add(new Vector2Int(x, y));
                }
    }

    private void GenerateRivers(LayerData layer, int layerIndex, HashSet<Vector2Int> cells)
    {
        for (int i = 0; i < layer.riverCount; i++)
        {
            Vector2Int start = PickEdge();
            Vector2Int end = PickEdge();

            if (start == end)
            {
                i--;
                continue;
            }

            var path = AStar(start, end);
            DrawPath(layer, path, layer.pathWidth, cells);
            riverPathCellsByLayer[layerIndex].Add(new List<Vector2Int>(path));
        }
    }

    private Vector2Int PickEdge()
    {
        int edge = Random.Range(0, 4);
        switch (edge)
        {
            case 0: return new Vector2Int(0, Random.Range(0, height));
            case 1: return new Vector2Int(width - 1, Random.Range(0, height));
            case 2: return new Vector2Int(Random.Range(0, width), 0);
            default: return new Vector2Int(Random.Range(0, width), height - 1);
        }
    }

    private List<Vector2Int> AStar(Vector2Int start, Vector2Int end)
    {
        var noise = new float[width, height];
        for (int nx = 0; nx < width; nx++)
            for (int ny = 0; ny < height; ny++)
                noise[nx, ny] = Random.Range(0f, 4f);

        var open = new List<Vector2Int> { start };
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var gScore = new Dictionary<Vector2Int, float> { [start] = 0 };

        while (open.Count > 0)
        {
            open.Sort((a, b) => (gScore[a] + Heuristic(a, end)).CompareTo(gScore[b] + Heuristic(b, end)));
            var current = open[0];
            open.RemoveAt(0);

            if (current == end)
                return Reconstruct(cameFrom, current);

            foreach (var dir in Dir8)
            {
                var next = current + dir;
                if (!Inside(next)) continue;

                float cost = gScore[current] + Vector2Int.Distance(current, next) + noise[next.x, next.y];

                if (!gScore.ContainsKey(next) || cost < gScore[next])
                {
                    gScore[next] = cost;
                    cameFrom[next] = current;
                    if (!open.Contains(next))
                        open.Add(next);
                }
            }
        }

        return new List<Vector2Int> { start };
    }

    private float Heuristic(Vector2Int a, Vector2Int b)
    {
        return Vector2Int.Distance(a, b);
    }

    private List<Vector2Int> Reconstruct(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
    {
        var path = new List<Vector2Int> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Add(current);
        }
        path.Reverse();
        return path;
    }

    private void DrawPath(LayerData layer, List<Vector2Int> path, int widthCells, HashSet<Vector2Int> filled)
    {
        if (path.Count < 2) return;

        List<Vector3> worldPath = new List<Vector3>();

        for (int i = 0; i < path.Count; i++)
        {
            var cell = path[i];
            SetTile(layer, cell, filled);
            worldPath.Add(layer.targetTilemap.CellToWorld(new Vector3Int(cell.x, cell.y, 0)));

            if (i > 0)
            {
                var prev = path[i - 1];
                int dx = cell.x - prev.x;
                int dy = cell.y - prev.y;
                if (dx != 0 && dy != 0)
                {
                    SetTile(layer, new Vector2Int(prev.x + dx, prev.y), filled);
                    SetTile(layer, new Vector2Int(prev.x, prev.y + dy), filled);
                }
            }

            if (widthCells >= 2)
            {
                foreach (var dir in Dir8)
                    SetTile(layer, cell + dir, filled);
            }

            if (widthCells >= 3)
            {
                foreach (var dir in Dir8)
                    if (Random.value > 0.35f)
                        SetTile(layer, cell + dir * 2, filled);
            }

            foreach (var dir in Dir8)
                if (Random.value > 0.7f)
                    SetTile(layer, cell + dir, filled);
        }

        debugPaths.Add(worldPath);
    }

    private void SetTile(LayerData layer, Vector2Int pos, HashSet<Vector2Int> filled)
    {
        if (!Inside(pos)) return;
        if (filled.Contains(pos)) return;

        layer.targetTilemap.SetTile(new Vector3Int(pos.x, pos.y, 0), layer.ruleTile);
        filled.Add(pos);
    }

    private bool Inside(Vector2Int p)
    {
        return p.x >= 0 && p.x < width && p.y >= 0 && p.y < height;
    }

    private void DeleteGenerated()
    {
        debugPaths.Clear();
        debugLayerCells.Clear();
        debugBridgesData.Clear();
        riverPathCellsByLayer.Clear();
        DestroyProps();
        DestroyBridges();
        foreach (var layer in layers)
        {
            if (layer.targetTilemap != null)
                layer.targetTilemap.ClearAllTiles();
        }

        SceneView.RepaintAll();
    }

    private void SaveSettings()
    {
        var data = new SaveData
        {
            width = this.width,
            height = this.height,
            debugRivers = this.debugRivers,
            debugLayers = this.debugLayers,
            debugBridges = this.debugBridges,
            layers = new List<SerializableLayerData>()
        };

        foreach (var layer in layers)
        {
            var sl = new SerializableLayerData
            {
                layerName = layer.layerName,
                ruleTileId = ObjToId(layer.ruleTile),
                tilemapId = ObjToId(layer.targetTilemap),
                isWater = layer.isWater,
                fillPercent = layer.fillPercent,
                riverCount = layer.riverCount,
                pathWidth = layer.pathWidth,
                bridgesPerRiver = layer.bridgesPerRiver,
                maxBridgeLength = layer.maxBridgeLength,
                bridgeSpriteHorizontalId = ObjToId(layer.bridgeSpriteHorizontal),
                bridgeSpriteVerticalId = ObjToId(layer.bridgeSpriteVertical),
                bridgeScale = layer.bridgeScale,
                propsFoldout = layer.propsFoldout,
                propEdgePadding = layer.propEdgePadding,
                props = new List<SerializablePropData>()
            };

            foreach (var prop in layer.props)
            {
                sl.props.Add(new SerializablePropData
                {
                    spriteId = ObjToId(prop.sprite),
                    spawnPercent = prop.spawnPercent,
                    useSpacing = prop.useSpacing,
                    spacingRadius = prop.spacingRadius,
                    randomFlipX = prop.randomFlipX
                });
            }

            data.layers.Add(sl);
        }

        EditorPrefs.SetString(SAVE_KEY, JsonUtility.ToJson(data));
    }

    private void LoadSettings()
    {
        if (!EditorPrefs.HasKey(SAVE_KEY)) return;

        var json = EditorPrefs.GetString(SAVE_KEY);
        if (string.IsNullOrEmpty(json)) return;

        SaveData data;
        try { data = JsonUtility.FromJson<SaveData>(json); }
        catch (System.Exception) { return; }
        if (data == null) return;

        width = data.width;
        height = data.height;
        debugRivers = data.debugRivers;
        debugLayers = data.debugLayers;
        debugBridges = data.debugBridges;
        layers.Clear();

        if (data.layers == null) return;

        foreach (var sl in data.layers)
        {
            var layer = new LayerData
            {
                layerName = sl.layerName,
                ruleTile = IdToObj<RuleTile>(sl.ruleTileId),
                targetTilemap = IdToObj<Tilemap>(sl.tilemapId),
                isWater = sl.isWater,
                fillPercent = sl.fillPercent,
                riverCount = sl.riverCount,
                pathWidth = sl.pathWidth,
                bridgesPerRiver = sl.bridgesPerRiver,
                maxBridgeLength = sl.maxBridgeLength >= 2 ? sl.maxBridgeLength : 12,
                bridgeSpriteHorizontal = IdToObj<Sprite>(sl.bridgeSpriteHorizontalId),
                bridgeSpriteVertical = IdToObj<Sprite>(sl.bridgeSpriteVerticalId),
                bridgeScale = sl.bridgeScale > 0.001f ? sl.bridgeScale : 1f,
                propsFoldout = sl.propsFoldout,
                propEdgePadding = sl.propEdgePadding,
                props = new List<PropData>()
            };

            if (sl.props != null)
            {
                foreach (var sp in sl.props)
                {
                    layer.props.Add(new PropData
                    {
                        sprite = IdToObj<Sprite>(sp.spriteId),
                        spawnPercent = sp.spawnPercent,
                        useSpacing = sp.useSpacing,
                        spacingRadius = sp.spacingRadius,
                        randomFlipX = sp.randomFlipX
                    });
                }
            }

            layers.Add(layer);
        }
    }

    private static string ObjToId(Object obj)
    {
        if (obj == null) return "";
        return GlobalObjectId.GetGlobalObjectIdSlow(obj).ToString();
    }

    private static T IdToObj<T>(string id) where T : Object
    {
        if (string.IsNullOrEmpty(id)) return null;
        if (!GlobalObjectId.TryParse(id, out var gid)) return null;
        return GlobalObjectId.GlobalObjectIdentifierToObjectSlow(gid) as T;
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        Handles.zTest = CompareFunction.Always;

        if (debugRivers)
        {
            foreach (var path in debugPaths)
            {
                for (int i = 0; i < path.Count - 1; i++)
                    Handles.DrawLine(path[i], path[i + 1], 4f);
            }
        }

        if (debugLayers && debugLayerCells.Count == layers.Count)
        {
            for (int li = 0; li < layers.Count; li++)
            {
                var layer = layers[li];
                if (layer.targetTilemap == null) continue;

                var tm = layer.targetTilemap;
                var cells = debugLayerCells[li];
                Color fillColor = debugLayerColors[li % debugLayerColors.Length];
                Color outlineColor = new Color(fillColor.r, fillColor.g, fillColor.b, 0.8f);

                foreach (var cell in cells)
                {
                    int x = cell.x, y = cell.y;
                    Vector3 bot = tm.CellToWorld(new Vector3Int(x, y, 0));
                    Vector3 right = tm.CellToWorld(new Vector3Int(x + 1, y, 0));
                    Vector3 top = tm.CellToWorld(new Vector3Int(x + 1, y + 1, 0));
                    Vector3 left = tm.CellToWorld(new Vector3Int(x, y + 1, 0));

                    Vector3[] verts = new Vector3[] { bot, right, top, left };
                    Handles.DrawSolidRectangleWithOutline(verts, fillColor, outlineColor);
                }
            }

            DrawDebugLayerLegend();
        }

        if (debugBridges)
        {
            Color bridgeColor = new Color(0.2f, 1f, 0.3f, 0.5f);
            Color bridgeOutline = new Color(0.2f, 1f, 0.3f, 0.9f);
            foreach (var (layerIndex, cells) in debugBridgesData)
            {
                if (layerIndex < 0 || layerIndex >= layers.Count || layers[layerIndex].targetTilemap == null) continue;
                var tm = layers[layerIndex].targetTilemap;
                foreach (var cell in cells)
                {
                    Vector3 bot = tm.CellToWorld(new Vector3Int(cell.x, cell.y, 0));
                    Vector3 right = tm.CellToWorld(new Vector3Int(cell.x + 1, cell.y, 0));
                    Vector3 top = tm.CellToWorld(new Vector3Int(cell.x + 1, cell.y + 1, 0));
                    Vector3 left = tm.CellToWorld(new Vector3Int(cell.x, cell.y + 1, 0));
                    Vector3[] verts = new Vector3[] { bot, right, top, left };
                    Handles.DrawSolidRectangleWithOutline(verts, bridgeColor, bridgeOutline);
                }
            }
        }
    }

    private void DrawDebugLayerLegend()
    {
        Handles.BeginGUI();
        float boxWidth = 160f;
        float lineHeight = 20f;
        float padding = 6f;
        float boxHeight = padding * 2 + layers.Count * lineHeight + 20f;

        Rect bg = new Rect(10, 10, boxWidth, boxHeight);
        GUI.Box(bg, GUIContent.none);

        GUI.Label(new Rect(bg.x + padding, bg.y + padding, boxWidth - padding * 2, lineHeight),
            "Layer Colors", EditorStyles.boldLabel);

        for (int li = 0; li < layers.Count; li++)
        {
            Color c = debugLayerColors[li % debugLayerColors.Length];
            float yPos = bg.y + padding + 20f + li * lineHeight;

            Rect colorRect = new Rect(bg.x + padding, yPos + 2, 14, 14);
            EditorGUI.DrawRect(colorRect, new Color(c.r, c.g, c.b, 1f));

            GUI.Label(new Rect(bg.x + padding + 20, yPos, boxWidth - padding * 2 - 20, lineHeight),
                layers[li].layerName);
        }

        Handles.EndGUI();
    }
}