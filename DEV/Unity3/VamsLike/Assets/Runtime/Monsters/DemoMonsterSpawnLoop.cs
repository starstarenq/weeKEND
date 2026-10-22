using UnityEngine;

namespace VamSurvivalLike.Monsters
{
    /// <summary>Small demo-only driver used by the generated scene to exercise the object pools.</summary>
    public sealed class DemoMonsterSpawnLoop : MonoBehaviour
    {
        [SerializeField] private MonsterPoolSpawner poolSpawner;
        [SerializeField] private MonsterActor[] monsterPrefabs;
        [SerializeField, Min(0f)] private float initialSpawnRadius = 6f;
        [SerializeField, Min(0.1f)] private float spawnInterval = 1f;
        [SerializeField, Min(1)] private int initialCount = 12;

        private float nextSpawnTime;
        private int nextPrefabIndex;

        private void Start()
        {
            for (var index = 0; index < initialCount; index++)
                SpawnAtRandomRing();
            nextSpawnTime = Time.time + spawnInterval;
        }

        private void Update()
        {
            if (Time.time < nextSpawnTime)
                return;

            nextSpawnTime = Time.time + spawnInterval;
            SpawnAtRandomRing();
        }

        private void SpawnAtRandomRing()
        {
            if (poolSpawner == null || monsterPrefabs == null || monsterPrefabs.Length == 0)
                return;

            var angle = Random.value * Mathf.PI * 2f;
            var radius = initialSpawnRadius + Random.Range(0f, 3f);
            var offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
            poolSpawner.Spawn(monsterPrefabs[nextPrefabIndex], transform.position + offset);
            nextPrefabIndex = (nextPrefabIndex + 1) % monsterPrefabs.Length;
        }
    }
}
