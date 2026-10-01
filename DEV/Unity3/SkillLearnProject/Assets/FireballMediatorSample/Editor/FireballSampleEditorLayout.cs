using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace FireballMediatorSample.Editor
{
    public sealed class FireballSampleEditorLayout : EditorWindow
    {
        const string Root = "Assets/FireballMediatorSample";
        float cooldown = 1.2f, manaCost = 20, damage = 25, speed = 12, regeneration = 8;
        string status = "Generate assets and a playable 2D Fireball sample scene.";
        SceneAsset lastScene;
        Vector2 scroll;

        [MenuItem("Tools/Skill Manager/Fireball Sample 2D")]
        public static void Open()
        {
            var window = GetWindow<FireballSampleEditorLayout>("Fireball Sample 2D");
            window.minSize = new Vector2(440, 400);
        }

        void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.LabelField("2D / NEW INPUT SYSTEM / SKILL MANAGER", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Space / Gamepad South → performed → UseSkill(0) → CanUse → Cast → OnSkillUsed\nXY plane, sprites and Physics2D. Focus Game and fire right (+X) at the target.", MessageType.Info);
            cooldown = EditorGUILayout.Slider("Cooldown (seconds)", cooldown, 0.1f, 5);
            manaCost = EditorGUILayout.Slider("Mana cost", manaCost, 0, 100);
            regeneration = EditorGUILayout.Slider("Mana regeneration / second", regeneration, 0, 30);
            damage = EditorGUILayout.Slider("Damage", damage, 1, 100);
            speed = EditorGUILayout.Slider("Projectile speed", speed, 3, 30);
            using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling))
            {
                if (GUILayout.Button("Add Hotbar To Current Scene"))
                {
                    try { FireballHotbarInstaller.InstallCurrentScene(); status = "Hotbar added. Save the current scene to keep it."; }
                    catch (Exception exception) { status = exception.Message; Debug.LogException(exception); }
                }
                if (GUILayout.Button("Create 2D Fireball Sample Scene", GUILayout.Height(36)))
                {
                    try
                    {
                        string path = Generate();
                        if (!string.IsNullOrEmpty(path))
                        {
                            lastScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
                            status = "Created: " + path + "\nPress Play, focus Game, then press Space.";
                            Selection.activeObject = lastScene;
                        }
                    }
                    catch (Exception exception) { status = exception.Message; Debug.LogException(exception); }
                }
                using (new EditorGUI.DisabledScope(lastScene == null))
                    if (GUILayout.Button("Open Generated Scene") && EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                        EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(lastScene));
            }
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Each run creates a unique Generated folder. Existing assets are preserved. Scene, prefab, and ScriptableObject serialization is handled by Unity APIs.", MessageType.None);
            EditorGUILayout.SelectableLabel(status, EditorStyles.textArea, GUILayout.MinHeight(75));
            EditorGUILayout.EndScrollView();
        }

        public string Generate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play mode before generating a sample.");
#if !ENABLE_INPUT_SYSTEM
            throw new InvalidOperationException("Enable Input System Package (New) or Both in Player Settings / Active Input Handling, then restart Unity.");
