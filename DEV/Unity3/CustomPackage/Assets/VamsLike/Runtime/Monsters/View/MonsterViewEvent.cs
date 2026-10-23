using UnityEngine;
using VamSurvivalLike.Combat;

namespace VamSurvivalLike.Monsters.View
{
    /// <summary>Gameplay-to-presentation messages understood by every monster view.</summary>
    public enum MonsterViewEvent
    {
        Spawned,
        Despawned,
        AttackWindup,
        AttackStarted,
        AttackHit,
        AttackEnded,
        Damaged,
        Died,
        Custom
    }

    /// <summary>
    /// Immutable data passed to SFX, VFX, UI and material views. Optional fields let the
    /// same contract support melee, projectiles and custom monster abilities.
    /// </summary>
    public readonly struct MonsterViewContext
    {
        public MonsterActor Actor { get; }
        public MonsterViewEvent Event { get; }
        public Transform Target { get; }
        public Vector3 Position { get; }
        public Vector3 Direction { get; }
        public bool HasDirection { get; }
        public float Value { get; }
        public float Range { get; }
        public string Key { get; }
        public object Source { get; }
        public MonsterAttackPlan AttackPlan { get; }
        public bool HasAttackPlan { get; }

        public MonsterViewContext(
            MonsterActor actor,
            MonsterViewEvent viewEvent,
            Transform target = null,
            Vector3? position = null,
            Vector3? direction = null,
            float value = 0f,
            string key = null,
            object source = null,
            float range = 0f,
            MonsterAttackPlan? attackPlan = null)
        {
            Actor = actor;
            Event = viewEvent;
            Target = target;
            Position = position ?? (actor != null ? actor.transform.position : Vector3.zero);
            Direction = direction ?? (actor != null ? actor.transform.forward : Vector3.forward);
            HasDirection = direction.HasValue;
            Value = value;
            Range = range;
            Key = key;
            Source = source;
            AttackPlan = attackPlan.GetValueOrDefault();
            HasAttackPlan = attackPlan.HasValue;
        }
    }
}
