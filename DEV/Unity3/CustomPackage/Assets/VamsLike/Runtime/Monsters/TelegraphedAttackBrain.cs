using UnityEngine;
using VamSurvivalLike.Combat;

namespace VamSurvivalLike.Monsters
{
    /// <summary>Shared stationary telegraph flow used by shape-specific attack brains.</summary>
    public abstract class TelegraphedAttackBrain : MonsterBrain
    {
        [SerializeField, Min(0.1f)] private float attackRange = 3f;
        [SerializeField, Min(0.01f)] private float windupDuration = 1f;
        [SerializeField, Min(0.01f)] private float activeDuration = 0.4f;
        [SerializeField, Min(0.01f)] private float recoveryDuration = 1f;
        [SerializeField, Min(0.05f)] private float attackWidth = 2f;
        [SerializeField, Range(1f, 359f)] private float attackAngle = 100f;
        [SerializeField, Range(0f, 0.99f)] private float innerRadiusRatio = 0.55f;
        [SerializeField, Min(0.05f)] private float hitHeight = 2f;
        [SerializeField] private LayerMask targetLayers = ~0;

        private enum Phase { Approach, Windup, Active, Recovery }
        private Phase phase;
        private float remaining;
        private Vector3 attackDirection;
        private MonsterAttackPlan attackPlan;

        protected abstract string AttackKey { get; }
        protected abstract AttackShape Shape { get; }

        public void Configure(float range, float windup, float active, float recovery,
            float width = 2f, float angle = 100f, float innerRatio = 0.55f, float height = 2f, int layers = ~0)
        {
            attackRange = Mathf.Max(0.1f, range);
            windupDuration = Mathf.Max(0.01f, windup);
            activeDuration = Mathf.Max(0.01f, active);
            recoveryDuration = Mathf.Max(0.01f, recovery);
            attackWidth = Mathf.Max(0.05f, width);
            attackAngle = Mathf.Clamp(angle, 1f, 359f);
            innerRadiusRatio = Mathf.Clamp(innerRatio, 0f, 0.99f);
            hitHeight = Mathf.Max(0.05f, height);
            targetLayers = layers;
        }

        public override void OnMonsterSpawned() => ResetState();

        public override void OnMonsterDespawned()
        {
            if (phase == Phase.Windup || phase == Phase.Active)
                Actor.EndAttack(attackPlan);
            ResetState();
        }

        public override void Tick()
        {
            if (Actor.Target == null || !Actor.IsAlive)
                return;

            switch (phase)
            {
                case Phase.Approach:
                    var offset = TargetOffset();
                    if (offset.magnitude > attackRange)
                    {
                        Move(offset, Actor.MoveSpeed, offset.magnitude - attackRange);
                        return;
                    }
                    attackDirection = offset.sqrMagnitude > 0.001f ? offset.normalized : transform.forward;
                    Face(attackDirection);
                    phase = Phase.Windup;
                    remaining = windupDuration;
                    attackPlan = new MonsterAttackPlan(AttackKey, Shape, transform.position, attackDirection,
                        attackRange, attackWidth, attackAngle, innerRadiusRatio, hitHeight, Actor.ContactDamage,
                        targetLayers, windupDuration, activeDuration);
                    Actor.BeginAttack(attackPlan, Actor.Target);
                    break;

                case Phase.Windup:
                    remaining -= Time.deltaTime;
                    if (remaining <= 0f)
                    {
                        phase = Phase.Active;
                        remaining = activeDuration;
                        Actor.StartAttack(attackPlan, Actor.Target);
                    }
                    break;

                case Phase.Active:
                    remaining -= Time.deltaTime;
                    if (remaining <= 0f)
                    {
                        Actor.EndAttack(attackPlan);
                        phase = Phase.Recovery;
                        remaining = recoveryDuration;
                    }
                    break;

                case Phase.Recovery:
                    remaining -= Time.deltaTime;
                    if (remaining <= 0f)
                        phase = Phase.Approach;
                    break;
            }
        }

        private void Face(Vector3 direction)
        {
            if (direction.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }

        private void ResetState()
        {
            phase = Phase.Approach;
            remaining = 0f;
            attackDirection = Vector3.zero;
            attackPlan = default;
        }
    }
}
