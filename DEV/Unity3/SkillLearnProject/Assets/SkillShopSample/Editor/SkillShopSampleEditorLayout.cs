using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KDH_SkillSystem.ShopSample.EditorTools
{
    /// <summary>
    /// Resources 폴더의 SkillBase 데이터를 미리 보고 가격을 정한 뒤,
    /// UGUI 상점 + 샘플 인벤토리 씬을 Unity API 로 생성하는 EditorGUILayout 창.
    /// 기존 씬은 수정하지 않고, 매번 새 Generated 폴더에 새 씬을 저장한다.
    /// </summary>
    public sealed class SkillShopSampleEditorLayout : EditorWindow
    {
        const string Root = "Assets/SkillShopSample";
        const string KoreanFontPath = "Fonts & Materials/HeirofLightRegular SDF";

        string resourcesPath = "Skills";
        int startingGold = 500;
        int defaultPrice = 150;
        int inventoryCapacity = 8;
        readonly Dictionary<int, int> prices = new Dictionary<int, int>();
        SkillBase[] skills = Array.Empty<SkillBase>();
        string status = "Resources 의 스킬 데이터로 UGUI 상점 샘플 씬을 생성합니다.";
        SceneAsset lastScene;
        Vector2 scroll;

        [MenuItem("Tools/Skill Manager/Skill Shop Sample (UGUI)")]
        public static void Open()
        {
            var window = GetWindow<SkillShopSampleEditorLayout>("Skill Shop Sample");
            window.minSize = new Vector2(460, 480);
        }

        void OnEnable() => Reload();

        void Reload()
        {
            skills = Resources.LoadAll<SkillBase>(resourcesPath).Where(s => s != null).OrderBy(s => s.SkillId).ToArray();
            foreach (var skill in skills)
                if (!prices.ContainsKey(skill.SkillId)) prices[skill.SkillId] = defaultPrice;
        }

        void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.LabelField("UGUI SKILL SHOP / SAMPLE INVENTORY", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("구매 버튼 → SkillShop.TryPurchase → 골드 차감 → SkillInventory.TryAdd → ItemAdded 이벤트 → 인벤토리 슬롯 표시", MessageType.Info);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("데이터 소스", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                resourcesPath = EditorGUILayout.TextField("Resources 경로", resourcesPath);
                if (GUILayout.Button("새로고침", GUILayout.Width(80))) Reload();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("상점 설정", EditorStyles.boldLabel);
            startingGold = EditorGUILayout.IntSlider("시작 골드", startingGold, 0, 5000);
            defaultPrice = EditorGUILayout.IntSlider("기본 가격", defaultPrice, 0, 2000);
            inventoryCapacity = EditorGUILayout.IntSlider("인벤토리 칸 수", inventoryCapacity, 1, 16);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"판매 목록 ({skills.Length}개)", EditorStyles.boldLabel);
            if (skills.Length == 0)
                EditorGUILayout.HelpBox($"Resources/{resourcesPath} 에서 SkillBase 에셋을 찾지 못했습니다.", MessageType.Warning);
            foreach (var skill in skills) DrawSkillRow(skill);

            EditorGUILayout.Space();
            using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || skills.Length == 0))
            {
                if (GUILayout.Button("Create Skill Shop Sample Scene", GUILayout.Height(36)))
                {
                    try
                    {
                        string path = Generate();
                        if (!string.IsNullOrEmpty(path))
                        {
                            lastScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
                            status = "생성 완료: " + path + "\nPlay 후 '구매' 버튼을 누르면 오른쪽 인벤토리에 아이템이 들어갑니다.";
                            Selection.activeObject = lastScene;
                        }
                    }
                    catch (Exception exception) { status = exception.Message; Debug.LogException(exception); }
                }
            }
            using (new EditorGUI.DisabledScope(lastScene == null || EditorApplication.isPlayingOrWillChangePlaymode))
                if (GUILayout.Button("Open Generated Scene") && EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(lastScene));

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("씬/컴포넌트 직렬화는 모두 Unity API(EditorSceneManager, AddComponent)가 처리합니다. 씬 YAML·GUID 는 직접 수정하지 않습니다.", MessageType.None);
            EditorGUILayout.SelectableLabel(status, EditorStyles.textArea, GUILayout.MinHeight(60));
            EditorGUILayout.EndScrollView();
        }

        void DrawSkillRow(SkillBase skill)
        {
            Texture preview = skill.Icon != null ? AssetPreview.GetAssetPreview(skill.Icon) : null;
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                GUILayout.Label(preview, GUILayout.Width(40), GUILayout.Height(40));
                using (new EditorGUILayout.VerticalScope())
                {
                    EditorGUILayout.LabelField($"[{skill.SkillId}] {skill.SkillName}", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField($"{skill.Element} · MP {skill.CostInfo.mpCost} · CD {skill.CostInfo.cooldown}s", EditorStyles.miniLabel);
                }
                prices[skill.SkillId] = EditorGUILayout.IntField(prices[skill.SkillId], GUILayout.Width(70));
                GUILayout.Label("G", GUILayout.Width(14));
                if (GUILayout.Button("Ping", GUILayout.Width(40))) EditorGUIUtility.PingObject(skill);
            }
            if (preview == null && skill.Icon != null && AssetPreview.IsLoadingAssetPreview(skill.Icon.GetInstanceID())) Repaint();
        }

        /// <summary>새 씬을 만들어 상점 로직/UI 를 구성하고 저장한다. 저장된 씬 경로를 반환한다.</summary>
        public string Generate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Play 모드를 종료한 뒤 생성하세요.");
