using System.IO;
using SysKill.Isometric;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace SysKill.EditorTools
{
    /// <summary>Creates a complete playable 2D isometric sample from Assets/Art/iso.</summary>
    public sealed class IsometricDemoBuilderWindow : EditorWindow
    {
        const string ArtRoot = "Assets/Kenney2DSamples/Art/Isometric";
        const string OutputRoot = "Assets/Kenney2DSamples/Generated/Isometric";
        const string ScenePath = "Assets/Kenney2DSamples/Scenes/03_Isometric.unity";
        static readonly Vector3 CellSize = new Vector3(2.56f, 1.28f, 1f);
        const float FloorHeight = 1.28f;
        const int FloorSortingStride = 4000;
        static bool suiteAdditiveBuild;

        [SerializeField, Range(5, 12)] int mapWidth = 7;
        [SerializeField, Range(5, 12)] int mapHeight = 7;
        [SerializeField, Range(2, 4)] int floorCount = 2;
        [SerializeField] bool addSceneToBuildSettings = true;

        [MenuItem("Tools/SysKill/Isometric Demo Builder")]
        static void OpenWindow()
        {
            var window = GetWindow<IsometricDemoBuilderWindow>("Isometric Demo");
            window.minSize = new Vector2(430f, 260f);
            window.Show();
        }

        [MenuItem("Tools/SysKill/Create Playable Isometric Demo")]
        static void QuickBuild()
        {
            if (ConfirmSceneReplacement())
                BuildScene(7, 7, 2, true);
        }

        // Headless entry point used by CI or: Unity -batchmode -executeMethod ...BuildFromCommandLine
        public static void BuildFromCommandLine()
        {
            BuildScene(7, 7, 2, true);
        }

        public static void BuildSampleForSuite(int width = 7, int height = 7, int floors = 2, bool addToBuild = true)
        {
            suiteAdditiveBuild = true;
            try
            {
                BuildScene(width, height, floors, addToBuild);
            }
            finally
            {
                suiteAdditiveBuild = false;
            }
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("Playable 2D Isometric Demo", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "모든 층이 같은 Scene 공간에 높이 차이를 두고 함께 존재합니다. 층별 충돌은 분리되며 계단을 걸어서만 다른 층으로 이동합니다.\n" +
                "이동: WASD / 방향키 / 게임패드 스틱", MessageType.Info);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                mapWidth = EditorGUILayout.IntSlider("Map Width", mapWidth, 5, 12);
                mapHeight = EditorGUILayout.IntSlider("Map Height", mapHeight, 5, 12);
                floorCount = EditorGUILayout.IntSlider("Floor Count", floorCount, 2, 4);
                addSceneToBuildSettings = EditorGUILayout.Toggle("Add To Build Settings", addSceneToBuildSettings);
                EditorGUILayout.LabelField("Output Scene", ScenePath);
            }

            GUILayout.FlexibleSpace();
            GUI.enabled = AssetDatabase.IsValidFolder(ArtRoot);
            if (GUILayout.Button("Create / Replace Playable Scene", GUILayout.Height(38f)) && ConfirmSceneReplacement())
                BuildScene(mapWidth, mapHeight, floorCount, addSceneToBuildSettings);
            GUI.enabled = true;

            if (!AssetDatabase.IsValidFolder(ArtRoot))
                EditorGUILayout.HelpBox($"Source folder not found: {ArtRoot}", MessageType.Error);
        }

        static bool ConfirmSceneReplacement()
        {
            if (!File.Exists(ScenePath))
                return true;
            return EditorUtility.DisplayDialog("Replace isometric demo?", $"{ScenePath} will be replaced.", "Replace", "Cancel");
        }

        static void BuildScene(int width, int height, int floors, bool addToBuild)
        {
            if (!AssetDatabase.IsValidFolder(ArtRoot))
                throw new DirectoryNotFoundException($"Required source folder was not found: {ArtRoot}");

            width = Mathf.Max(5, width);
            height = Mathf.Max(5, height);
            floors = Mathf.Clamp(floors, 2, 4);

            EnsureFolder("Assets/Generated");
            EnsureFolder(OutputRoot);
            EnsureFolder(OutputRoot + "/Tiles");
            EnsureFolder("Assets/Scenes");

            Scene previousScene = SceneManager.GetActiveScene();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,
                suiteAdditiveBuild ? NewSceneMode.Additive : NewSceneMode.Single);
            SceneManager.SetActiveScene(scene);
            scene.name = "IsometricDemo";

            var gridObject = new GameObject("Isometric Grid");
            var grid = gridObject.AddComponent<Grid>();
            grid.cellLayout = GridLayout.CellLayout.Isometric;
            grid.cellSwizzle = GridLayout.CellSwizzle.XYZ;
            grid.cellSize = CellSize;

            Tile dirt = GetOrCreateTile("Dirt", LoadSprite("Isometric/dirt_E.png"));
            Tile stone = GetOrCreateTile("Stone", LoadSprite("Isometric/stoneTile_E.png"));
            Tile broken = GetOrCreateTile("BrokenStone", LoadSprite("Isometric/stoneMissingTiles_E.png"));

            int minX = -(width / 2);
            int minY = -(height / 2);
            int maxX = minX + width - 1;
            int maxY = minY + height - 1;
            GameObject player = CreatePlayer(width, height);
            var floorManagerObject = new GameObject("Floor Manager");
            var floorManager = floorManagerObject.AddComponent<IsometricFloorManager>();
            var floorRoots = new GameObject[floors];
            var floorTilemaps = new Tilemap[floors];
            var floorOffsets = new Vector2[floors];

            for (int floorIndex = 0; floorIndex < floors; floorIndex++)
            {
                var floorRoot = new GameObject($"Floor {floorIndex + 1}");
                floorRoot.transform.SetParent(gridObject.transform, false);
                floorRoot.transform.localPosition = Vector3.up * (floorIndex * FloorHeight);
                floorRoots[floorIndex] = floorRoot;
                floorOffsets[floorIndex] = floorRoot.transform.position;

                Tilemap floor = CreateTilemap(floorRoot.transform, "Ground", (floorIndex * FloorSortingStride) - 1000);
                floorTilemaps[floorIndex] = floor;
                // The Tile assets carry a Cartesian visual offset, so the
                // standard pivot and standard tile anchor remain untouched.
                floor.tileAnchor = new Vector3(0.5f, 0.5f, 0f);
                FillFloor(floor, floorIndex, minX, minY, maxX, maxY, dirt, stone, broken);

                var decorRoot = new GameObject("Walls & Props").transform;
                decorRoot.SetParent(floorRoot.transform, false);
                int sortingBase = floorIndex * FloorSortingStride;
                CreatePerimeterDecor(grid, decorRoot, minX, minY, maxX, maxY, sortingBase);
                CreateProps(grid, decorRoot, minX, minY, maxX, maxY, floorIndex, sortingBase);
            }

            CreateFloorConnections(grid, floorRoots, floorTilemaps, floorManager, minX, minY, maxX, maxY);
            // Configure after every prop and stair collider exists so the
            // initial collision pass can disable all non-current floors.
            floorManager.Configure(floorRoots, floorOffsets, player.GetComponent<Rigidbody2D>(), FloorSortingStride);
            CreateCamera(player.transform, width, height);
            CreateInstructions(floors);

            if (!suiteAdditiveBuild)
                Selection.activeGameObject = player;
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            if (addToBuild)
                AddSceneToBuildSettings(ScenePath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            if (suiteAdditiveBuild)
            {
                EditorSceneManager.CloseScene(scene, true);
                if (previousScene.IsValid() && previousScene.isLoaded)
                    SceneManager.SetActiveScene(previousScene);
            }
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath));
            Debug.Log($"Playable isometric demo created: {ScenePath}");
        }

        static void FillFloor(Tilemap tilemap, int floorIndex, int minX, int minY, int maxX, int maxY,
            Tile dirt, Tile stone, Tile broken)
        {
            for (int y = minY; y <= maxY; y++)
            for (int x = minX; x <= maxX; x++)
            {
                bool border = x == minX || x == maxX || y == minY || y == maxY;
                int pattern = Mathf.Abs((x * 17) + (y * 31) + (floorIndex * 13));
                Tile tile;
                if (border)
                    tile = dirt;
                else if ((pattern + floorIndex) % 7 == 0)
                    tile = broken;
                else
                    tile = floorIndex % 2 == 0 ? stone : dirt;
                tilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }

        static void CreateFloorConnections(Grid grid, GameObject[] floorRoots, Tilemap[] floorTilemaps, IsometricFloorManager manager,
            int minX, int minY, int maxX, int maxY)
        {
            for (int lowerFloor = 0; lowerFloor < floorRoots.Length - 1; lowerFloor++)
            {
                bool useRightStairwell = lowerFloor % 2 == 0;
                var stairCell = useRightStairwell
                    ? new Vector2Int(maxX - 1, minY + 1)
                    : new Vector2Int(minX + 1, maxY - 1);
                var exitCell = useRightStairwell
                    ? new Vector2Int(maxX - 2, minY + 2)
                    : new Vector2Int(minX + 2, maxY - 2);

                Vector3 stairPosition = CellCenter(grid, stairCell.x, stairCell.y);
                Vector3 baseExitPosition = CellCenter(grid, exitCell.x, exitCell.y);
                floorTilemaps[lowerFloor].SetTile(new Vector3Int(stairCell.x, stairCell.y, 0), null);
                floorTilemaps[lowerFloor + 1].SetTile(new Vector3Int(stairCell.x, stairCell.y, 0), null);

                CreateStairPortal(
                    $"Stairs Up to Floor {lowerFloor + 2}",
                    LoadSprite(useRightStairwell ? "Isometric/stairs_N.png" : "Isometric/stairs_E.png"),
                    stairPosition,
                    floorRoots[lowerFloor].transform,
                    manager,
                    lowerFloor,
                    lowerFloor + 1,
                    baseExitPosition + (Vector3.up * ((lowerFloor + 1) * FloorHeight)),
                    ((lowerFloor + 1) * FloorSortingStride) - 250);

                CreateStairPortal(
                    $"Stairs Down to Floor {lowerFloor + 1}",
                    LoadSprite(useRightStairwell ? "Isometric/stairs_S.png" : "Isometric/stairs_W.png"),
                    stairPosition,
                    floorRoots[lowerFloor + 1].transform,
                    manager,
                    lowerFloor + 1,
                    lowerFloor,
                    baseExitPosition + (Vector3.up * (lowerFloor * FloorHeight)),
                    ((lowerFloor + 1) * FloorSortingStride) - 250);
            }
        }

        static void CreateStairPortal(string name, Sprite sprite, Vector3 anchor, Transform parent,
            IsometricFloorManager manager, int sourceFloor, int destinationFloor,
            Vector2 destinationPosition, int sortingBase)
        {
            GameObject stairs = CreateDecor(name, sprite, anchor, parent, false, sortingBase);
            var trigger = stairs.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(1.15f, 0.58f);
            var portal = stairs.AddComponent<IsometricStairPortal>();
            portal.Configure(manager, sourceFloor, destinationFloor, destinationPosition);
        }

        static Tilemap CreateTilemap(Transform parent, string name, int sortingOrder)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var tilemap = go.AddComponent<Tilemap>();
            var renderer = go.AddComponent<TilemapRenderer>();
            renderer.sortOrder = TilemapRenderer.SortOrder.TopRight;
            renderer.mode = TilemapRenderer.Mode.Individual;
            renderer.sortingOrder = sortingOrder;
            return tilemap;
        }

        static void CreatePerimeterDecor(Grid grid, Transform parent, int minX, int minY, int maxX, int maxY, int sortingBase)
        {
            Sprite wallNorth = LoadSprite("Isometric/stoneWall_N.png");
            Sprite wallWest = LoadSprite("Isometric/stoneWall_W.png");
            for (int x = minX; x <= maxX; x++)
                CreateDecor($"North Wall {x}", wallNorth, CellCenter(grid, x, maxY), parent, false, sortingBase);
            for (int y = minY; y < maxY; y++)
                CreateDecor($"West Wall {y}", wallWest, CellCenter(grid, minX, y), parent, false, sortingBase);
        }

        static void CreateProps(Grid grid, Transform parent, int minX, int minY, int maxX, int maxY,
            int floorIndex, int sortingBase)
        {
            Vector2Int[] cells =
            {
                new Vector2Int(minX + 1, minY + 2),
                new Vector2Int(maxX - 2, minY + 1),
                new Vector2Int(minX + 2, maxY - 1),
                new Vector2Int(maxX - 1, maxY - 2)
            };
            string[] sprites =
            {
                "Isometric/woodenCrate_E.png",
                "Isometric/barrel_E.png",
                "Isometric/chestClosed_E.png",
                "Isometric/woodenCrates_E.png"
            };

            for (int i = 0; i < cells.Length; i++)
                CreateDecor($"Floor {floorIndex + 1} Prop {i + 1}", LoadSprite(sprites[i]),
                    CellCenter(grid, cells[i].x, cells[i].y), parent, true, sortingBase);
        }

        static GameObject CreateDecor(string name, Sprite sprite, Vector3 anchor, Transform parent, bool collider, int sortingBase)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            // Anchor is expressed in the base Grid's local plane. Keeping it
            // local lets the containing floor add its vertical elevation.
            root.transform.localPosition = anchor;

            var visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);
            visual.transform.localPosition = new Vector3(0f, 1.78f, 0f);
            var renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingBase - Mathf.RoundToInt(anchor.y * 100f);

            if (collider)
            {
                var box = root.AddComponent<BoxCollider2D>();
                box.size = new Vector2(0.65f, 0.35f);
            }
            return root;
        }

        static GameObject CreatePlayer(int width, int height)
        {
            var player = new GameObject("Player");
            player.transform.position = Vector3.zero;
            var body = player.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;

            var collider = player.AddComponent<CapsuleCollider2D>();
            collider.direction = CapsuleDirection2D.Horizontal;
            collider.size = new Vector2(0.42f, 0.22f);
            collider.offset = new Vector2(0f, 0.08f);

            var visual = new GameObject("Character Visual");
            visual.transform.SetParent(player.transform, false);
            visual.transform.localPosition = new Vector3(0f, 1.82f, 0f);
            var renderer = visual.AddComponent<SpriteRenderer>();

            var idle = new Sprite[8];
            var run = new Sprite[80];
            for (int direction = 0; direction < 8; direction++)
            {
                idle[direction] = LoadSprite($"Characters/Male/Male_{direction}_Idle0.png");
                for (int frame = 0; frame < 10; frame++)
                    run[(direction * 10) + frame] = LoadSprite($"Characters/Male/Male_{direction}_Run{frame}.png");
            }
            renderer.sprite = idle[3];

            float halfWidth = (width - 1) * 0.5f - 0.35f;
            float halfHeight = (height - 1) * 0.5f - 0.35f;
            var controller = player.AddComponent<IsometricPlayerController>();
            controller.Configure(renderer, idle, run,
                new Vector2(-halfWidth, -halfHeight), new Vector2(halfWidth, halfHeight),
                new Vector2(CellSize.x, CellSize.y));
            return player;
        }

        static void CreateCamera(Transform player, int width, int height)
        {
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            go.transform.position = new Vector3(0f, 0.7f, -10f);
            var camera = go.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = Mathf.Max(5.5f, Mathf.Max(width, height) * 0.82f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.055f, 0.075f, 0.09f, 1f);
            go.AddComponent<AudioListener>();
            var follow = go.AddComponent<IsometricCameraFollow>();
            follow.Configure(player, new Vector3(0f, 0.7f, -10f));
        }

        static void CreateInstructions(int floors)
        {
            var go = new GameObject($"Controls (Move: WASD, Automatic Stair Ramps, Floors: {floors})");
            go.transform.position = new Vector3(0f, 0f, 0f);
        }

        static Vector3 CellCenter(Grid grid, int x, int y)
        {
            return grid.GetCellCenterWorld(new Vector3Int(x, y, 0));
        }

        static Sprite LoadSprite(string relativePath)
        {
            string path = $"{ArtRoot}/{relativePath}";
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
                throw new FileNotFoundException($"Sprite could not be loaded. Check Texture Type is Sprite: {path}");
            return sprite;
        }

        static Tile GetOrCreateTile(string name, Sprite sprite)
        {
            string path = $"{OutputRoot}/Tiles/{name}.asset";
            Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
            if (tile == null)
            {
                tile = CreateInstance<Tile>();
                AssetDatabase.CreateAsset(tile, path);
            }
            tile.sprite = sprite;
            tile.color = Color.white;
            tile.colliderType = Tile.ColliderType.None;
            // Art/iso sprites are 256x512 with the ground diamond near the
            // bottom of the canvas. Offset only the rendered sprite in local
            // Cartesian Y while preserving the default (0.5, 0.5) pivot/anchor.
            tile.transform = Matrix4x4.Translate(new Vector3(0f, 1.76f, 0f));
            tile.flags = TileFlags.LockTransform;
            EditorUtility.SetDirty(tile);
            return tile;
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;
            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            string name = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent))
                EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }

        static void AddSceneToBuildSettings(string scenePath)
        {
            var scenes = EditorBuildSettings.scenes;
            foreach (var scene in scenes)
                if (scene.path == scenePath)
                    return;

            var updated = new EditorBuildSettingsScene[scenes.Length + 1];
            scenes.CopyTo(updated, 0);
            updated[updated.Length - 1] = new EditorBuildSettingsScene(scenePath, true);
            EditorBuildSettings.scenes = updated;
        }
    }
}
