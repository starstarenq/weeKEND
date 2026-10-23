using UnityEngine;
using VamSurvivalLike.Combat;

namespace VamSurvivalLike.Monsters
{
    public sealed class ChargeBrain : MonsterBrain
    {
        [SerializeField, Min(0.1f)] private float triggerDistance = 6f;
        [SerializeField, Min(0.01f)] private float windupDuration = 0.6f;
        [SerializeField, Min(0.01f)] private float chargeDuration = 0.7f;
        [SerializeField, Min(0.01f)] private float recoveryDuration = 1f;
        [SerializeField, Min(1f)] private float speedMultiplier = 3f;
        [SerializeField, Min(0.05f)] private float attackWidth = 1.3f;
        [SerializeField, Min(0.05f)] private float hitHeight = 2f;
        [SerializeField] private LayerMask targetLayers = ~0;
        private enum Phase { Approach, Windup, Charge, Recovery }
        private Phase phase;
        private float remaining;
        private Vector3 direction;
        private MonsterAttackPlan attackPlan;

        public void Configure(float trigger, float windup, float duration, float recovery, float speedScale,
            float width = 1.3f, float height = 2f, int layers = ~0)
        {
            triggerDistance = Mathf.Max(0.1f, trigger);
            windupDuration = Mathf.Max(0.01f, windup);
            chargeDuration = Mathf.Max(0.01f, duration);
            recoveryDuration = Mathf.Max(0.01f, recovery);
            speedMultiplier = Mathf.Max(1f, speedScale);
            attackWidth = Mathf.Max(0.05f, width);
            hitHeight = Mathf.Max(0.05f, height);
            targetLayers = layers;
        }

        public override void OnMonsterSpawned() => ResetState();
        public override void OnMonsterDespawned() => ResetState();

        private void ResetState()
        {
            phase = Phase.Approach;
            remaining = 0f;
            direction = Vector3.zero;
            attackPlan = default;
        }

        public override void Tick()
        {
            if (Actor.Target == null || !Actor.IsAlive) return;
            switch (phase)
            {
                case Phase.Approach:
                    var offset = TargetOffset();
                    if (offset.magnitude > triggerDistance)
                        Move(offset, Actor.MoveSpeed, offset.magnitude - triggerDistance);
                    else
                    {
                        direction = offset.sqrMagnitude > 0.001f ? offset.normalized : transform.forward;
                        phase = Phase.Windup;
                        remaining = windupDuration;
                        var chargeRange = Actor.MoveSpeed * speedMultiplier * chargeDuration;
                        attackPlan = new MonsterAttackPlan("Charge", AttackShape.Rectangle, transform.position,
                            direction, chargeRange, attackWidth, 90f, 0f, hitHeight, Actor.ContactDamage,
                            targetLayers, windupDuration, chargeDuration);
                        Actor.BeginAttack(attackPlan, Actor.Target);
                    }
                    break;
                case Phase.Windup:
                    remaining -= Time.deltaTime;
                    if (remaining <= 0f)
                    {
                        phase = Phase.Charge;
                        remaining = chargeDuration;
                        Actor.StartAttack(attackPlan, Actor.Target);
                    }
                    break;
                case Phase.Charge:
                    Move(direction, Actor.MoveSpeed * speedMultiplier,
                        Actor.MoveSpeed * speedMultiplier * Mathf.Max(0f, remaining));
                    remaining -= Time.deltaTime;
                    if (remaining <= 0f)
                    {
                        phase = Phase.Recovery;
                        remaining = recoveryDuration;
                        Actor.EndAttack(attackPlan);
                    }
                    break;
                case Phase.Recovery:
                    remaining -= Time.deltaTime;
                    if (remaining <= 0f) phase = Phase.Approach;
                    break;
            }
        }
    }
}
