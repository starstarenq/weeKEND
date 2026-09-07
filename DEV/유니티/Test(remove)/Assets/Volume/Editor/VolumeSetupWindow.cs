using UnityEditor;
using UnityEngine;

namespace VolumeSetup.EditorTools
{
    /// <summary>
    /// Global Volume + 그래픽 품질(SSAO 등) 세팅 EditorWindow.
    /// 메뉴: Tools ▸ Volume Setup ▸ Global Volume & Quality
    /// </summary>
    public class VolumeSetupWindow : EditorWindow
    {
        [SerializeField] private VolumeQualitySettings settings = new VolumeQualitySettings();
        private SerializedObject so;
        private Vector2 scroll;
        private bool fOut = true, fTone = true, fBloom = true, fColor = false, fVig = false, fSSAO = true;

        [MenuItem("Tools/Volume Setup/Global Volume & Quality")]
        public static void Open()
        {
            var win = GetWindow<VolumeSetupWindow>("Volume Setup");
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
                "현재 씬에 Global Volume 을 추가하고 포스트 프로세싱/SSAO 로 품질을 올립니다.\n" +
                "Unity API 로만 구성합니다 (YAML·GUID 직접 편집 없음).\n" +
                "SSAO 는 URP Renderer Feature 이므로 3D Universal Renderer 에 적용됩니다.",
                MessageType.Info);

            fOut = Section(fOut, "출력 / 대상");
            if (fOut)
            {
                Prop(p, "rootFolder", "Root Folder");
                Prop(p, "profileName", "Profile Name");
                Prop(p, "volumeName", "Volume Object Name");
                Prop(p, "volumePriority", "Volume Priority");
                Prop(p, "addToCurrentScene", "Add To Current Scene");
            }

            fTone = Section(fTone, "Tonemapping");
            if (fTone)
            {
                Prop(p, "enableTonemapping", "Enable");
                Prop(p, "tonemappingMode", "Mode");
            }

            fBloom = Section(fBloom, "Bloom");
            if (fBloom)
            {
                Prop(p, "enableBloom", "Enable");
                Prop(p, "bloomThreshold", "Threshold");
                Prop(p, "bloomIntensity", "Intensity");
                Prop(p, "bloomScatter", "Scatter");
                Prop(p, "bloomTint", "Tint");
                Prop(p, "bloomHighQuality", "High Quality");
            }

            fColor = Section(fColor, "Color Adjustments");
            if (fColor)
            {
                Prop(p, "enableColorAdjustments", "Enable");
                Prop(p, "postExposure", "Post Exposure");
                Prop(p, "contrast", "Contrast");
                Prop(p, "saturation", "Saturation");
                Prop(p, "colorFilter", "Color Filter");
            }

            fVig = Section(fVig, "Vignette");
            if (fVig)
            {
                Prop(p, "enableVignette", "Enable");
                Prop(p, "vignetteIntensity", "Intensity");
                Prop(p, "vignetteSmoothness", "Smoothness");
                Prop(p, "vignetteColor", "Color");
            }

            fSSAO = Section(fSSAO, "SSAO (Renderer Feature)");
            if (fSSAO)
            {
                Prop(p, "enableSSAO", "Enable");
                Prop(p, "ssaoIntensity", "Intensity");
                Prop(p, "ssaoRadius", "Radius");
                Prop(p, "ssaoDirectLightingStrength", "Direct Lighting Strength");
            }

            so.ApplyModifiedProperties();

            EditorGUILayout.Space(8);

            if (GUILayout.Button("Ensure Folders", GUILayout.Height(22)))
                VolumeSetupBuilder.EnsureFolders(settings.rootFolder);

            GUI.backgroundColor = new Color(0.6f, 0.9f, 0.6f);
            if (GUILayout.Button("▶ Apply Volume & Quality", GUILayout.Height(34)))
                VolumeSetupBuilder.Apply(settings);
            GUI.backgroundColor = Color.white;

            EditorGUILayout.HelpBox(
                "SSAO 자동 추가가 실패하면 Console 에 수동 방법이 안내됩니다.\n" +
                "(렌더러 에셋 ▸ Add Renderer Feature ▸ Screen Space Ambient Occlusion)",
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
