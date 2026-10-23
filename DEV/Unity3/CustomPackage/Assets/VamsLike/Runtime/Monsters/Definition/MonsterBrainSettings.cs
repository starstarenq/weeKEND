using System;
using UnityEngine;

namespace VamSurvivalLike.Monsters.Definition
{
    [Serializable]
    public sealed class MonsterBrainSettings
    {
        [Header("Direct Chase")]
        [Min(0f)] public float stoppingDistance = 1.1f;
        [Min(0f)] public float turnSpeed = 720f;

        [Header("Charge / Telegraphed Attack")]
        [Min(0.1f)] public float triggerDistance = 6f;
        [Min(0.01f)] public float windupDuration = 0.6f;
        [Min(0.01f)] public float chargeDuration = 0.7f;
        [Min(0.01f)] public float recoveryDuration = 1f;
        [Min(1f)] public float chargeSpeedMultiplier = 3f;
        [Min(0.05f)] public float attackWidth = 2f;
        [Range(1f, 359f)] public float attackAngle = 100f;
        [Range(0f, 0.99f)] public float innerRadiusRatio = 0.55f;
        [Min(0.05f)] public float hitHeight = 2f;
        public LayerMask targetLayers = ~0;

        [Header("Orbit")]
        [Min(0.1f)] public float orbitRadius = 3f;
        public bool clockwise = true;

        [Header("Keep Distance")]
        [Min(0f)] public float desiredDistance = 4f;
        [Min(0f)] public float distanceTolerance = 0.4f;

        public void Apply(MonsterActor actor, MonsterBrainType type, MonsterDefinitionOverride.BrainOverride dataOverride = null)
        {
            switch (type)
            {
                case MonsterBrainType.Charge:
                    if (actor.TryGetComponent<ChargeBrain>(out var charge))
                        charge.Configure(Value(dataOverride?.triggerDistance ?? -1f, triggerDistance),
                            Value(dataOverride?.windupDuration ?? -1f, windupDuration),
                            Value(dataOverride?.chargeDuration ?? -1f, chargeDuration),
                            Value(dataOverride?.recoveryDuration ?? -1f, recoveryDuration),
                            Value(dataOverride?.chargeSpeedMultiplier ?? -1f, chargeSpeedMultiplier),
                            attackWidth, hitHeight, targetLayers);
                    break;
                case MonsterBrainType.Orbit:
                    if (actor.TryGetComponent<OrbitBrain>(out var orbit))
                        orbit.Configure(Value(dataOverride?.orbitRadius ?? -1f, orbitRadius),
                            dataOverride != null && dataOverride.clockwise >= 0 ? dataOverride.clockwise != 0 : clockwise);
                    break;
                case MonsterBrainType.KeepDistance:
                    if (actor.TryGetComponent<KeepDistanceBrain>(out var keepDistance))
                        keepDistance.Configure(Value(dataOverride?.desiredDistance ?? -1f, desiredDistance),
                            Value(dataOverride?.distanceTolerance ?? -1f, distanceTolerance));
                    break;
                case MonsterBrainType.Pulse:
                    ConfigureTelegraph<PulseBrain>(actor, dataOverride);
                    break;
                case MonsterBrainType.Cleave:
                    ConfigureTelegraph<CleaveBrain>(actor, dataOverride);
                    break;
                case MonsterBrainType.RingBurst:
                    ConfigureTelegraph<RingBurstBrain>(actor, dataOverride);
                    break;
                default:
                    if (actor.TryGetComponent<DirectChaseBrain>(out var direct))
                        direct.Configure(Value(dataOverride?.stoppingDistance ?? -1f, stoppingDistance),
                            Value(dataOverride?.turnSpeed ?? -1f, turnSpeed));
                    break;
            }
        }

        private void ConfigureTelegraph<T>(MonsterActor actor, MonsterDefinitionOverride.BrainOverride dataOverride)
            where T : TelegraphedAttackBrain
        {
            if (actor.TryGetComponent<T>(out var brain))
                brain.Configure(Value(dataOverride?.triggerDistance ?? -1f, triggerDistance),
                    Value(dataOverride?.windupDuration ?? -1f, windupDuration),
                    Value(dataOverride?.chargeDuration ?? -1f, chargeDuration),
                    Value(dataOverride?.recoveryDuration ?? -1f, recoveryDuration),
                    attackWidth, attackAngle, innerRadiusRatio, hitHeight, targetLayers);
        }

        private static float Value(float candidate, float fallback) => candidate >= 0f ? candidate : fallback;
    }
}