#else
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return null;
            Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            if (shader == null) throw new InvalidOperationException("No supported sprite shader found.");

            string folder = AssetDatabase.GenerateUniqueAssetPath(Root + "/Generated2D");
            AssetDatabase.CreateFolder(Root, Path.GetFileName(folder));
            var spriteMaterial = Material(folder, shader, "SpriteUnlit", Color.white);
            var square = CreateSprite(folder, "Square", false);
            var circle = CreateSprite(folder, "Circle", true);

            var inputAsset = ScriptableObject.CreateInstance<InputActionAsset>();
            string inputPath = folder + "/FireballControls.inputactions";
            try
            {
                inputAsset.name = "FireballControls";
                var map = inputAsset.AddActionMap("Gameplay");
                var action = map.AddAction("Fireball", InputActionType.Button);
                action.AddBinding("<Keyboard>/space");
                action.AddBinding("<Gamepad>/buttonSouth");
                File.WriteAllText(inputPath, inputAsset.ToJson());
            }
            finally { DestroyImmediate(inputAsset); }
            AssetDatabase.ImportAsset(inputPath, ImportAssetOptions.ForceSynchronousImport);
            var controls = AssetDatabase.LoadAssetAtPath<InputActionAsset>(inputPath);
            if (controls == null) throw new InvalidOperationException("Input Action import failed.");
            var sound = CreateSound(folder);

            // The save prompt above protects the current scene before switching to a new one.
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            string scenePath = folder + "/FireballSample2D.unity";
            try
            {
                var projectileObject = SpriteObject(scene, "Fireball", circle, Vector3.zero,
                    new Vector2(0.44f, 0.44f), new Color(1, 0.3f, 0.025f), spriteMaterial, 5);
                projectileObject.AddComponent<FireballProjectile>();
                var core = SpriteObject(scene, "Hot Core", circle, Vector3.zero,
                    new Vector2(0.55f, 0.55f), new Color(1, 0.9f, 0.35f), spriteMaterial, 6);
                core.transform.SetParent(projectileObject.transform, false);
                var prefab = PrefabUtility.SaveAsPrefabAsset(projectileObject, folder + "/Fireball.prefab");
                DestroyImmediate(projectileObject);

                var data = ScriptableObject.CreateInstance<FireballSkillData>();
                data.cooldown = cooldown; data.manaCost = manaCost; data.damage = damage;
                data.speed = speed; data.lifetime = 6; data.castSound = sound;
                data.projectilePrefab = prefab.GetComponent<FireballProjectile>();
                AssetDatabase.CreateAsset(data, folder + "/FireballSkill.asset");

                SpriteObject(scene, "Arena Backdrop", square, Vector3.zero, new Vector2(24, 14),
                    new Color(0.055f, 0.08f, 0.12f), spriteMaterial, -10);
                SpriteObject(scene, "Ground Stripe", square, new Vector3(0, -2.1f, 0), new Vector2(18, 0.12f),
                    new Color(0.15f, 0.25f, 0.35f), spriteMaterial, -5);
                var caster = SpriteObject(scene, "Caster", circle, new Vector3(-6, -1, 0), new Vector2(1, 2),
                    new Color(0.2f, 0.7f, 1), spriteMaterial);
                var origin = NewObject(scene, "Cast Origin");
                origin.transform.SetParent(caster.transform, false);
                origin.transform.localPosition = new Vector3(0.9f, 0, 0);
                var targetObject = SpriteObject(scene, "Training Target", square, new Vector3(6, -1, 0), new Vector2(1.5f, 2),
                    new Color(0.65f, 0.3f, 0.75f), spriteMaterial);
                targetObject.AddComponent<BoxCollider2D>();
                var dummy = targetObject.AddComponent<FireballTarget>();

                var systems = NewObject(scene, "Skill Manager");
                var audio = systems.AddComponent<AudioSource>();
                audio.playOnAwake = false; audio.spatialBlend = 0;
                var manager = systems.AddComponent<SkillManager>();
                manager.Configure(data, origin.transform, audio, 100, regeneration);
                caster.AddComponent<PlayerSkillInput>().Configure(manager, controls);
                NewObject(scene, "Event Subscriber - HUD").AddComponent<FireballSampleHUD>().Configure(manager, dummy);
                FireballHotbarInstaller.Install(scene, manager, caster.GetComponent<PlayerSkillInput>());

                var cameraObject = NewObject(scene, "Main Camera");
                cameraObject.tag = "MainCamera";
                var camera = cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent<AudioListener>();
                camera.transform.position = new Vector3(0, 0, -10);
                camera.transform.rotation = Quaternion.identity;
                camera.orthographic = true; camera.orthographicSize = 8;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.02f, 0.03f, 0.05f);
                if (!EditorSceneManager.SaveScene(scene, scenePath))
                    throw new IOException("Unity could not save the generated scene.");
                AssetDatabase.SaveAssets();
                if (SceneView.lastActiveSceneView != null)
                {
                    SceneView.lastActiveSceneView.in2DMode = true;
                    SceneView.lastActiveSceneView.LookAt(Vector3.zero, Quaternion.identity, 12, true);
                }
            }
            catch
            {
                // Leave partial output available for inspection rather than deleting user-visible assets.
                EditorSceneManager.MarkSceneDirty(scene);
                throw;
            }
            return scenePath;
#endif
        }

        static GameObject NewObject(Scene scene, string name)
        {
            var go = new GameObject(name);
            SceneManager.MoveGameObjectToScene(go, scene);
            return go;
        }

        static GameObject SpriteObject(Scene scene, string name, Sprite sprite, Vector3 position,
            Vector2 size, Color color, Material material, int order = 0)
        {
            var go = NewObject(scene, name);
            go.transform.position = position; go.transform.localScale = new Vector3(size.x, size.y, 1);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite; renderer.color = color;
            renderer.sharedMaterial = material; renderer.sortingOrder = order;
            return go;
        }

        static Sprite CreateSprite(string folder, string name, bool circle)
        {
            const int size = 64;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            string path = folder + "/" + name + ".png";
            try
            {
                var pixels = new Color[size * size];
                for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                    {
                        float radius = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), Vector2.one * size * 0.5f);
                        pixels[y * size + x] = new Color(1, 1, 1, circle ? Mathf.Clamp01(size * 0.5f - radius) : 1);
                    }
                texture.SetPixels(pixels); texture.Apply();
                File.WriteAllBytes(path, texture.EncodeToPNG());
            }
            finally { DestroyImmediate(texture); }
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = size;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        static Material Material(string folder, Shader shader, string name, Color color)
        {
            var material = new Material(shader) { name = name, color = color };
            AssetDatabase.CreateAsset(material, folder + "/" + name + ".mat");
            return material;
        }

        static AudioClip CreateSound(string folder)
        {
            const int rate = 22050, count = 6615;
            string path = folder + "/FireballCast.wav";
            using (var writer = new BinaryWriter(File.Create(path)))
            {
                writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); writer.Write(36 + count * 2);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt ")); writer.Write(16);
                writer.Write((short)1); writer.Write((short)1); writer.Write(rate); writer.Write(rate * 2);
                writer.Write((short)2); writer.Write((short)16);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("data")); writer.Write(count * 2);
                for (int i = 0; i < count; i++)
                {
                    float t = i / (float)rate;
                    float envelope = Mathf.Sin(Mathf.PI * i / count) * Mathf.Exp(-10 * t);
                    writer.Write((short)(Mathf.Sin(2 * Mathf.PI * (620 * t - 700 * t * t)) * envelope * 16000));
                }
            }
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }
    }
}
