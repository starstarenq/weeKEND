using System;
using UnityEditor;
using UnityEngine;
using VamSurvivalLike.Monsters;
using VamSurvivalLike.Monsters.Definition;

namespace VamSurvivalLike.Editor
{
    public sealed class MonsterDefinitionLayoutWindow : EditorWindow
    {
        private MonsterDefinition definition;
        private SerializedObject serializedDefinition;
        private Vector2 scroll;
        private string status = "Select or create a MonsterDefinition.";

        [MenuItem("Tools/Vam Survival Like/Monster Definition Layout")]
        private static void Open() => GetWindow<MonsterDefinitionLayoutWindow>("Monster Definition");

        private void OnGUI()
        {
            EditorGUILayout.LabelField("MONSTER DEFINITION ASSEMBLER", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Compose gameplay, brain and view hierarchy data. Validate it, then bake a pool-ready prefab or let MonsterPoolSpawner assemble it at runtime.",
                MessageType.Info);

            DrawAssetToolbar();
            if (definition == null)
                return;

            EnsureSerializedObject();
            serializedDefinition.Update();
            scroll = EditorGUILayout.BeginScrollView(scroll);
            DrawDefinition();
            EditorGUILayout.Space(8f);
            DrawViewRecipe();
            EditorGUILayout.Space(8f);
            DrawActions();
            EditorGUILayout.Space(8f);
            EditorGUILayout.HelpBox(status, MessageType.None);
            EditorGUILayout.EndScrollView();

            if (serializedDefinition.ApplyModifiedProperties())
                EditorUtility.SetDirty(definition);
        }

        private void DrawAssetToolbar()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            definition = (MonsterDefinition)EditorGUILayout.ObjectField("Definition", definition,
                typeof(MonsterDefinition), false);
            if (EditorGUI.EndChangeCheck())
                serializedDefinition = definition != null ? new SerializedObject(definition) : null;
            if (GUILayout.Button("New", GUILayout.Width(64f)))
                CreateDefinition();
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Generate + Register Diverse Sample Set"))
                DiverseMonsterSetGenerator.GenerateAndRegister();
        }

        private void DrawDefinition()
        {
            EditorGUILayout.LabelField("Identity and Gameplay", EditorStyles.boldLabel);
            Property("monsterId");
            Property("basePrefab");
            Property("stats");
            Property("brainType");
            Property("brain", true);
            Property("prewarmCount");
            Property("maxPoolSize");
            Property("jsonOverride");
        }

