using UnityEngine;

namespace VamSurvivalLike.Monsters
{
    public sealed class ChargeBrain : MonsterBrain
    {
        [SerializeField, Min(0.1f)] private float triggerDistance = 6f;
        [SerializeField, Min(0.01f)] private float windupDuration = 0.6f;
        [SerializeField, Min(0.01f)] private float chargeDuration = 0.7f;
        [SerializeField, Min(0.01f)] private float recoveryDuration = 1f;
        [SerializeField, Min(1f)] private float speedMultiplier = 3f;
        private enum Phase { Approach, Windup, Charge, Recovery }
        private Phase phase;
        private float remaining;
        private Vector3 direction;

        public override void OnMonsterSpawned() => ResetState();
        public override void OnMonsterDespawned() => ResetState();

        private void ResetState()
        {
            phase = Phase.Approach;
            remaining = 0f;
            direction = Vector3.zero;
        }

        public override void Tick()
        {
            if (Actor.Target == null || !Actor.IsAlive) return;
            switch (phase)
            {
                case Phase.Approach:
                    var offset = TargetOffset();
                    if (offset.magnitude > triggerDistance)
                        Move(offset, Actor.Stats.MoveSpeed, offset.magnitude - triggerDistance);
                    else
                    {
                        direction = offset.sqrMagnitude > 0.001f ? offset.normalized : transform.forward;
                        phase = Phase.Windup;
                        remaining = windupDuration;
                    }
                    break;
                case Phase.Windup:
                    remaining -= Time.deltaTime;
                    if (remaining <= 0f) { phase = Phase.Charge; remaining = chargeDuration; }
                    break;
                case Phase.Charge:
                    Move(direction, Actor.Stats.MoveSpeed * speedMultiplier,
                        Actor.Stats.MoveSpeed * speedMultiplier * Mathf.Max(0f, remaining));
                    remaining -= Time.deltaTime;
                    if (remaining <= 0f) { phase = Phase.Recovery; remaining = recoveryDuration; }
                    break;
                case Phase.Recovery:
                    remaining -= Time.deltaTime;
                    if (remaining <= 0f) phase = Phase.Approach;
                    break;
            }
        }
    }
}
