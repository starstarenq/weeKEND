using System;
using SysKill.SkillIndicators;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using VamSurvivalLike.Monsters;
using VamSurvivalLike.Monsters.Definition;

namespace VamSurvivalLike.Editor
{
    /// <summary>Creates four data-driven monster variants and registers them with the open demo scene.</summary>
    public static class DiverseMonsterSetGenerator
    {
        private const string OutputFolder = "Assets/VamsLike/Definitions/Generated";
        private const string OverrideFolder = "Assets/VamsLike/Demo/MonsterOverrides";

        private readonly struct Spec
        {
            public readonly string Id;
            public readonly string AssetName;
            public readonly string MonsterPrefab;
            public readonly string Stats;
            public readonly string IndicatorPrefab;
            public readonly string Json;
            public readonly string ModuleId;
            public readonly string AttackKey;
            public readonly MonsterBrainType BrainType;
            public readonly SkillIndicator.Shape Shape;
            public readonly Color Tint;
            public readonly float Radius;
            public readonly float Width;
            public readonly float Length;
            public readonly float Angle;
            public readonly bool SyncRange;

            public Spec(string id, string assetName, string monsterPrefab, string stats, string indicatorPrefab,
                string json, string moduleId, string attackKey, MonsterBrainType brainType,
                SkillIndicator.Shape shape, Color tint,
                float radius, float width, float length, float angle, bool syncRange)
            {
                Id = id;
                AssetName = assetName;
                MonsterPrefab = monsterPrefab;
                Stats = stats;
                IndicatorPrefab = indicatorPrefab;
                Json = json;
                ModuleId = moduleId;
                AttackKey = attackKey;
                BrainType = brainType;
                Shape = shape;
                Tint = tint;
                Radius = radius;
                Width = width;
                Length = length;
                Angle = angle;
                SyncRange = syncRange;
            }
        }

        private static readonly Spec[] Specs =
        {
            new("cube_dash", "CubeDashDefinition",
                "Assets/VamsLike/Demo/CubeGrunt.prefab", "Assets/VamsLike/Demo/CubeGruntStats.asset",
                "Assets/VamsLike/SkillIndicatorLab/Generated/Prefabs/RectangleIndicator.prefab",
                $"{OverrideFolder}/CubeDash.json", "dash_rectangle", "Charge", MonsterBrainType.Charge,
                SkillIndicator.Shape.Rectangle,
                new Color(1f, 0.29f, 0.24f, 1f), 2f, 1.3f, 8f, 90f, true),
            new("sphere_burst", "SphereBurstDefinition",
                "Assets/VamsLike/Demo/SphereRunner.prefab", "Assets/VamsLike/Demo/SphereRunnerStats.asset",
                "Assets/VamsLike/SkillIndicatorLab/Generated/Prefabs/CircleIndicator.prefab",
                $"{OverrideFolder}/SphereBurst.json", "burst_circle", "Pulse", MonsterBrainType.Pulse,
                SkillIndicator.Shape.Circle,
                new Color(0.16f, 0.71f, 1f, 1f), 3f, 2f, 4f, 90f, true),
            new("capsule_cleave", "CapsuleCleaveDefinition",
                "Assets/VamsLike/Demo/CapsuleElite.prefab", "Assets/VamsLike/Demo/CapsuleEliteStats.asset",
                "Assets/VamsLike/SkillIndicatorLab/Generated/Prefabs/SectorIndicator.prefab",
                $"{OverrideFolder}/CapsuleCleave.json", "cleave_sector", "Cleave", MonsterBrainType.Cleave,
                SkillIndicator.Shape.Sector,
                new Color(1f, 0.69f, 0.18f, 1f), 4f, 2f, 4f, 110f, true),
            new("sphere_ring", "SphereRingDefinition",
                "Assets/VamsLike/Demo/SphereRunner.prefab", "Assets/VamsLike/Demo/SphereRunnerStats.asset",
                "Assets/VamsLike/SkillIndicatorLab/Generated/Prefabs/DonutIndicator.prefab",
                $"{OverrideFolder}/SphereRing.json", "ring_donut", "RingBurst", MonsterBrainType.RingBurst,
                SkillIndicator.Shape.Donut,
                new Color(0.72f, 0.36f, 1f, 1f), 4.5f, 2f, 4f, 90f, true)
        };

