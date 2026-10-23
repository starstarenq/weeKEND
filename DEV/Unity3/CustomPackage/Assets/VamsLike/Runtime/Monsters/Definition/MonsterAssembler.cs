using System;
using System.Collections.Generic;
using UnityEngine;

namespace VamSurvivalLike.Monsters.Definition
{
    public static class MonsterAssembler
    {
        public static MonsterActor CreateTemplate(MonsterDefinition definition, Transform parent = null)
        {
            Validate(definition);
            var dataOverride = definition.LoadOverride();
            var actor = UnityEngine.Object.Instantiate(definition.BasePrefab, parent);
            actor.name = $"{definition.MonsterId}_AssembledTemplate";
            actor.Configure(definition.Stats, definition.BrainType, dataOverride?.stats);
            definition.Brain.Apply(actor, definition.BrainType, dataOverride?.brain);

            foreach (var module in definition.Views.Modules)
                module?.Assemble(actor.gameObject, dataOverride);

            actor.gameObject.SetActive(false);
            return actor;
        }

        public static void Validate(MonsterDefinition definition)
        {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));
            if (definition.BasePrefab == null)
                throw new InvalidOperationException($"MonsterDefinition '{definition.name}' has no Base Prefab.");
            if (definition.Stats == null)
                throw new InvalidOperationException($"MonsterDefinition '{definition.name}' has no MonsterStats.");
            if (string.IsNullOrWhiteSpace(definition.MonsterId))
                throw new InvalidOperationException($"MonsterDefinition '{definition.name}' has no Monster ID.");
            var moduleIds = new HashSet<string>();
            foreach (var module in definition.Views.Modules)
            {
                if (module == null)
                    throw new InvalidOperationException($"MonsterDefinition '{definition.name}' contains a missing View module.");
                if (string.IsNullOrWhiteSpace(module.ModuleId))
                    throw new InvalidOperationException($"MonsterDefinition '{definition.name}' contains a View module without an ID.");
                if (!moduleIds.Add(module.ModuleId))
                    throw new InvalidOperationException($"View module ID '{module.ModuleId}' is duplicated in '{definition.name}'.");
            }
        }

        public static Transform GetOrCreateAnchor(Transform root, string path)
        {
            if (string.IsNullOrWhiteSpace(path) || path == ".")
                return root;
            var current = root;
            foreach (var segment in path.Split('/'))
            {
                if (string.IsNullOrWhiteSpace(segment))
                    continue;
                var child = current.Find(segment);
                if (child == null)
                {
                    child = new GameObject(segment).transform;
                    child.SetParent(current, false);
                }
                current = child;
            }
            return current;
        }
    }
}
