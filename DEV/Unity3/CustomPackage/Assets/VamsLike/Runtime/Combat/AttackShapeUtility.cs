using UnityEngine;

namespace VamSurvivalLike.Combat
{
    public static class AttackShapeUtility
    {
        public static bool Contains(in MonsterAttackPlan plan, Vector3 worldPosition)
        {
            var offset = worldPosition - plan.Origin;
            if (offset.y < 0f || offset.y > plan.Height)
                return false;

            var right = Vector3.Cross(Vector3.up, plan.Direction).normalized;
            var x = Vector3.Dot(offset, right);
            var z = Vector3.Dot(offset, plan.Direction);

            if (plan.Shape == AttackShape.Rectangle)
                return Mathf.Abs(x) <= plan.Width * 0.5f && z >= 0f && z <= plan.Range;

            var distance = Mathf.Sqrt(x * x + z * z);
            if (distance > plan.Range)
                return false;
            if (plan.Shape == AttackShape.Donut && distance < plan.Range * plan.InnerRadiusRatio)
                return false;
            if (plan.Shape == AttackShape.Sector && distance > 0.0001f)
                return Mathf.Abs(Mathf.Atan2(x, z) * Mathf.Rad2Deg) <= plan.Angle * 0.5f;
            return true;
        }
    }
}
