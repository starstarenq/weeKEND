using UnityEngine;

namespace VamSurvivalLike.Monsters
{
    [CreateAssetMenu(menuName = "Vam Survival Like/Monster Stats", fileName = "MonsterStats")]
    public sealed class MonsterStats : ScriptableObject
    {
        [field: SerializeField, Min(1f)] public float MaxHealth { get; private set; } = 30f;
        [field: SerializeField, Min(0f)] public float MoveSpeed { get; private set; } = 3f;
        [field: SerializeField, Min(0f)] public float ContactDamage { get; private set; } = 5f;
        [field: SerializeField, Min(0.05f)] public float ContactInterval { get; private set; } = 1f;
    }
}
