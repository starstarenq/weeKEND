using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SysKill.DialogueSample.Editor
{
    [InitializeOnLoad]
    public static class DialogueSampleBuilder
    {
        const string Root = "Assets/DialogueSample";
        const string ConfigFolder = Root + "/Resources/Config";
        const string ScenePath = Root + "/Scenes/DialogueSampleScene.unity";

        static DialogueSampleBuilder() { EditorApplication.delayCall += CreateOnce; }

        static void CreateOnce()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            EnsureSettings();
            EnsureBranchSample();
            if (!File.Exists(ScenePath)) Build();
        }

        [MenuItem("GameLecture/Dialogue/Create Sample Scene")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            EnsureFolder(ConfigFolder);
            EnsureSettings();
            EnsureBranchSample();
            EnsureFolder(Root + "/Scenes");
            var guide = Portrait("DialogueGuide", new Color32(72, 157, 207, 255));
            var traveler = Portrait("DialogueTraveler", new Color32(220, 149, 75, 255));
            var path = ConfigFolder + "/DialogueSampleConfig.asset";
            if (AssetDatabase.LoadAssetAtPath<DialogueConfig>(path) == null)
            {
                var config = ScriptableObject.CreateInstance<DialogueConfig>();
                config.lines = new[]
                {
                    new DialogueConfigLine { id = 10000000, speaker = "안내자", sprite = guide, text = "이 대사는 ScriptableObject에서 불러왔습니다." },
                    new DialogueConfigLine { id = 10000001, speaker = "여행자", sprite = traveler, text = "DialogueSampleConfig의 Lines에서 화자, Sprite, 대사를 편집하세요." },
                    new DialogueConfigLine { id = 10000002, speaker = "안내자", sprite = guide, text = "Sprite는 Inspector에 드래그해서 연결할 수 있습니다." }
                };
                AssetDatabase.CreateAsset(config, path);
            }
            AssetDatabase.SaveAssets();
            if (File.Exists(ScenePath)) return;
            var active = SceneManager.GetActiveScene();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            try
            {
                var host = new GameObject("Dialogue Sample");
                SceneManager.MoveGameObjectToScene(host, scene);
                var player = host.AddComponent<DialogueSamplePlayer>();
                if (TMPro.TMP_Settings.defaultFontAsset != null)
                    DialogueUGUIMigration.Migrate(player, TMPro.TMP_Settings.defaultFontAsset);
                else Debug.LogWarning("TMP 기본 폰트가 없습니다. 폰트 설정 후 Migrate Current Scene to UGUI를 실행하세요.");
                var cameraObject = new GameObject("Main Camera", typeof(Camera));
                SceneManager.MoveGameObjectToScene(cameraObject, scene);
                var camera = cameraObject.GetComponent<Camera>();
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(.04f, .06f, .1f);
                cameraObject.tag = "MainCamera";
                if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new IOException("Could not save sample scene.");
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
                if (active.IsValid() && active.isLoaded) SceneManager.SetActiveScene(active);
            }
            Debug.Log("Dialogue sample created: " + ScenePath);
        }

        static Sprite Portrait(string name, Color color)
        {
            var path = ConfigFolder + "/" + name + ".asset";
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (existing != null) return existing;
            var texture = new Texture2D(64, 64, TextureFormat.RGBA32, false) { name = name + "Texture" };
            var pixels = new Color[64 * 64];
            for (var y = 0; y < 64; y++)
                for (var x = 0; x < 64; x++)
                {
                    var face = (x - 32) * (x - 32) + (y - 36) * (y - 36) < 22 * 22;
                    var eye = y >= 38 && y <= 42 && ((x >= 22 && x <= 26) || (x >= 38 && x <= 42));
                    var mouth = y >= 24 && y <= 26 && x >= 25 && x <= 39;
                    pixels[y * 64 + x] = eye || mouth ? Color.black : face ? color : Color.clear;
                }
            texture.SetPixels(pixels);
            texture.Apply();
            var sprite = Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(.5f, .5f), 64, 0, SpriteMeshType.FullRect);
            sprite.name = name;
            AssetDatabase.CreateAsset(sprite, path);
            AssetDatabase.AddObjectToAsset(texture, sprite);
            return sprite;
        }

        [MenuItem("GameLecture/Dialogue/Select Dialogue Settings")]
        public static void SelectSettings()
        {
            EnsureSettings();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<DialogueSettings>(ConfigFolder + "/DialogueSettings.asset");
            EditorGUIUtility.PingObject(Selection.activeObject);
        }

        static void EnsureSettings()
        {
            EnsureFolder(ConfigFolder);
            var path = ConfigFolder + "/DialogueSettings.asset";
            if (AssetDatabase.LoadAssetAtPath<DialogueSettings>(path) != null) return;
            const string sounds = Root + "/Resources/Sounds";
            EnsureFolder(sounds);
            var soundPath = sounds + "/DialogueKeyboard.wav";
            if (!File.Exists(soundPath))
            {
                // Short deterministic synthesized key click; no external audio dependency.
                const int rate = 44100;
                const int count = 1764;
                var random = new System.Random(702);
                using (var writer = new BinaryWriter(File.Create(soundPath)))
                {
                    writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); writer.Write(36 + count * 2);
                    writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt ")); writer.Write(16);
                    writer.Write((short)1); writer.Write((short)1); writer.Write(rate); writer.Write(rate * 2);
                    writer.Write((short)2); writer.Write((short)16);
                    writer.Write(System.Text.Encoding.ASCII.GetBytes("data")); writer.Write(count * 2);
                    for (int i = 0; i < count; i++)
                    {
                        double time = i / (double)rate;
                        double envelope = System.Math.Min(1, i / 30.0) * System.Math.Exp(-time * 180);
                        double signal = (random.NextDouble() * 2 - 1) * .65 +
                            System.Math.Sin(2 * System.Math.PI * 1800 * time) * .35;
                        writer.Write((short)(signal * envelope * 20000));
                    }
                }
            }
            AssetDatabase.ImportAsset(soundPath, ImportAssetOptions.ForceSynchronousImport);
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(soundPath);
            if (clip == null) throw new IOException("Could not import dialogue typing sound.");
            var settings = ScriptableObject.CreateInstance<DialogueSettings>();
            settings.typingClip = clip;
            AssetDatabase.CreateAsset(settings, path);
            AssetDatabase.SaveAssets();
        }

        static void EnsureBranchSample()
        {
            var path = ConfigFolder + "/DialogueBranchConfig.asset";
            if (AssetDatabase.LoadAssetAtPath<DialogueConfig>(path) != null) return;
            var json = File.ReadAllText(Root + "/Resources/Data/JsonData/DialogueBranchData.json");
            var data = JsonUtility.FromJson<DialogueData>(json);
            var guide = Portrait("DialogueGuide", new Color32(72, 157, 207, 255));
            var traveler = Portrait("DialogueTraveler", new Color32(220, 149, 75, 255));
            var config = ScriptableObject.CreateInstance<DialogueConfig>();
            config.lines = new DialogueConfigLine[data.lines.Length];
            for (int i = 0; i < data.lines.Length; i++)
            {
                var line = data.lines[i];
                config.lines[i] = new DialogueConfigLine { id = line.id, nextId = line.nextId,
                    end = line.end, choices = DialogueConfigChoice.FromData(line.choices), speaker = line.speaker, text = line.text,
                    sprite = line.spriteKey == "DialogueGuide" ? guide : traveler };
            }
            AssetDatabase.CreateAsset(config, path);
            AssetDatabase.SaveAssets();
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = path.Substring(0, path.LastIndexOf('/'));
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
