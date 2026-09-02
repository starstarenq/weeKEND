using System;
using System.IO;
using KenneySamples.Common;
using KenneySamples.Farm;
using KenneySamples.SideView;
using SysKill.EditorTools;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace KenneySamples.Editor
{
    /// <summary>
    /// Curates a small teaching subset from the source art packs, creates three
    /// independent 2D sample scenes, and exports a self-contained unitypackage.
    /// Original files under Assets/Art are never modified.
    /// </summary>
    public sealed class Kenney2DSampleSuiteBuilder : EditorWindow
    {
        const string Root = "Assets/Kenney2DSamples";
        const string FarmSource = "Assets/Art/kenney_tiny-farm";
        const string BackgroundSource = "Assets/Art/kenney_background-elements";
        const string IsometricSource = "Assets/Art/Iso";
        const string FarmArt = Root + "/Art/Farm";
        const string BackgroundArt = Root + "/Art/Backgrounds";
        const string IsometricArt = Root + "/Art/Isometric";
        const string ScenesRoot = Root + "/Scenes";
        const string GeneratedRoot = Root + "/Generated";
        const string PackagePath = "Assets/Kenney2DSamples.unitypackage";

        static readonly int[] FarmTerrain =
        {
            0, 1, 12, 13, 24, 25, 36, 37, 48, 49, 50, 51, 52, 53, 60, 61, 62, 63
        };
        static readonly int[] FarmNature = { 2, 3, 4, 5, 6, 7, 8, 9 };
        static readonly int[] FarmProps = { 72, 73, 74, 75, 84, 85, 86, 87 };
        static readonly int[] FarmStructures = { 114, 115, 116, 117, 118, 119 };
        static readonly int[] FarmCharacters = { 109 };
        static readonly int[] FarmAnimals = { 120, 121, 122 };

        static readonly string[] BackgroundFiles =
        {
            "sky.png", "pointy_mountains.png", "mountain1.png", "hills1.png",
            "hills2.png", "clouds1.png", "grass1.png"
        };

        static readonly string[] IsometricEnvironment =
        {
            "dirt_E.png", "stoneTile_E.png", "stoneMissingTiles_E.png",
            "stoneWall_N.png", "stoneWall_W.png", "woodenCrate_E.png",
            "woodenCrates_E.png", "barrel_E.png", "chestClosed_E.png",
            "stairs_E.png", "stairs_N.png", "stairs_S.png", "stairs_W.png"
        };

        [MenuItem("Tools/Kenney 2D Samples/Sample Suite Builder")]
        static void Open()
        {
            var window = GetWindow<Kenney2DSampleSuiteBuilder>("Kenney 2D Samples");
            window.minSize = new Vector2(500f, 330f);
            window.Show();
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("Kenney 2D Teaching Sample Suite", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "원본 Art 폴더는 보존합니다. 교육에 필요한 최소 에셋만 표준 폴더로 복사하고 다음 Scene을 생성합니다.\n" +
                "01 Farm Tilemap / 02 SideView Parallax / 03 Isometric Multi-floor", MessageType.Info);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Package Root", Root);
                EditorGUILayout.LabelField("Export", PackagePath);
                EditorGUILayout.LabelField("Farm selection", "44 of 132 tiles");
                EditorGUILayout.LabelField("Background selection", $"{BackgroundFiles.Length} raster layers");
                EditorGUILayout.LabelField("Isometric selection", "13 environment + 88 character frames");
            }

            GUILayout.Space(8f);
            if (GUILayout.Button("1. Prepare Curated Assets", GUILayout.Height(34f)))
                PrepareCuratedAssets();
            if (GUILayout.Button("2. Build All Three Sample Scenes", GUILayout.Height(34f)))
                BuildAllScenes();
            if (GUILayout.Button("3. Export UnityPackage", GUILayout.Height(34f)))
                ExportPackage();
            GUILayout.Space(4f);
            if (GUILayout.Button("Prepare + Build + Export", GUILayout.Height(42f)))
            {
                PrepareCuratedAssets();
                BuildAllScenes();
                ExportPackage();
            }
        }

        [MenuItem("Tools/Kenney 2D Samples/Prepare Build And Export")]
        public static void PrepareBuildAndExport()
        {
            PrepareCuratedAssets();
            BuildAllScenes();
            ExportPackage();
        }

        public static void PrepareCuratedAssets()
        {
            ValidateSourceFolders();
            EnsureFolder(Root + "/Art");
            EnsureFolder(FarmArt);
            EnsureFolder(BackgroundArt);
            EnsureFolder(IsometricArt);
            EnsureFolder(ScenesRoot);
            EnsureFolder(GeneratedRoot);

            CopyFarmGroup(FarmTerrain, "Terrain");
            CopyFarmGroup(FarmNature, "Nature");
            CopyFarmGroup(FarmProps, "Props");
            CopyFarmGroup(FarmStructures, "Structures");
            CopyFarmGroup(FarmCharacters, "Characters");
            CopyFarmGroup(FarmAnimals, "Animals");

            EnsureFolder(BackgroundArt + "/Flat");
            foreach (string file in BackgroundFiles)
                CopyTexture($"{BackgroundSource}/PNG/Flat/{file}", $"{BackgroundArt}/Flat/{file}", 100f, FilterMode.Bilinear);

            EnsureFolder(IsometricArt + "/Isometric");
            foreach (string file in IsometricEnvironment)
                CopyTexture($"{IsometricSource}/Isometric/{file}", $"{IsometricArt}/Isometric/{file}", 100f, FilterMode.Bilinear);

            EnsureFolder(IsometricArt + "/Characters");
            EnsureFolder(IsometricArt + "/Characters/Male");
            for (int direction = 0; direction < 8; direction++)
            {
                CopyTexture(
                    $"{IsometricSource}/Characters/Male/Male_{direction}_Idle0.png",
                    $"{IsometricArt}/Characters/Male/Male_{direction}_Idle0.png", 100f, FilterMode.Bilinear);
                for (int frame = 0; frame < 10; frame++)
                    CopyTexture(
                        $"{IsometricSource}/Characters/Male/Male_{direction}_Run{frame}.png",
                        $"{IsometricArt}/Characters/Male/Male_{direction}_Run{frame}.png", 100f, FilterMode.Bilinear);
            }

            EnsureFolder(Root + "/ThirdPartyNotices");
            CopyAssetReplacing(FarmSource + "/License.txt", Root + "/ThirdPartyNotices/Kenney_TinyFarm_License.txt");
            CopyAssetReplacing(BackgroundSource + "/License.txt", Root + "/ThirdPartyNotices/Kenney_BackgroundElements_License.txt");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Curated Kenney 2D teaching assets prepared under " + Root);
        }

        public static void BuildAllScenes()
        {
            if (!AssetDatabase.IsValidFolder(FarmArt) || !AssetDatabase.IsValidFolder(BackgroundArt))
                throw new InvalidOperationException("Run Prepare Curated Assets first.");

            BuildFarmScene();
            BuildSideViewScene();
            IsometricDemoBuilderWindow.BuildSampleForSuite(7, 7, 2, false);
            AddScenesToBuildSettings(
                ScenesRoot + "/01_FarmTilemap.unity",
                ScenesRoot + "/02_SideViewParallax.unity",
                ScenesRoot + "/03_Isometric.unity");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("All three Kenney 2D sample scenes were created.");
        }

        public static void ExportPackage()
        {
            if (!AssetDatabase.IsValidFolder(ScenesRoot))
                throw new InvalidOperationException("Build the sample scenes before exporting.");
            // Every required project asset lives below Root. Avoid
            // IncludeDependencies here because it also exports URP package-cache
            // shader includes that the target Unity project already supplies.
            AssetDatabase.ExportPackage(Root, PackagePath, ExportPackageOptions.Recurse);
            AssetDatabase.Refresh();
            EditorGUIUtility.PingObject(AssetDatabase.LoadMainAssetAtPath(PackagePath));
            Debug.Log("UnityPackage exported: " + PackagePath);
        }

        static void BuildFarmScene()
        {
            EnsureFolder(GeneratedRoot + "/Farm");
            EnsureFolder(GeneratedRoot + "/Farm/Tiles");
            Scene previousScene = SceneManager.GetActiveScene();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            scene.name = "01_FarmTilemap";

            var gridObject = new GameObject("Farm Grid");
            var grid = gridObject.AddComponent<Grid>();
            grid.cellSize = Vector3.one;

            Tilemap paths = CreateTilemap(gridObject.transform, "Paths", 0);
            Tile vertical = GetOrCreateTile("Farm_PathVertical", FarmSpritePath(12, "Terrain"), false);
            Tile horizontal = GetOrCreateTile("Farm_PathHorizontal", FarmSpritePath(49, "Terrain"), false);
            Tile junction = GetOrCreateTile("Farm_PathJunction", FarmSpritePath(51, "Terrain"), false);
            for (int y = -5; y <= 5; y++) paths.SetTile(new Vector3Int(0, y, 0), vertical);
            for (int x = -8; x <= 8; x++) paths.SetTile(new Vector3Int(x, 0, 0), horizontal);
            paths.SetTile(Vector3Int.zero, junction);

            Tilemap decor = CreateTilemap(gridObject.transform, "Decor", 100);
            Tile tree = GetOrCreateTile("Farm_Tree", FarmSpritePath(3, "Nature"), false);
            Tile crop = GetOrCreateTile("Farm_Crop", FarmSpritePath(5, "Nature"), false);
            Tile sheep = GetOrCreateTile("Farm_Sheep", FarmSpritePath(120, "Animals"), false);
            foreach (Vector3Int cell in new[] { new Vector3Int(-5, 3), new Vector3Int(5, 3), new Vector3Int(-6, -3), new Vector3Int(6, -3) })
                decor.SetTile(cell, tree);
            for (int x = 2; x <= 5; x++)
            for (int y = 1; y <= 3; y++)
                decor.SetTile(new Vector3Int(x, y, 0), crop);
            decor.SetTile(new Vector3Int(-3, 2, 0), sheep);

            Tilemap obstacles = CreateTilemap(gridObject.transform, "Collision Objects", 200);
            Tile barn = GetOrCreateTile("Farm_Barn", FarmSpritePath(114, "Structures"), true);
            obstacles.SetTile(new Vector3Int(-4, -2, 0), barn);
            obstacles.gameObject.AddComponent<TilemapCollider2D>();

            GameObject player = CreateFarmPlayer();
            CreateCamera(player.transform, new Vector3(0f, 0f, -10f), 6f, true, true,
                new Color(0.43f, 0.68f, 0.30f, 1f));
            CreateLabelObject("Tilemap layers: Paths / Decor / Collision Objects");
            EditorSceneManager.SaveScene(scene, ScenesRoot + "/01_FarmTilemap.unity");
            EditorSceneManager.CloseScene(scene, true);
            if (previousScene.IsValid() && previousScene.isLoaded)
                SceneManager.SetActiveScene(previousScene);
        }

        static GameObject CreateFarmPlayer()
        {
            var player = new GameObject("Farm Player");
            player.transform.position = new Vector3(-2f, -2f, 0f);
            var renderer = player.AddComponent<SpriteRenderer>();
            renderer.sprite = LoadSprite(FarmSpritePath(109, "Characters"));
            var body = player.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            var collider = player.AddComponent<CapsuleCollider2D>();
            collider.size = new Vector2(0.55f, 0.8f);
            var controller = player.AddComponent<FarmTopDownController>();
            controller.Configure(renderer);
            return player;
        }

        static void BuildSideViewScene()
        {
            Scene previousScene = SceneManager.GetActiveScene();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            scene.name = "02_SideViewParallax";

            GameObject player = CreateSideViewPlayer();
            Camera camera = CreateCamera(player.transform, new Vector3(-4f, 0f, -10f), 5.4f, true, false,
                new Color(0.72f, 0.88f, 0.95f, 1f));

            CreateParallaxLayer("Sky", "sky.png", camera, 0.02f, 0f, -100, 5);
            CreateParallaxLayer("Far Mountains", "pointy_mountains.png", camera, 0.12f, -1.2f, -80, 7);
            CreateParallaxLayer("Clouds", "clouds1.png", camera, 0.20f, 1.6f, -60, 7);
            CreateParallaxLayer("Hills Far", "hills1.png", camera, 0.34f, -2.4f, -40, 7);
            CreateParallaxLayer("Hills Near", "hills2.png", camera, 0.56f, -3.0f, -20, 7);
            CreateParallaxLayer("Foreground Grass", "grass1.png", camera, 0.82f, -3.65f, 20, 9);

            var ground = new GameObject("Ground Collision");
            ground.transform.position = new Vector3(0f, -4.15f, 0f);
            var groundCollider = ground.AddComponent<BoxCollider2D>();
            groundCollider.size = new Vector2(100f, 1f);

            CreateLabelObject("Parallax: Sky 0.02 / Mountains 0.12 / Hills 0.34-0.56 / Grass 0.82");
            EditorSceneManager.SaveScene(scene, ScenesRoot + "/02_SideViewParallax.unity");
            EditorSceneManager.CloseScene(scene, true);
            if (previousScene.IsValid() && previousScene.isLoaded)
                SceneManager.SetActiveScene(previousScene);
        }

        static GameObject CreateSideViewPlayer()
        {
            var player = new GameObject("SideView Player");
            player.transform.position = new Vector3(-4f, -3.25f, 0f);
            var renderer = player.AddComponent<SpriteRenderer>();
            renderer.sprite = LoadSprite(FarmSpritePath(109, "Characters"));
            renderer.sortingOrder = 100;
            var body = player.AddComponent<Rigidbody2D>();
            body.gravityScale = 2.5f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            var collider = player.AddComponent<CapsuleCollider2D>();
            collider.size = new Vector2(0.55f, 0.8f);
            player.AddComponent<SideViewPlayerController>();
            return player;
        }

        static Camera CreateCamera(Transform target, Vector3 position, float size, bool followX, bool followY, Color color)
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = position;
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = size;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = color;
            cameraObject.AddComponent<AudioListener>();
            var follow = cameraObject.AddComponent<SampleCameraFollow2D>();
            follow.Configure(target, followX, followY, new Vector3(0f, 0f, -10f));
            return camera;
        }

        static void CreateParallaxLayer(string name, string spriteFile, Camera camera, float factor,
            float y, int sortingOrder, int repeatCount)
        {
            Sprite sprite = LoadSprite(BackgroundArt + "/Flat/" + spriteFile);
            var root = new GameObject(name);
            root.transform.position = new Vector3(0f, y, 0f);
            var parallax = root.AddComponent<ParallaxLayer2D>();
            parallax.Configure(camera, factor);
            float width = Mathf.Max(0.1f, sprite.bounds.size.x);
            int half = repeatCount / 2;
            for (int i = -half; i <= half; i++)
            {
                var child = new GameObject($"{name} {i + half + 1}");
                child.transform.SetParent(root.transform, false);
                child.transform.localPosition = new Vector3(i * width, 0f, 0f);
                var renderer = child.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.sortingOrder = sortingOrder;
            }
        }

        static Tilemap CreateTilemap(Transform parent, string name, int sortingOrder)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var tilemap = go.AddComponent<Tilemap>();
            tilemap.tileAnchor = new Vector3(0.5f, 0.5f, 0f);
            var renderer = go.AddComponent<TilemapRenderer>();
            renderer.mode = TilemapRenderer.Mode.Individual;
            renderer.sortingOrder = sortingOrder;
            return tilemap;
        }

        static Tile GetOrCreateTile(string name, string spritePath, bool collider)
        {
            string folder = GeneratedRoot + "/Farm/Tiles";
            EnsureFolder(folder);
            string path = folder + "/" + name + ".asset";
            Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
            if (tile == null)
            {
                tile = CreateInstance<Tile>();
                AssetDatabase.CreateAsset(tile, path);
            }
            tile.sprite = LoadSprite(spritePath);
            tile.colliderType = collider ? Tile.ColliderType.Sprite : Tile.ColliderType.None;
            tile.color = Color.white;
            EditorUtility.SetDirty(tile);
            return tile;
        }

        static void CopyFarmGroup(int[] ids, string category)
        {
            EnsureFolder(FarmArt + "/" + category);
            foreach (int id in ids)
            {
                string file = $"tile_{id:0000}.png";
                CopyTexture($"{FarmSource}/Tiles/{file}", $"{FarmArt}/{category}/{file}", 16f, FilterMode.Point);
            }
        }

        static void CopyTexture(string source, string destination, float pixelsPerUnit, FilterMode filter)
        {
            CopyAssetReplacing(source, destination);
            AssetDatabase.ImportAsset(destination, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(destination) as TextureImporter;
            if (importer == null)
                throw new InvalidOperationException("Not a texture: " + destination);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.filterMode = filter;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        static void CopyAssetReplacing(string source, string destination)
        {
            if (AssetDatabase.LoadMainAssetAtPath(source) == null)
                throw new FileNotFoundException("Required source asset was not found", source);
            string parent = Path.GetDirectoryName(destination)?.Replace('\\', '/');
            EnsureFolder(parent);
            if (AssetDatabase.LoadMainAssetAtPath(destination) != null)
                AssetDatabase.DeleteAsset(destination);
            if (!AssetDatabase.CopyAsset(source, destination))
                throw new IOException($"Could not copy {source} to {destination}");
        }

        static string FarmSpritePath(int id, string category) => $"{FarmArt}/{category}/tile_{id:0000}.png";

        static Sprite LoadSprite(string path)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
                throw new FileNotFoundException("Sprite was not imported correctly", path);
            return sprite;
        }

        static void ValidateSourceFolders()
        {
            foreach (string path in new[] { FarmSource, BackgroundSource, IsometricSource })
                if (!AssetDatabase.IsValidFolder(path))
                    throw new DirectoryNotFoundException("Required source folder was not found: " + path);
        }

        static void EnsureFolder(string path)
        {
            if (string.IsNullOrEmpty(path) || AssetDatabase.IsValidFolder(path))
                return;
            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        static void AddScenesToBuildSettings(params string[] scenePaths)
        {
            var scenes = new EditorBuildSettingsScene[scenePaths.Length];
            for (int i = 0; i < scenePaths.Length; i++)
                scenes[i] = new EditorBuildSettingsScene(scenePaths[i], true);
            EditorBuildSettings.scenes = scenes;
        }

        static void CreateLabelObject(string text)
        {
            var go = new GameObject(text);
            go.transform.position = Vector3.zero;
        }
    }
}
