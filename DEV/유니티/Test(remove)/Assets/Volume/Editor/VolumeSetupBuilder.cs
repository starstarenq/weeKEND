using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace VolumeSetup.EditorTools
{
    /// <summary>
    /// Global Volume + VolumeProfile(포스트 프로세싱) 및 SSAO Renderer Feature 를
    /// Unity API 로만 구성한다. (YAML/GUID 직접 편집·참조 없음)
    /// </summary>
    public static class VolumeSetupBuilder
    {
        // ============================================================
        //  진입점
        // ============================================================
        public static void Apply(VolumeQualitySettings s)
        {
            EnsureFolders(s.rootFolder);

            // 1) VolumeProfile 준비 + 오버라이드 구성
            var profile = GetOrCreateProfile(s.rootFolder, s.profileName);
            ConfigureProfile(profile, s);
            EditorUtility.SetDirty(profile);

            // 2) 씬에 Global Volume 배치
            if (s.addToCurrentScene)
            {
                var vol = GetOrCreateGlobalVolume(s.volumeName, profile, s.volumePriority);
                EditorSceneManager.MarkSceneDirty(vol.gameObject.scene);
            }

            // 3) SSAO Renderer Feature
            if (s.enableSSAO)
            {
                string msg = ConfigureSSAO(s);
                Debug.Log("[VolumeSetup] SSAO: " + msg);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[VolumeSetup] 완료. VolumeProfile: " + AssetDatabase.GetAssetPath(profile));
        }

        // ============================================================
        //  VolumeProfile / 오버라이드
        // ============================================================
        static VolumeProfile GetOrCreateProfile(string root, string name)
        {
            string path = $"{root}/Profiles/{name}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            if (existing != null) return existing;

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, path);
            return profile;
        }

        static void ConfigureProfile(VolumeProfile profile, VolumeQualitySettings s)
        {
            // Tonemapping
            if (s.enableTonemapping)
            {
                var t = GetOrAdd<Tonemapping>(profile);
                t.active = true;
                Set(t.mode, s.tonemappingMode);
            }

            // Bloom
            if (s.enableBloom)
            {
                var b = GetOrAdd<Bloom>(profile);
                b.active = true;
                Set(b.threshold, s.bloomThreshold);
                Set(b.intensity, s.bloomIntensity);
                Set(b.scatter, s.bloomScatter);
                Set(b.tint, s.bloomTint);
                Set(b.highQualityFiltering, s.bloomHighQuality);
            }

            // Color Adjustments
            if (s.enableColorAdjustments)
            {
                var c = GetOrAdd<ColorAdjustments>(profile);
                c.active = true;
                Set(c.postExposure, s.postExposure);
                Set(c.contrast, s.contrast);
                Set(c.saturation, s.saturation);
                Set(c.colorFilter, s.colorFilter);
            }

            // Vignette
            if (s.enableVignette)
            {
                var v = GetOrAdd<Vignette>(profile);
                v.active = true;
                Set(v.intensity, s.vignetteIntensity);
                Set(v.smoothness, s.vignetteSmoothness);
                Set(v.color, s.vignetteColor);
            }
        }

        static T GetOrAdd<T>(VolumeProfile profile) where T : VolumeComponent
        {
            if (profile.TryGet<T>(out var comp)) return comp;
            return profile.Add<T>(true);
        }

        /// <summary>VolumeParameter 값 설정 + overrideState 활성화.</summary>
        static void Set<T>(VolumeParameter<T> param, T value)
        {
            if (param == null) return;
            param.overrideState = true;
            param.value = value;
        }

        // ============================================================
        //  Global Volume 오브젝트
        // ============================================================
        static Volume GetOrCreateGlobalVolume(string name, VolumeProfile profile, int priority)
        {
            Volume vol = null;
            foreach (var v in Object.FindObjectsByType<Volume>(FindObjectsSortMode.None))
            {
                if (v.isGlobal && v.gameObject.name == name) { vol = v; break; }
            }
            if (vol == null)
            {
                var go = new GameObject(name);
                vol = go.AddComponent<Volume>();
                Undo.RegisterCreatedObjectUndo(go, "Create Global Volume");
            }
            vol.isGlobal = true;
            vol.priority = priority;
            vol.sharedProfile = profile;
            EditorUtility.SetDirty(vol);
            return vol;
        }

        // ============================================================
        //  SSAO Renderer Feature
        // ============================================================
        static string ConfigureSSAO(VolumeQualitySettings s)
        {
            try
            {
                var urp = GetActiveURPAsset();
                if (urp == null)
                    return "활성 URP 에셋을 찾지 못했습니다. (Graphics/Quality 설정 확인)";

                var urd = GetUniversalRendererData(urp);
                if (urd == null)
                    return "Universal Renderer(3D) 를 찾지 못했습니다. (현재 렌더러가 2D Renderer 일 수 있음) " +
                           "→ 3D Universal Renderer 에서 수동으로 SSAO 를 추가하세요.";

                // 기존 SSAO 탐색
                ScreenSpaceAmbientOcclusion ssao = null;
                foreach (var f in urd.rendererFeatures)
                    if (f is ScreenSpaceAmbientOcclusion existing) { ssao = existing; break; }

                bool added = false;
                if (ssao == null)
                {
                    ssao = ScriptableObject.CreateInstance<ScreenSpaceAmbientOcclusion>();
                    ssao.name = "ScreenSpaceAmbientOcclusion";
                    AssetDatabase.AddObjectToAsset(ssao, urd);
                    urd.rendererFeatures.Add(ssao);
                    added = true;
                }

                ssao.SetActive(true);
                ApplySSAOSettings(ssao, s);

                EditorUtility.SetDirty(urd);
                AssetDatabase.SaveAssets();

                // 내부 feature map 재생성을 위해 재임포트(OnValidate 유도)
                string path = AssetDatabase.GetAssetPath(urd);
                if (!string.IsNullOrEmpty(path))
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

                return added
                    ? $"'{urd.name}' 에 SSAO 를 추가/활성화했습니다."
                    : $"'{urd.name}' 의 기존 SSAO 를 갱신했습니다.";
            }
            catch (System.Exception e)
            {
                return "자동 추가 실패 → 렌더러 에셋에서 [Add Renderer Feature ▸ Screen Space Ambient Occlusion] 로 " +
                       "수동 추가하세요. 원인: " + e.Message;
            }
        }

        static void ApplySSAOSettings(ScreenSpaceAmbientOcclusion ssao, VolumeQualitySettings s)
        {
            // m_Settings 는 internal 타입이라 리플렉션으로 값 설정 (실패해도 기본값 유지)
            try
            {
                var field = typeof(ScreenSpaceAmbientOcclusion)
                    .GetField("m_Settings", BindingFlags.NonPublic | BindingFlags.Instance);
                var settings = field?.GetValue(ssao);
                if (settings == null) return;

                var st = settings.GetType();
                SetField(st, settings, "Intensity", s.ssaoIntensity);
                SetField(st, settings, "Radius", s.ssaoRadius);
                SetField(st, settings, "DirectLightingStrength", s.ssaoDirectLightingStrength);
            }
            catch { /* 기본값 사용 */ }
        }

        static void SetField(System.Type t, object obj, string name, object value)
        {
            var f = t.GetField(name, BindingFlags.Public | BindingFlags.Instance);
            if (f != null && f.FieldType == value.GetType()) f.SetValue(obj, value);
        }

        // ============================================================
        //  URP 에셋 / 렌더러 조회 (경로/GUID 아님, 런타임 API·리플렉션)
        // ============================================================
        static UniversalRenderPipelineAsset GetActiveURPAsset()
        {
            var rp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (rp == null) rp = QualitySettings.renderPipeline as UniversalRenderPipelineAsset;
            if (rp == null) rp = GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset;
            return rp;
        }

        static UniversalRendererData GetUniversalRendererData(UniversalRenderPipelineAsset urp)
        {
            var field = typeof(UniversalRenderPipelineAsset)
                .GetField("m_RendererDataList", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field?.GetValue(urp) is ScriptableRendererData[] list)
            {
                foreach (var d in list)
                    if (d is UniversalRendererData urd) return urd;
            }
            return null;
        }

        // ============================================================
        //  폴더 유틸
        // ============================================================
        static readonly string[] SubFolders = { "Editor", "Profiles" };

        public static void EnsureFolders(string root)
        {
            EnsureFolder(root);
            foreach (var f in SubFolders) EnsureFolder($"{root}/{f}");
            AssetDatabase.Refresh();
        }

        static void EnsureFolder(string path)
        {
            path = path.Replace('\\', '/').TrimEnd('/');
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            string name = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
                EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
