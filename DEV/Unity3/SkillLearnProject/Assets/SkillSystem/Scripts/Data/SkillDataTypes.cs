using UnityEngine;

namespace KDH_SkillSystem
{
    public enum SkillType
    {
        Active,
        Passive
    }

    public enum TargetType
    {
        Targeting,
        NonTargeting,
        Area
    }

    public enum Element
    {
        None,
        Fire,
        Ice,
        Lightning
    }

    [System.Serializable]
    public struct SkillCostInfo
    {
        [Tooltip("소모 마나 자원량")]
        public int mpCost;
        [Tooltip("재사용 대기시간 (초)")]
        public float cooldown;
    }

    [System.Serializable]
    public struct SkillImpactInfo
    {
        [Tooltip("데미지 계수")]
        public float damageMultiplier;
        [Tooltip("사거리")]
        public float range;
    }
}
