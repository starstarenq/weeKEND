using System;
using System.Collections.Generic;
using TTest.UnityUtilities.Pooling;
using UnityEngine;

namespace VamSurvivalLike.Monsters
{
    /// <summary>Owns one package ObjectPool per prefab and returns monsters to the correct pool on death.</summary>
    public sealed class MonsterPoolSpawner : MonoBehaviour
    {
        #pragma warning disable CS0649 // Unity serializes these fields in the Inspector.
        [Serializable]
        private sealed class PoolDefinition
        {
            public MonsterActor prefab;
            [Min(0)] public int prewarmCount = 8;
            [Min(1)] public int maxSize = 64;
        }
        #pragma warning restore CS0649

        [SerializeField] private PoolDefinition[] monsterTypes;
        [SerializeField] private Transform target;
        [SerializeField] private Transform activeRoot;

        private readonly Dictionary<MonsterActor, ObjectPool> pools = new();
        private readonly Dictionary<MonsterActor, ObjectPool> instancePools = new();

        private void Awake()
        {
            if (target == null)
                target = GameObject.FindGameObjectWithTag("Player")?.transform;

            foreach (var definition in monsterTypes)
            {
                if (definition == null || definition.prefab == null || pools.ContainsKey(definition.prefab))
                    continue;

                var maxSize = Mathf.Max(1, definition.prewarmCount, definition.maxSize);
                var pool = new ObjectPool(definition.prefab.gameObject, $"{definition.prefab.name} Pool", transform,
                    definition.prewarmCount, maxSize);
                pools.Add(definition.prefab, pool);
                pool.Prewarm(definition.prewarmCount);
            }
        }

        public MonsterActor Spawn(MonsterActor prefab, Vector3 position)
        {
            if (!pools.TryGetValue(prefab, out var pool))
                throw new ArgumentException("The requested prefab has no Pool Definition.", nameof(prefab));

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
            foreach (var pool in pools.Values)
                pool.Dispose();
            pools.Clear();
            instancePools.Clear();
        }
    }
}
