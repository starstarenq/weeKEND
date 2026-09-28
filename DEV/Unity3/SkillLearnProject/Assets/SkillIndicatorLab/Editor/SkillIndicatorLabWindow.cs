using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace SysKill.SkillIndicators.Editor
{
    public sealed class SkillIndicatorLabWindow : EditorWindow
    {
        // Resolve from the preserved script GUID so exports can live anywhere under Assets.
        public static string Root
        {
            get
            {
                string scriptPath = AssetDatabase.GUIDToAssetPath("b337515d44e86bb498e30e4d904081be");
                if (string.IsNullOrEmpty(scriptPath))
                    throw new InvalidOperationException("SkillIndicatorLabWindow.cs.meta is missing. Import the complete SkillIndicatorLab folder including its .meta files.");
                return Path.GetDirectoryName(Path.GetDirectoryName(scriptPath)).Replace('\\', '/') + "/Generated";
            }
        }
        public static string ScenePath => Root + "/Scenes/SkillIndicatorLab.unity";
        static readonly string[] Names = { "Circle", "Rectangle", "Sector", "Donut" };
        static readonly Color[] Colors = {
            new Color(0.15f, 0.78f, 1, 1), new Color(1, 0.58f, 0.12f, 1),
            new Color(1, 0.2f, 0.36f, 1), new Color(0.5f, 0.38f, 1, 1)
        };
        float radius = 2, width = 2.2f, length = 4, angle = 105, inner = 0.55f, charge = 2.4f;
        string status = "Create four Shader Graphs, reusable prefabs, and a demonstration scene.";
        Vector2 scroll;

        [MenuItem("Tools/Skill Indicators/Shader Graph Lab")]
        public static void Open()
        {
            var window = GetWindow<SkillIndicatorLabWindow>("Skill Indicator Lab");
            window.minSize = new Vector2(420, 420);
        }

        void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.LabelField("SHADER GRAPH / SKILL INDICATORS", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Circle · impact at target\nRectangle · charge and dash\nSector · melee arc\nDonut · persistent area with a safe center", MessageType.Info);
            radius = EditorGUILayout.Slider("Radius", radius, 0.5f, 2.4f);
            width = EditorGUILayout.Slider("Dash width", width, 0.5f, 4);
            length = EditorGUILayout.Slider("Dash length", length, 1, 4.5f);
            angle = EditorGUILayout.Slider("Melee angle", angle, 15, 300);
            inner = EditorGUILayout.Slider("Donut inner / outer radius", inner, 0.1f, 0.85f);
            charge = EditorGUILayout.Slider("Charge duration", charge, 0.2f, 6);
            using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling))
            {
                if (GUILayout.Button("Create New Lab Scene", GUILayout.Height(34)))
                {
                    try { status = Generate(); }
                    catch (Exception e) { status = e.ToString(); Debug.LogException(e); }
                }
                if (GUILayout.Button("Open First Generated Scene"))
                {
                    if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null &&
                        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                }
            }
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Each generation uses a new folder when a lab already exists. Existing scenes and assets are preserved. Disable SkillIndicatorDemo to control a scene indicator from combat code.", MessageType.None);
            EditorGUILayout.SelectableLabel(status, EditorStyles.textArea, GUILayout.MinHeight(95));
            EditorGUILayout.EndScrollView();
        }

        public string Generate()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (string.IsNullOrEmpty(SceneManager.GetSceneAt(i).path))
                    throw new InvalidOperationException("Save the untitled open scene before creating the lab. Unity requires this for additive scene creation.");
            if (!(GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset))
                throw new InvalidOperationException("An active Universal Render Pipeline is required.");
            string root = AssetDatabase.IsValidFolder(Root) ? AssetDatabase.GenerateUniqueAssetPath(Root) : Root;
            Folder(root + "/Graphs"); Folder(root + "/Materials"); Folder(root + "/Prefabs"); Folder(root + "/Scenes");
            Material[] materials = new Material[4];
            for (int i = 0; i < 4; i++)
            {
                Shader shader = SkillIndicatorGraphBuilder.Create(root + "/Graphs/SG_" + Names[i] + ".shadergraph", i, Colors[i]);
                materials[i] = new Material(shader) { name = "M_" + Names[i] };
                AssetDatabase.CreateAsset(materials[i], root + "/Materials/M_" + Names[i] + ".mat");
            }
            Material floor = Solid(root, "Floor", new Color(0.025f, 0.035f, 0.055f));
            Material panel = Solid(root, "Panel", new Color(0.055f, 0.075f, 0.105f));
            Material actorMaterial = Solid(root, "Actor", new Color(0.7f, 0.78f, 0.87f));
            Scene previous = SceneManager.GetActiveScene();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            try
            {
                SceneManager.SetActiveScene(scene);
                Cube("Backdrop", new Vector3(0, -0.25f, 0), new Vector3(18, 0.3f, 16), floor);
                Camera camera = new GameObject("Lab Camera", typeof(Camera), typeof(UniversalAdditionalCameraData)).GetComponent<Camera>();
                camera.transform.position = new Vector3(0, 25, 0);
                camera.transform.rotation = Quaternion.Euler(90, 0, 0);
                camera.orthographic = true; camera.orthographicSize = 8.6f;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.018f, 0.025f, 0.04f);
                camera.nearClipPlane = 0.1f; camera.farClipPlane = 60;
                camera.GetUniversalAdditionalCameraData().renderPostProcessing = false;
                Label("SKILL / TELEGRAPH LAB", new Vector3(-7.5f, 0.06f, 7), 0.34f, Color.white);
                Label("SHADER GRAPH     /     4 SHAPES     /     PLAY TO PREVIEW", new Vector3(-7.5f, 0.06f, 6.4f), 0.14f, new Color(0.5f, 0.65f, 0.8f));
                Vector3[] centers = { new Vector3(-4, 0, 3), new Vector3(4, 0, 3), new Vector3(-4, 0, -3.5f), new Vector3(4, 0, -3.5f) };
                string[] descriptions = { "TARGET IMPACT", "CHARGE > DASH", "MELEE / FORWARD ARC", "AREA / SAFE CENTER" };
                for (int i = 0; i < 4; i++)
                {
                    Vector3 center = centers[i];
                    Cube(Names[i] + " Platform", center + Vector3.down * 0.08f, new Vector3(7.5f, 0.12f, 6.1f), panel);
                    Material accent = Solid(root, Names[i] + " Accent", Colors[i]);
                    Cube(Names[i] + " Accent Rail", center + new Vector3(-3.55f, 0.01f, 0), new Vector3(0.035f, 0.01f, 5.6f), accent);
                    Label("0" + (i + 1) + "  /  " + Names[i].ToUpperInvariant(), center + new Vector3(-3.15f, 0.06f, 2.75f), 0.23f, Colors[i]);
                    Label(descriptions[i], center + new Vector3(-3.15f, 0.06f, -2.55f), 0.15f, new Color(0.65f, 0.72f, 0.8f));
                    GameObject go = new GameObject(Names[i] + " Indicator");
                    var indicator = go.AddComponent<SkillIndicator>();
                    indicator.shape = (SkillIndicator.Shape)i;
                    indicator.radius = radius; indicator.width = width; indicator.length = length;
                    indicator.angle = angle; indicator.innerRadiusRatio = inner; indicator.tint = Colors[i];
                    GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    quad.name = "Shader Graph Surface"; quad.transform.SetParent(go.transform, false);
                    DestroyImmediate(quad.GetComponent<Collider>());
                    indicator.surface = quad.GetComponent<Renderer>();
                    indicator.surface.sharedMaterial = materials[i];
                    indicator.surface.shadowCastingMode = ShadowCastingMode.Off;
                    indicator.surface.receiveShadows = false;
                    indicator.Apply();
                    PrefabUtility.SaveAsPrefabAsset(go, root + "/Prefabs/" + Names[i] + "Indicator.prefab");
                    go.transform.position = center + (i == 1 ? new Vector3(0, 0, -length * 0.5f) : i == 2 ? new Vector3(0, 0, -0.8f) : Vector3.zero);
                    var demo = go.AddComponent<SkillIndicatorDemo>();
                    demo.indicator = indicator; demo.chargeSeconds = charge;
                    demo.activeSeconds = i == 3 ? 3 : 0.65f;
                    demo.timeOffset = i * 0.3f;
                    if (i == 1 || i == 2)
                    {
                        GameObject actor = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                        actor.name = "Demo Attacker"; actor.transform.SetParent(go.transform, false);
                        actor.transform.localPosition = new Vector3(0, 0.45f, -0.35f);
                        actor.transform.localScale = new Vector3(0.35f, 0.45f, 0.35f);
                        actor.GetComponent<Renderer>().sharedMaterial = actorMaterial;
                        DestroyImmediate(actor.GetComponent<Collider>());
                        demo.actor = actor.transform;
                    }
                    if (i == 0)
                    {
                        GameObject hit = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        hit.name = "Target Impact Burst"; hit.transform.SetParent(go.transform, false);
                        hit.transform.localPosition = new Vector3(0, 0.12f, 0);
                        hit.GetComponent<Renderer>().sharedMaterial = accent;
                        DestroyImmediate(hit.GetComponent<Collider>());
                        hit.SetActive(false); demo.hitEffect = hit.transform;
                    }
                }
                string scenePath = root + "/Scenes/SkillIndicatorLab.unity";
                if (!EditorSceneManager.SaveScene(scene, scenePath)) throw new IOException("Scene save failed.");
                AssetDatabase.SaveAssets();
                Validate(scene, materials);
                Capture(camera, root + "/Preview.png");
                Validate(scene, materials);
                Debug.Log("[SkillIndicatorLab] SUCCESS: " + scenePath + " | 4 graphs, 4 prefabs; shader and shape checks passed.");
                return scenePath;
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
                if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
            }
        }

        static void Validate(Scene scene, Material[] materials)
        {
            foreach (Material material in materials)
            {
                if (ShaderUtil.ShaderHasError(material.shader))
                    throw new InvalidOperationException("Shader errors: " + material.name + "\n" + string.Join("\n", ShaderUtil.GetShaderMessages(material.shader).Select(m => m.message)));
                foreach (string property in new[] { "_Shape", "_Progress", "_Tint", "_Inner", "_Angle", "_Border", "_Impact", "_Opacity" })
                    if (!material.HasProperty(property)) throw new InvalidOperationException("Missing shader property " + property);
            }
            var indicators = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<SkillIndicator>()).ToArray();
            if (indicators.Length != 4) throw new InvalidOperationException("Expected four indicators.");
            foreach (var indicator in indicators)
            {
                bool center = indicator.ContainsGroundPoint(indicator.transform.position);
                if (center == (indicator.shape == SkillIndicator.Shape.Donut)) throw new InvalidOperationException("Center shape check failed.");
                if (indicator.ContainsGroundPoint(indicator.transform.TransformPoint(new Vector3(20, 0, 20)))) throw new InvalidOperationException("Outside shape check failed.");
                if (indicator.shape == SkillIndicator.Shape.Sector && indicator.ContainsGroundPoint(indicator.transform.TransformPoint(new Vector3(0, 0, -1)))) throw new InvalidOperationException("Sector direction failed.");
            }
        }

        static void Capture(Camera camera, string path)
        {
            var rt = new RenderTexture(1440, 1440, 24);
            var texture = new Texture2D(1440, 1440, TextureFormat.RGB24, false);
            RenderTexture previous = RenderTexture.active;
            bool asyncCompilation = ShaderUtil.allowAsyncCompilation;
            try
            {
                ShaderUtil.allowAsyncCompilation = false;
                camera.targetTexture = rt;
                camera.Render(); RenderTexture.active = rt;
                texture.ReadPixels(new Rect(0, 0, 1440, 1440), 0, 0); texture.Apply();
                File.WriteAllBytes(path, texture.EncodeToPNG());
            }
            finally { ShaderUtil.allowAsyncCompilation = asyncCompilation; camera.targetTexture = null; RenderTexture.active = previous; rt.Release(); DestroyImmediate(rt); DestroyImmediate(texture); }
            AssetDatabase.ImportAsset(path);
        }
        static Material Solid(string root, string name, Color color)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Unlit")) { name = name };
            material.SetColor("_BaseColor", color);
            AssetDatabase.CreateAsset(material, root + "/Materials/" + name + ".mat");
            return material;
        }
        static void Cube(string name, Vector3 position, Vector3 scale, Material material)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name; go.transform.position = position; go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
        }
        static void Label(string text, Vector3 position, float size, Color color)
        {
            var label = new GameObject(text, typeof(TextMesh)).GetComponent<TextMesh>();
            label.transform.position = position; label.transform.rotation = Quaternion.Euler(90, 0, 0);
            label.text = text; label.fontSize = 64; label.characterSize = size * 0.2f;
            label.anchor = TextAnchor.UpperLeft; label.color = color;
        }
        static void Folder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            int split = path.LastIndexOf('/');
            Folder(path.Substring(0, split));
            AssetDatabase.CreateFolder(path.Substring(0, split), path.Substring(split + 1));
        }
    }

}
