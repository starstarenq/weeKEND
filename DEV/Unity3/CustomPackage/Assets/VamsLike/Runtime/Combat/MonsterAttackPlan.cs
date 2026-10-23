using UnityEngine;

namespace VamSurvivalLike.Combat
{
    public enum AttackShape
    {
        Circle,
        Rectangle,
        Sector,
        Donut
    }

    /// <summary>Immutable source of truth shared by combat resolution and presentation.</summary>
    public readonly struct MonsterAttackPlan
    {
        public string Key { get; }
        public AttackShape Shape { get; }
        public Vector3 Origin { get; }
        public Vector3 Direction { get; }
        public float Range { get; }
        public float Width { get; }
        public float Angle { get; }
        public float InnerRadiusRatio { get; }
        public float Height { get; }
        public float Damage { get; }
        public LayerMask TargetLayers { get; }
        public float WindupDuration { get; }
        public float ActiveDuration { get; }

        public MonsterAttackPlan(string key, AttackShape shape, Vector3 origin, Vector3 direction,
            float range, float width, float angle, float innerRadiusRatio, float height, float damage,
            LayerMask targetLayers, float windupDuration, float activeDuration)
        {
            Key = key;
            Shape = shape;
            Origin = origin;
            direction.y = 0f;
            Direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
            Range = Mathf.Max(0.05f, range);
            Width = Mathf.Max(0.05f, width);
            Angle = Mathf.Clamp(angle, 1f, 359f);
            InnerRadiusRatio = Mathf.Clamp(innerRadiusRatio, 0f, 0.99f);
            Height = Mathf.Max(0.05f, height);
            Damage = Mathf.Max(0f, damage);
            TargetLayers = targetLayers;
            WindupDuration = Mathf.Max(0f, windupDuration);
            ActiveDuration = Mathf.Max(0f, activeDuration);
        }
    }
}
