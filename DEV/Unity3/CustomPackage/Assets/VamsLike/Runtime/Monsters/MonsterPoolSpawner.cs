using System;
using System.Collections.Generic;
using TTest.UnityUtilities.Pooling;
using UnityEngine;
using VamSurvivalLike.Monsters.Definition;

namespace VamSurvivalLike.Monsters
{
    /// <summary>Assembles and owns one ObjectPool per MonsterDefinition.</summary>
    public sealed class MonsterPoolSpawner : MonoBehaviour
    {
        [SerializeField] private MonsterDefinition[] definitions;
        [SerializeField] private Transform target;
        [SerializeField] private Transform activeRoot;

        private readonly Dictionary<MonsterActor, ObjectPool> instancePools = new();
        private readonly Dictionary<MonsterDefinition, ObjectPool> definitionPools = new();
        private readonly List<MonsterActor> assembledTemplates = new();

        private void Awake()
        {
            if (target == null)
                target = GameObject.FindGameObjectWithTag("Player")?.transform;

            if (definitions == null)
                return;
            foreach (var definition in definitions)
            {
                if (definition == null || definitionPools.ContainsKey(definition))
                    continue;
                var template = MonsterAssembler.CreateTemplate(definition, transform);
                assembledTemplates.Add(template);
                var pool = new ObjectPool(template.gameObject, $"{definition.MonsterId} Pool", transform,
                    definition.PrewarmCount, definition.MaxPoolSize);
                definitionPools.Add(definition, pool);
                pool.Prewarm(definition.PrewarmCount);
            }
        }

        public MonsterActor Spawn(MonsterDefinition definition, Vector3 position)
        {
            if (!definitionPools.TryGetValue(definition, out var pool))
                throw new ArgumentException("The requested MonsterDefinition is not registered.", nameof(definition));
            return SpawnFromPool(pool, position);
        }

        private MonsterActor SpawnFromPool(ObjectPool pool, Vector3 position)
        {
            var monster = pool.Spawn<MonsterActor>(activeRoot);
            monster.transform.SetPositionAndRotation(position, Quaternion.identity);
            instancePools[monster] = pool;
            monster.SetTarget(target);
            monster.Died += ReturnToPool;
            return monster;
        }

        private void ReturnToPool(MonsterActor monster)
        {
            monster.Died -= ReturnToPool;
            if (instancePools.Remove(monster, out var pool))
                pool.Despawn(monster.gameObject);
        }

        private void OnDestroy()
        {
            foreach (var pool in definitionPools.Values)
                pool.Dispose();
            definitionPools.Clear();
            instancePools.Clear();
            foreach (var template in assembledTemplates)
                if (template != null)
                    Destroy(template.gameObject);
            assembledTemplates.Clear();
        }
    }
}