        [MenuItem("Tools/Vam Survival Like/Generate Diverse Monster Set")]
        public static void GenerateAndRegister()
        {
            try
            {
                var definitions = GenerateAssets();
                RegisterInOpenScene(definitions);
                Selection.activeObject = definitions[0];
                EditorGUIUtility.PingObject(definitions[0]);
                Debug.Log($"[DiverseMonsterSet] Generated {definitions.Length} definitions and registered the open scene.");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorUtility.DisplayDialog("Monster Set Generation Failed", exception.Message, "OK");
            }
        }

        public static MonsterDefinition[] GenerateAssets()
        {
            AssetDatabase.Refresh();
            EnsureFolder("Assets/VamsLike/Definitions");
            EnsureFolder(OutputFolder);

            var definitions = new MonsterDefinition[Specs.Length];
            for (var index = 0; index < Specs.Length; index++)
                definitions[index] = CreateOrUpdate(Specs[index]);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return definitions;
        }

        private static MonsterDefinition CreateOrUpdate(in Spec spec)
        {
            var definitionPath = $"{OutputFolder}/{spec.AssetName}.asset";
            var definition = AssetDatabase.LoadAssetAtPath<MonsterDefinition>(definitionPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<MonsterDefinition>();
                AssetDatabase.CreateAsset(definition, definitionPath);
            }

            var monsterPrefab = RequiredComponent<MonsterActor>(spec.MonsterPrefab);
            var stats = Required<MonsterStats>(spec.Stats);
            var indicatorPrefab = RequiredComponent<SkillIndicator>(spec.IndicatorPrefab);
            var json = Required<TextAsset>(spec.Json);

            Undo.RecordObject(definition, "Generate Diverse Monster Definition");
            definition.Configure(spec.Id, monsterPrefab, stats, spec.BrainType, json, 6, 32);
            definition.Brain.attackWidth = spec.Width;
            definition.Brain.attackAngle = spec.Angle;
            definition.Brain.innerRadiusRatio = spec.Shape == SkillIndicator.Shape.Donut ? 0.58f : 0f;
            definition.Brain.hitHeight = 2f;
            definition.Brain.targetLayers = ~0;
            definition.Views.Clear();
            var module = new SkillIndicatorViewModuleDefinition();
            module.Configure(indicatorPrefab, spec.Shape, spec.ModuleId, spec.AttackKey,
                spec.Radius, spec.Width, spec.Length, spec.Angle, spec.Tint,
                spec.SyncRange, true);
            definition.Views.Add(module);
            EditorUtility.SetDirty(definition);
            MonsterAssembler.Validate(definition);
            return definition;
        }

        private static void RegisterInOpenScene(MonsterDefinition[] definitions)
        {
            var spawner = UnityEngine.Object.FindFirstObjectByType<MonsterPoolSpawner>();
            if (spawner == null)
                throw new InvalidOperationException("The open scene has no MonsterPoolSpawner.");

            SetObjectArray(spawner, "definitions", definitions);

            var loop = UnityEngine.Object.FindFirstObjectByType<DemoMonsterSpawnLoop>();
            if (loop != null)
                SetObjectArray(loop, "monsterDefinitions", definitions);

            EditorSceneManager.MarkSceneDirty(spawner.gameObject.scene);
        }

        private static void SetObjectArray(UnityEngine.Object target, string propertyName, MonsterDefinition[] values)
        {
            Undo.RecordObject(target, "Register Diverse Monster Set");
            var serialized = new SerializedObject(target);
            var property = serialized.FindProperty(propertyName);
            if (property == null)
                throw new MissingFieldException(target.GetType().Name, propertyName);
            property.arraySize = values.Length;
            for (var index = 0; index < values.Length; index++)
                property.GetArrayElementAtIndex(index).objectReferenceValue = values[index];
            serialized.ApplyModifiedProperties();
        }

        private static T Required<T>(string path) where T : UnityEngine.Object
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
                throw new InvalidOperationException($"Required {typeof(T).Name} asset was not found: {path}");
            return asset;
        }

        private static T RequiredComponent<T>(string path) where T : Component
        {
            var prefab = Required<GameObject>(path);
            var component = prefab.GetComponent<T>();
            if (component == null)
                throw new InvalidOperationException($"Prefab has no {typeof(T).Name}: {path}");
            return component;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;
            var slash = path.LastIndexOf('/');
            var parent = path.Substring(0, slash);
            var name = path.Substring(slash + 1);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