#if !ENABLE_INPUT_SYSTEM
            throw new InvalidOperationException("Player Settings > Active Input Handling 을 Input System Package (New) 또는 Both 로 설정하세요.");
#else
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return null;

            TMP_FontAsset font = Resources.Load<TMP_FontAsset>(KoreanFontPath);
            if (font == null) font = TMP_Settings.defaultFontAsset;
            if (font == null) throw new InvalidOperationException("TMP 폰트를 찾지 못했습니다. TMP Essential Resources 를 임포트하세요.");
            if (font == TMP_Settings.defaultFontAsset)
                Debug.LogWarning("[SkillShopSample] 한글 폰트(HeirofLightRegular SDF)를 찾지 못해 기본 폰트를 사용합니다. 한글이 깨질 수 있습니다.");

            string folder = AssetDatabase.GenerateUniqueAssetPath(Root + "/Generated");
            AssetDatabase.CreateFolder(Root, Path.GetFileName(folder));
            string scenePath = folder + "/SkillShopSample.unity";

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraObject = new GameObject("Main Camera");
            SceneManager.MoveGameObjectToScene(cameraObject, scene);
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.transform.position = new Vector3(0, 0, -10);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.02f, 0.03f, 0.05f);

            // 로직 오브젝트: 지갑 / 인벤토리 / 상점 (UI 와 분리)
            var systems = new GameObject("Skill Shop System");
            SceneManager.MoveGameObjectToScene(systems, scene);
            var wallet = systems.AddComponent<SkillShopWallet>();
            wallet.Configure(startingGold);
            var inventory = systems.AddComponent<SkillInventory>();
            inventory.Configure(inventoryCapacity);
            var shop = systems.AddComponent<SkillShop>();
            var table = skills.Select(s => new SkillPriceEntry(s.SkillId, Mathf.Max(0, prices[s.SkillId])));
            shop.Configure(resourcesPath, defaultPrice, table, wallet, inventory);

            SkillShopUIBuilder.Build(scene, shop, inventory, font);

            if (!EditorSceneManager.SaveScene(scene, scenePath))
                throw new IOException("씬 저장에 실패했습니다: " + scenePath);
            AssetDatabase.SaveAssets();
            return scenePath;
#endif
        }
    }
}
