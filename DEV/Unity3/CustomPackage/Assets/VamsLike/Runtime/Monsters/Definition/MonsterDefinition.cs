using UnityEngine;

namespace VamSurvivalLike.Monsters.Definition
{
    [CreateAssetMenu(menuName = "Vam Survival Like/Monster Definition", fileName = "MonsterDefinition")]
    public sealed class MonsterDefinition : ScriptableObject
    {
        [SerializeField] private string monsterId = "monster";
        [SerializeField] private MonsterActor basePrefab;
        [SerializeField] private MonsterStats stats;
        [SerializeField] private MonsterBrainType brainType;
        [SerializeField] private MonsterBrainSettings brain = new();
        [SerializeField] private MonsterViewRecipe views = new();
        [SerializeField, Min(0)] private int prewarmCount = 8;
        [SerializeField, Min(1)] private int maxPoolSize = 64;
        [SerializeField, Tooltip("Optional schemaVersion 1 JSON values applied without changing this asset.")]
        private TextAsset jsonOverride;

        public string MonsterId => monsterId;
        public MonsterActor BasePrefab => basePrefab;
        public MonsterStats Stats => stats;
        public MonsterBrainType BrainType => brainType;
        public MonsterBrainSettings Brain => brain;
        public MonsterViewRecipe Views => views;
        public TextAsset JsonOverride => jsonOverride;
        public int PrewarmCount => Mathf.Max(0, prewarmCount);
        public int MaxPoolSize => Mathf.Max(1, prewarmCount, maxPoolSize);

        public MonsterDefinitionOverride LoadOverride() => MonsterDefinitionOverride.Parse(jsonOverride, monsterId);

        public void Configure(
            string id,
            MonsterActor prefab,
            MonsterStats monsterStats,
            MonsterBrainType type,
            TextAsset overrideJson,
            int prewarm = 8,
            int maxSize = 64)
        {
            monsterId = id;
            basePrefab = prefab;
            stats = monsterStats;
            brainType = type;
            jsonOverride = overrideJson;
            prewarmCount = Mathf.Max(0, prewarm);
            maxPoolSize = Mathf.Max(1, prewarmCount, maxSize);
        }

        private void OnValidate()
        {
            monsterId = string.IsNullOrWhiteSpace(monsterId) ? name : monsterId.Trim();
        }
    }
}
