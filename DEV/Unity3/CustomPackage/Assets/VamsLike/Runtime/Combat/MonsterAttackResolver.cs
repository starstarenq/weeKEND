using System.Collections.Generic;
using UnityEngine;
using VamSurvivalLike.Monsters;

namespace VamSurvivalLike.Combat
{
    [DisallowMultipleComponent]
    public sealed class MonsterAttackResolver : MonoBehaviour
    {
        private readonly Collider[] hits = new Collider[128];
        private readonly HashSet<IDamageable> uniqueTargets = new();
        private MonsterActor actor;

        public void Initialize(MonsterActor owner) => actor = owner;

        public void Resolve(in MonsterAttackPlan plan)
        {
            uniqueTargets.Clear();
            var center = plan.Shape == AttackShape.Rectangle
                ? plan.Origin + plan.Direction * (plan.Range * 0.5f) + Vector3.up * (plan.Height * 0.5f)
                : plan.Origin + Vector3.up * (plan.Height * 0.5f);
            var extents = plan.Shape == AttackShape.Rectangle
                ? new Vector3(plan.Width * 0.5f, plan.Height * 0.5f, plan.Range * 0.5f)
                : new Vector3(plan.Range, plan.Height * 0.5f, plan.Range);
            var rotation = Quaternion.LookRotation(plan.Direction, Vector3.up);
            var count = Physics.OverlapBoxNonAlloc(center, extents, hits, rotation,
                plan.TargetLayers, QueryTriggerInteraction.Collide);

            for (var index = 0; index < count; index++)
            {
                var behaviours = hits[index].GetComponentsInParent<MonoBehaviour>();
                foreach (var behaviour in behaviours)
                {
                    if (behaviour is not IDamageable target || !target.IsAlive || ReferenceEquals(target, actor))
                        continue;
                    if (!AttackShapeUtility.Contains(plan, behaviour.transform.position) || !uniqueTargets.Add(target))
                        break;
                    target.TakeDamage(plan.Damage, actor);
                    actor.ReportAttackHit(plan.Key, behaviour.transform, behaviour.transform.position, plan.Damage);
                    break;
                }
            }
        }
    }
}
