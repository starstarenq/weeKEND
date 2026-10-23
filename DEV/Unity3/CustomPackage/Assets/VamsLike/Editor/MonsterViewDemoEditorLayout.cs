using SysKill.SkillIndicators;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using VamSurvivalLike.Monsters;
using VamSurvivalLike.Monsters.View;

namespace VamSurvivalLike.Editor
{
    /// <summary>Builds Demo.unity exclusively through Unity editor APIs.</summary>
    public static class MonsterViewDemoEditorLayout
    {
        private const string ScenePath = "Assets/VamsLike/Demo/Demo.unity";
        private const string MonsterPrefabPath = "Assets/VamsLike/Demo/CapsuleElite.prefab";
        private const string IndicatorPrefabPath = "Assets/VamsLike/SkillIndicatorLab/Generated/Prefabs/SectorIndicator.prefab";
        private const string AttackKey = "DemoAttack";

        [MenuItem("Tools/Vam Survival Like/Create View + Skill Indicator Demo")]
        public static void CreateDemoScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null &&
                !EditorUtility.DisplayDialog("Replace Demo Scene?", $"Replace {ScenePath}?", "Replace", "Cancel"))
                return;

            var monsterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(MonsterPrefabPath);
            var indicatorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(IndicatorPrefabPath);
            if (monsterPrefab == null || indicatorPrefab == null)
            {
                EditorUtility.DisplayDialog(
                    "Missing Demo Assets",
                    $"Required assets:\n{MonsterPrefabPath}\n{IndicatorPrefabPath}",
                    "OK");
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateLighting();
            CreateCamera();
            CreateGround();
            var target = CreateTarget();
            var actor = CreateMonster(monsterPrefab);
            AttachIndicatorView(actor, target, indicatorPrefab);
            CreateInstructions();

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                throw new System.InvalidOperationException($"Could not save {ScenePath}.");

            AssetDatabase.SaveAssets();
            Selection.activeGameObject = actor.gameObject;
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath));
            Debug.Log($"[MonsterViewDemo] Created and opened {ScenePath}");
        }

        private static MonsterActor CreateMonster(GameObject prefab)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = "Monster_With_Direct_View";
            instance.transform.SetPositionAndRotation(new Vector3(0f, 0.5f, -2f), Quaternion.identity);

            var actor = instance.GetComponent<MonsterActor>();
            if (actor == null)
                throw new System.InvalidOperationException($"{MonsterPrefabPath} has no MonsterActor.");
            return actor;
        }

        private static void AttachIndicatorView(MonsterActor actor, Transform target, GameObject indicatorPrefab)
        {
            var indicatorObject = (GameObject)PrefabUtility.InstantiatePrefab(indicatorPrefab, actor.transform);
            indicatorObject.name = "SectorIndicator_ViewAsset";
            indicatorObject.transform.localPosition = Vector3.zero;

            var demo = indicatorObject.GetComponent<SkillIndicatorDemo>();
            if (demo != null)
                Object.DestroyImmediate(demo);

            var indicator = indicatorObject.GetComponent<SkillIndicator>();
            if (indicator == null)
                throw new System.InvalidOperationException($"{IndicatorPrefabPath} has no SkillIndicator.");

            indicator.owner = actor.transform;
            indicator.targetLayers = 0; // View-only: gameplay damage remains outside presentation.
            indicator.opacity = 0f;
            indicator.impact = 0f;
            indicator.progress = 0f;
            indicator.Apply();
            indicatorObject.SetActive(false);

            var view = actor.gameObject.AddComponent<MonsterSkillIndicatorView>();
            view.Configure(indicator, AttackKey, true);

            var driver = actor.gameObject.AddComponent<MonsterViewDemoController>();
            driver.Configure(actor, target, AttackKey);
        }

        private static Transform CreateTarget()
        {
            var target = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            target.name = "Attack_Target";
            target.transform.position = new Vector3(0f, 1f, 3f);
            target.transform.localScale = new Vector3(0.7f, 1f, 0.7f);
            return target.transform;
        }

        private static void CreateGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(2f, 1f, 2f);
        }

        private static void CreateCamera()
        {
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetPositionAndRotation(
                new Vector3(8f, 9f, -10f),
                Quaternion.Euler(31f, -36f, 0f));
            cameraObject.GetComponent<Camera>().clearFlags = CameraClearFlags.Skybox;
        }

        private static void CreateLighting()
        {
            var lightObject = new GameObject("Directional Light", typeof(Light));
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            var light = lightObject.GetComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
        }

        private static void CreateInstructions()
        {
            new GameObject("README - Play to preview direct MonsterSkillIndicatorView binding");
        }
    }
}