        private void DrawViewRecipe()
        {
            EditorGUILayout.LabelField("View Recipe / Hierarchy", EditorStyles.boldLabel);
            var modules = serializedDefinition.FindProperty("views").FindPropertyRelative("modules");
            for (var index = 0; index < modules.arraySize; index++)
            {
                var module = modules.GetArrayElementAtIndex(index);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Module {index + 1}: {ManagedTypeName(module)}", EditorStyles.boldLabel);
                if (GUILayout.Button("Remove", GUILayout.Width(68f)))
                {
                    modules.DeleteArrayElementAtIndex(index);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    break;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.PropertyField(module, GUIContent.none, true);
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+ Skill Indicator"))
                AddModule(new SkillIndicatorViewModuleDefinition());
            if (GUILayout.Button("+ View Prefab"))
                AddModule(new PrefabViewModuleDefinition());
            EditorGUILayout.EndHorizontal();
        }

        private void DrawActions()
        {
            EditorGUILayout.LabelField("Assembly", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Validate", GUILayout.Height(28f)))
                ValidateDefinition();
            if (GUILayout.Button("Bake Prefab...", GUILayout.Height(28f)))
                BakePrefab();
            EditorGUILayout.EndHorizontal();

            if (Selection.activeGameObject != null && Selection.activeGameObject.TryGetComponent<MonsterPoolSpawner>(out _)
                && GUILayout.Button("Register Definition on Selected Spawner"))
                RegisterOnSelectedSpawner();
        }

        private void AddModule(MonsterViewModuleDefinition module)
        {
            serializedDefinition.ApplyModifiedProperties();
            Undo.RecordObject(definition, "Add Monster View Module");
            definition.Views.Add(module);
            EditorUtility.SetDirty(definition);
            serializedDefinition.Update();
        }

        private void ValidateDefinition()
        {
            GameObject temporary = null;
            try
            {
                serializedDefinition.ApplyModifiedProperties();
                var actor = MonsterAssembler.CreateTemplate(definition);
                temporary = actor.gameObject;
                status = $"Valid: '{definition.MonsterId}' assembled with {definition.Views.Modules.Count} view module(s).";
            }
            catch (Exception exception)
            {
                status = $"Validation failed: {exception.Message}";
                Debug.LogException(exception);
            }
            finally
            {
                if (temporary != null)
                    DestroyImmediate(temporary);
            }
        }

        private void BakePrefab()
        {
            serializedDefinition.ApplyModifiedProperties();
            var defaultName = string.IsNullOrWhiteSpace(definition.MonsterId) ? definition.name : definition.MonsterId;
            var path = EditorUtility.SaveFilePanelInProject("Bake Assembled Monster", $"{defaultName}.prefab", "prefab",
                "Choose where to save the assembled monster prefab.", "Assets/VamsLike/Generated");
            if (string.IsNullOrEmpty(path))
                return;

            GameObject temporary = null;
            try
            {
                var actor = MonsterAssembler.CreateTemplate(definition);
                temporary = actor.gameObject;
                temporary.SetActive(true);
                var prefab = PrefabUtility.SaveAsPrefabAsset(temporary, path);
                if (prefab == null)
                    throw new InvalidOperationException($"Unity could not save '{path}'.");
                AssetDatabase.SaveAssets();
                Selection.activeObject = prefab;
                EditorGUIUtility.PingObject(prefab);
                status = $"Baked pool-ready prefab: {path}";
            }
            catch (Exception exception)
            {
                status = $"Bake failed: {exception.Message}";
                Debug.LogException(exception);
            }
            finally
            {
                if (temporary != null)
                    DestroyImmediate(temporary);
            }
        }

        private void RegisterOnSelectedSpawner()
        {
            var spawner = Selection.activeGameObject.GetComponent<MonsterPoolSpawner>();
            var serializedSpawner = new SerializedObject(spawner);
            var definitions = serializedSpawner.FindProperty("definitions");
            for (var index = 0; index < definitions.arraySize; index++)
            {
                if (definitions.GetArrayElementAtIndex(index).objectReferenceValue == definition)
                {
                    status = "The selected Spawner already contains this definition.";
                    return;
                }
            }

            Undo.RecordObject(spawner, "Register Monster Definition");
            definitions.InsertArrayElementAtIndex(definitions.arraySize);
            definitions.GetArrayElementAtIndex(definitions.arraySize - 1).objectReferenceValue = definition;
            serializedSpawner.ApplyModifiedProperties();
            status = $"Registered '{definition.MonsterId}' on {spawner.name}.";
        }

        private void CreateDefinition()
        {
            var path = EditorUtility.SaveFilePanelInProject("Create Monster Definition", "MonsterDefinition", "asset",
                "Choose a location for the definition.", "Assets/VamsLike/Definitions");
            if (string.IsNullOrEmpty(path))
                return;
            var asset = CreateInstance<MonsterDefinition>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            definition = asset;
            serializedDefinition = new SerializedObject(definition);
            Selection.activeObject = asset;
            status = $"Created {path}";
        }

        private void Property(string name, bool includeChildren = false)
        {
            var property = serializedDefinition.FindProperty(name);
            if (property != null)
                EditorGUILayout.PropertyField(property, includeChildren);
        }

        private void EnsureSerializedObject()
        {
            if (serializedDefinition == null || serializedDefinition.targetObject != definition)
                serializedDefinition = new SerializedObject(definition);
        }

        private static string ManagedTypeName(SerializedProperty property)
        {
            var typeName = property.managedReferenceFullTypename;
            if (string.IsNullOrEmpty(typeName))
                return "Missing Type";
            var lastDot = typeName.LastIndexOf('.');
            return lastDot >= 0 ? typeName.Substring(lastDot + 1) : typeName;
        }
    }
}
