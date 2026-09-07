using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace LightingSample.EditorTools
{
    /// <summary>
    /// 광원 실습 샘플 씬을 Unity API 로만 생성한다. (YAML/GUID 직접 편집 없음)
    /// </summary>
    public static class LightingSampleBuilder
    {
        // ============================================================
        //  진입점
        // ============================================================
        public static GameObject Build(LightingSampleSettings s)
        {
            EnsureFolders(s.rootFolder);

            if (s.createNewScene)
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var root = new GameObject(s.sampleName);

            // 카메라 (새 씬이거나 씬에 카메라가 없을 때)
            if (Object.FindFirstObjectByType<Camera>() == null)
                CreateCamera(root, s);

            // 머티리얼
            var matGround = CreateMaterial(s, "GroundMat", new Color(0.45f, 0.47f, 0.42f));
            var matWall   = CreateMaterial(s, "WallMat",   new Color(0.80f, 0.78f, 0.74f));
            var matSphere = CreateMaterial(s, "SphereMat", new Color(0.75f, 0.75f, 0.78f));

            // ① 외부 광원 (지면 + 태양)
            if (s.buildOutdoor)
                BuildOutdoor(root, s, matGround);

            // ② 차폐된 내부 공간
            if (s.buildRoom)
                BuildRoom(root, s, matWall);

            // ③ Light Probe 배치
            if (s.buildLightProbes)
                BuildLightProbes(root, s);

            // ④ Light Probe On/Off 비교
            if (s.buildComparison)
                BuildComparison(root, s, matSphere);

            // 환경 / 베이크 설정
            SetupEnvironment(s);
            if (s.setupLightingSettings)
                SetupLightingSettings(s);

            // 저장
            if (s.createNewScene)
            {
                string scenePath = AssetDatabase.GenerateUniqueAssetPath(
                    $"{s.rootFolder}/Scenes/{s.sampleName}.unity");
                EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), scenePath);
            }

            AssetDatabase.SaveAssets();
            Selection.activeGameObject = root;
            if (SceneView.lastActiveSceneView != null)
                SceneView.lastActiveSceneView.FrameSelected();
            return root;
        }

        // ============================================================
        //  ① 외부 광원
        // ============================================================
        static void BuildOutdoor(GameObject root, LightingSampleSettings s, Material mat)
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground (Outdoor)";
            ground.transform.SetParent(root.transform, true);
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = Vector3.one * (s.groundSize / 10f); // Plane 기본 10 units
            ground.GetComponent<Renderer>().sharedMaterial = mat;
            MarkStaticGI(ground);

            // 태양 (Directional, Mixed) — 외부를 비추고 지붕이 실내를 차폐한다.
            var sunGo = new GameObject("Directional Light (Sun)");
            sunGo.transform.SetParent(root.transform, true);
            sunGo.transform.rotation = Quaternion.Euler(s.sunEuler);
            var sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = s.sunColor;
            sun.intensity = s.sunIntensity;
            sun.shadows = LightShadows.Soft;
            sun.lightmapBakeType = LightmapBakeType.Mixed;
            RenderSettings.sun = sun;
        }

        // ============================================================
        //  ② 차폐된 내부 공간 (방)
        // ============================================================
        static void BuildRoom(GameObject root, LightingSampleSettings s, Material mat)
        {
            var roomRoot = new GameObject("Room (Occluded Interior)");
            roomRoot.transform.SetParent(root.transform, true);

            Vector3 c = s.roomCenter;
            Vector3 inner = s.roomInteriorSize;
            float t = s.wallThickness;
            float hx = inner.x * 0.5f;
            float hz = inner.z * 0.5f;
            float wallZ = inner.z + 2f * t; // 코너를 덮도록

            // 바닥은 외부 지면을 공유. 벽은 지면 위에 세운다.
            // 근접 벽(-X, 야외를 향함): 출입구(door) 를 남기고 좌/우/상인방으로 분할
            float nearX = c.x - hx;
            float halfDoor = s.doorWidth * 0.5f;
            float leftLen  = (hz + t) - halfDoor;
            float leftZc   = -(halfDoor + leftLen * 0.5f);
            CreateBox(roomRoot, "Wall_Near_Left",  new Vector3(nearX, inner.y * 0.5f, leftZc),  new Vector3(t, inner.y, leftLen), mat);
            CreateBox(roomRoot, "Wall_Near_Right", new Vector3(nearX, inner.y * 0.5f, -leftZc), new Vector3(t, inner.y, leftLen), mat);
            // 문 상단 인방(lintel)
            float lintelH = inner.y - s.doorHeight;
            if (lintelH > 0.01f)
                CreateBox(roomRoot, "Wall_Near_Lintel",
                    new Vector3(nearX, s.doorHeight + lintelH * 0.5f, 0f),
                    new Vector3(t, lintelH, s.doorWidth), mat);

            // 나머지 벽
            CreateBox(roomRoot, "Wall_Far",   new Vector3(c.x + hx, inner.y * 0.5f, 0f), new Vector3(t, inner.y, wallZ), mat);
            CreateBox(roomRoot, "Wall_Left",  new Vector3(c.x, inner.y * 0.5f, -hz),     new Vector3(inner.x, inner.y, t), mat);
            CreateBox(roomRoot, "Wall_Right", new Vector3(c.x, inner.y * 0.5f, hz),      new Vector3(inner.x, inner.y, t), mat);
            // 지붕(태양광 차폐)
            CreateBox(roomRoot, "Ceiling", new Vector3(c.x, inner.y + t * 0.5f, 0f),
                new Vector3(inner.x + 2f * t, t, wallZ), mat);

            // 내부 조명 (Point, Mixed) — 차폐된 내부의 '맵 빛'
            var lightGo = new GameObject("Interior Point Light");
            lightGo.transform.SetParent(roomRoot.transform, true);
            lightGo.transform.position = new Vector3(c.x, inner.y * 0.75f, 0f);
            var pl = lightGo.AddComponent<Light>();
            pl.type = LightType.Point;
            pl.color = s.interiorLightColor;
            pl.intensity = s.interiorLightIntensity;
            pl.range = s.interiorLightRange;
            pl.shadows = LightShadows.Soft;
            pl.lightmapBakeType = LightmapBakeType.Mixed;
        }

        // ============================================================
        //  ③ Light Probe 배치
        // ============================================================
        static void BuildLightProbes(GameObject root, LightingSampleSettings s)
        {
            var go = new GameObject("Light Probe Group");
            go.transform.SetParent(root.transform, true);
            go.transform.position = Vector3.zero; // local == world
            var group = go.AddComponent<LightProbeGroup>();

            var positions = new List<Vector3>();
            // 야외(x≈0) → 출입구(x≈5) → 실내(x≈15) 를 관통하는 그리드
            float xStart = 0f;
            float xEnd = s.roomCenter.x + s.roomInteriorSize.x * 0.5f;
            float hz = s.roomInteriorSize.z * 0.5f - 0.5f;
            float step = Mathf.Max(0.5f, s.probeSpacing);
            float[] heights = { s.probeHeightLow, s.probeHeightHigh };

            for (float x = xStart; x <= xEnd + 0.01f; x += step)
                for (float z = -hz; z <= hz + 0.01f; z += step)
                    foreach (float y in heights)
                        positions.Add(new Vector3(x, y, z));

            group.probePositions = positions.ToArray();
        }

        // ============================================================
        //  ④ Light Probe On/Off 비교
        // ============================================================
        static void BuildComparison(GameObject root, LightingSampleSettings s, Material mat)
        {
            var compRoot = new GameObject("LightProbe Comparison");
            compRoot.transform.SetParent(root.transform, true);

            int n = Mathf.Max(1, s.spherePerRow);
            float xStart = 0f;
            float xEnd = s.roomCenter.x + s.roomInteriorSize.x * 0.4f;

            var onRenderers = new List<Renderer>();

            for (int i = 0; i < n; i++)
            {
                float tt = (n == 1) ? 0.5f : i / (float)(n - 1);
                float x = Mathf.Lerp(xStart, xEnd, tt);

                // On 줄 (BlendProbes) — z = +1.2
                var on = CreateSphere(compRoot, $"Sphere_ON_{i}",
                    new Vector3(x, s.probeHeightLow, 1.2f), s.sphereRadius, mat);
                var onR = on.GetComponent<Renderer>();
                onR.lightProbeUsage = LightProbeUsage.BlendProbes;
                onRenderers.Add(onR);

                // Off 줄 — z = -1.2 (동일 위치, 프로브 미사용)
                var off = CreateSphere(compRoot, $"Sphere_OFF_{i}",
                    new Vector3(x, s.probeHeightLow, -1.2f), s.sphereRadius, mat);
                off.GetComponent<Renderer>().lightProbeUsage = LightProbeUsage.Off;
            }

            // 런타임 토글 컨트롤러 (ON 줄을 자동 On/Off)
            var ctrl = compRoot.AddComponent<LightProbeDemoController>();
            ctrl.probeRenderers = onRenderers;
            ctrl.autoToggle = true;
            ctrl.toggleInterval = s.autoToggleInterval;
            ctrl.lightProbesEnabled = true;
        }

        // ============================================================
        //  환경 / 베이크 설정
        // ============================================================
        static void SetupEnvironment(LightingSampleSettings s)
        {
            if (RenderSettings.skybox == null)
            {
                var shader = Shader.Find("Skybox/Procedural");
                if (shader != null)
                {
                    var sky = new Material(shader) { name = "Lab_Sky" };
                    string p = AssetDatabase.GenerateUniqueAssetPath($"{s.rootFolder}/Materials/Lab_Sky.mat");
                    AssetDatabase.CreateAsset(sky, p);
                    RenderSettings.skybox = sky;
                }
            }
            RenderSettings.ambientMode = AmbientMode.Skybox;
            DynamicGI.UpdateEnvironment();
        }

        static void SetupLightingSettings(LightingSampleSettings s)
        {
            var ls = new LightingSettings { name = s.sampleName + "_Lighting" };
            ls.bakedGI = true;
            ls.realtimeGI = false;
            ls.lightmapResolution = s.lightmapResolution;
            ls.lightmapMaxSize = s.lightmapMaxSize;

            string p = AssetDatabase.GenerateUniqueAssetPath(
                $"{s.rootFolder}/{s.sampleName}_Lighting.lighting");
            AssetDatabase.CreateAsset(ls, p);
            Lightmapping.lightingSettings = ls;
        }

        /// <summary>Light Probe / Lightmap 데이터를 굽는다.</summary>
        public static void Bake()
        {
            Debug.Log("[LightingSample] 베이크 시작 (Lightmapping.BakeAsync)...");
            Lightmapping.BakeAsync();
        }

        // ============================================================
        //  프리미티브 / 머티리얼 유틸
        // ============================================================
        static GameObject CreateBox(GameObject parent, string name, Vector3 center, Vector3 size, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent.transform, true);
            go.transform.position = center;
            go.transform.localScale = size;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            MarkStaticGI(go);
            return go;
        }

        static GameObject CreateSphere(GameObject parent, string name, Vector3 pos, float radius, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.transform.SetParent(parent.transform, true);
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * (radius * 2f);
            go.GetComponent<Renderer>().sharedMaterial = mat;
            // 비교 구체는 동적 오브젝트 → static 아님 (Light Probe 대상)
            return go;
        }

        static void CreateCamera(GameObject root, LightingSampleSettings s)
        {
            var go = new GameObject("Main Camera");
            go.transform.SetParent(root.transform, true);
            go.transform.position = new Vector3(-8f, 7f, -13f);
            go.transform.rotation = Quaternion.LookRotation(
                (new Vector3(7f, 2f, 0f) - go.transform.position).normalized, Vector3.up);
            var cam = go.AddComponent<Camera>();
            cam.tag = "MainCamera";
            go.AddComponent<AudioListener>();
        }

        static Material CreateMaterial(LightingSampleSettings s, string name, Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            var mat = new Material(shader) { name = name };
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            else mat.color = color;
            string p = AssetDatabase.GenerateUniqueAssetPath($"{s.rootFolder}/Materials/{name}.mat");
            AssetDatabase.CreateAsset(mat, p);
            return mat;
        }

        static void MarkStaticGI(GameObject go)
        {
            GameObjectUtility.SetStaticEditorFlags(go,
                StaticEditorFlags.ContributeGI |
                StaticEditorFlags.OccluderStatic |
                StaticEditorFlags.OccludeeStatic |
                StaticEditorFlags.BatchingStatic |
                StaticEditorFlags.ReflectionProbeStatic);
            // static(라이트맵) 오브젝트는 프로브가 아닌 라이트맵을 사용
            var r = go.GetComponent<Renderer>();
            if (r != null) r.lightProbeUsage = LightProbeUsage.Off;
        }

        // ============================================================
        //  폴더 유틸
        // ============================================================
        static readonly string[] SubFolders = { "Editor", "Scripts", "Scenes", "Materials", "Prefabs" };

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
