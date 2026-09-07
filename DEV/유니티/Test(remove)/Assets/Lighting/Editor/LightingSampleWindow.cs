using UnityEditor;
using UnityEngine;

namespace LightingSample.EditorTools
{
    /// <summary>
    /// 광원 실습 샘플 생성기 EditorWindow.
    /// 메뉴: Tools ▸ Lighting Sample ▸ Generator
    /// </summary>
    public class LightingSampleWindow : EditorWindow
    {
        [SerializeField] private LightingSampleSettings settings = new LightingSampleSettings();
        private SerializedObject so;
        private Vector2 scroll;
        private bool fOut = true, fParts = true, fSpace = false, fLight = false, fProbe = false, fBake = false;

        [MenuItem("Tools/Lighting Sample/Generator")]
        public static void Open()
        {
            var win = GetWindow<LightingSampleWindow>("Lighting Sample");
            win.minSize = new Vector2(360, 460);
            win.Show();
        }

        private void OnEnable() => so = new SerializedObject(this);

        private void OnGUI()
        {
            so.Update();
            var p = so.FindProperty("settings");
            scroll = EditorGUILayout.BeginScrollView(scroll);

            EditorGUILayout.HelpBox(
                "Unity API 로만 씬/에셋을 생성합니다 (YAML·GUID 직접 편집 없음).\n" +
                "생성 후 Light Probe 를 보려면 반드시 'Bake Lighting' 을 실행하세요.",
                MessageType.Info);

            fOut = Section(fOut, "출력 (Output)");
            if (fOut)
            {
                Prop(p, "rootFolder", "Root Folder");
                Prop(p, "sampleName", "Sample Name");
                Prop(p, "createNewScene", "Create New Scene");
            }

            fParts = Section(fParts, "실습 구성 요소");
            if (fParts)
            {
                Prop(p, "buildOutdoor", "① 외부 광원 (야외+태양)");
                Prop(p, "buildRoom", "② 차폐된 내부 공간");
                Prop(p, "buildLightProbes", "③ Light Probe 배치");
                Prop(p, "buildComparison", "④ Probe On/Off 비교");
                Prop(p, "setupLightingSettings", "베이크 설정 생성");
            }

            fSpace = Section(fSpace, "공간 크기");
            if (fSpace)
            {
                Prop(p, "groundSize", "Ground Size");
                Prop(p, "roomInteriorSize", "Room Interior Size");
                Prop(p, "roomCenter", "Room Center");
                Prop(p, "wallThickness", "Wall Thickness");
                Prop(p, "doorWidth", "Door Width");
                Prop(p, "doorHeight", "Door Height");
            }

            fLight = Section(fLight, "조명");
            if (fLight)
            {
                Prop(p, "sunColor", "Sun Color");
                Prop(p, "sunIntensity", "Sun Intensity");
                Prop(p, "sunEuler", "Sun Rotation (Euler)");
                Prop(p, "interiorLightColor", "Interior Light Color");
                Prop(p, "interiorLightIntensity", "Interior Intensity");
                Prop(p, "interiorLightRange", "Interior Range");
            }

            fProbe = Section(fProbe, "Light Probe / 비교 구체");
            if (fProbe)
            {
                Prop(p, "probeSpacing", "Probe Spacing");
                Prop(p, "probeHeightLow", "Probe Height (Low)");
                Prop(p, "probeHeightHigh", "Probe Height (High)");
                Prop(p, "spherePerRow", "Spheres Per Row");
                Prop(p, "sphereRadius", "Sphere Radius");
                Prop(p, "autoToggleInterval", "Auto Toggle Interval");
            }

            fBake = Section(fBake, "베이크 품질");
            if (fBake)
            {
                Prop(p, "lightmapResolution", "Lightmap Resolution");
                Prop(p, "lightmapMaxSize", "Lightmap Max Size");
            }

            so.ApplyModifiedProperties();

            EditorGUILayout.Space(8);

            if (GUILayout.Button("Ensure Folders", GUILayout.Height(22)))
                LightingSampleBuilder.EnsureFolders(settings.rootFolder);

            GUI.backgroundColor = new Color(0.6f, 0.9f, 0.6f);
            if (GUILayout.Button("▶ Build Lighting Sample", GUILayout.Height(34)))
            {
                var go = LightingSampleBuilder.Build(settings);
                if (go != null) Debug.Log($"[LightingSample] 생성 완료: {go.name}");
            }
            GUI.backgroundColor = new Color(0.95f, 0.85f, 0.5f);
            if (GUILayout.Button("💡 Bake Lighting (Generate)", GUILayout.Height(30)))
                LightingSampleBuilder.Bake();
            GUI.backgroundColor = Color.white;

            EditorGUILayout.HelpBox(
                "재생(Play)하면 ④ 구체의 ON 줄이 자동으로 Probe On/Off 를 전환하며 차이를 보여줍니다.",
                MessageType.None);

            EditorGUILayout.EndScrollView();
        }

        private static bool Section(bool state, string title)
        {
            EditorGUILayout.Space(2);
            return EditorGUILayout.Foldout(state, title, true, EditorStyles.foldoutHeader);
        }

        private static void Prop(SerializedProperty parent, string name, string label)
        {
            var pr = parent.FindPropertyRelative(name);
            if (pr != null) EditorGUILayout.PropertyField(pr, new GUIContent(label), true);
        }
    }
}
