using System;
using TTest.UnityUtilities.Pooling;
using UnityEngine;
using VamSurvivalLike.Combat;
using VamSurvivalLike.Monsters.View;
using VamSurvivalLike.Monsters.Definition;

namespace VamSurvivalLike.Monsters
{
    /// <summary>Health, pool lifecycle, and enum-selected AI. Only the selected brain is ticked.</summary>
    public sealed class MonsterActor : MonoBehaviour, IDamageable, IPoolable
    {
        [SerializeField] private MonsterStats stats;
        [SerializeField] private MonsterBrainType brainType;
        [SerializeField] private Renderer[] renderers;
        [SerializeField] private MonsterViewHub viewHub;
        [SerializeField] private MonsterAttackResolver attackResolver;

        private float health;
        private bool hasDespawned;
        private bool spawned;
        private MonsterBrain activeBrain;
        private MonsterBrainType appliedBrainType;
        [SerializeField, HideInInspector] private float maxHealth;
        [SerializeField, HideInInspector] private float moveSpeed = -1f;
        [SerializeField, HideInInspector] private float contactDamage = -1f;
        [SerializeField, HideInInspector] private float contactInterval;

        public MonsterBrainType BrainType => brainType;

        public MonsterStats Stats => stats;
        public float MaxHealth => maxHealth > 0f ? maxHealth : stats != null ? stats.MaxHealth : 0f;
        public float MoveSpeed => moveSpeed >= 0f ? moveSpeed : stats != null ? stats.MoveSpeed : 0f;
        public float ContactDamage => contactDamage >= 0f ? contactDamage : stats != null ? stats.ContactDamage : 0f;
        public float ContactInterval => contactInterval > 0f ? contactInterval : stats != null ? stats.ContactInterval : 0.05f;
        public MonsterViewHub Views => viewHub;
        public Transform Target { get; private set; }
        public bool IsAlive => health > 0f && !hasDespawned;
        public event Action<MonsterActor> Died;
        public event Action<MonsterViewContext> ViewEvent;

        private void Awake()
        {
            if (stats != null && maxHealth <= 0f)
            {
                maxHealth = stats.MaxHealth;
                moveSpeed = stats.MoveSpeed;
                contactDamage = stats.ContactDamage;
                contactInterval = stats.ContactInterval;
            }
            if (renderers == null || renderers.Length == 0)
                renderers = GetComponentsInChildren<Renderer>();

            if (viewHub == null && !TryGetComponent(out viewHub))
                viewHub = gameObject.AddComponent<MonsterViewHub>();
            viewHub.Initialize(this);
            if (attackResolver == null && !TryGetComponent(out attackResolver))
                attackResolver = gameObject.AddComponent<MonsterAttackResolver>();
            attackResolver.Initialize(this);

            ApplyBrain();
        }

        private void Update()
        {
            if (appliedBrainType != brainType)
                ApplyBrain();
            if (!IsAlive)
                return;

            if (activeBrain != null && activeBrain.isActiveAndEnabled)
                activeBrain.Tick();
        }

        public void SetBrainType(MonsterBrainType type)
        {
            brainType = type;
            if (activeBrain == null || appliedBrainType != type)
                ApplyBrain();
        }

        private void ApplyBrain()
        {
            if (spawned && activeBrain != null)
                activeBrain.OnMonsterDespawned();

            switch (brainType)
            {
                case MonsterBrainType.Charge: activeBrain = GetOrAddBrain<ChargeBrain>(); break;
                case MonsterBrainType.Orbit: activeBrain = GetOrAddBrain<OrbitBrain>(); break;
                case MonsterBrainType.KeepDistance: activeBrain = GetOrAddBrain<KeepDistanceBrain>(); break;
                case MonsterBrainType.Pulse: activeBrain = GetOrAddBrain<PulseBrain>(); break;
                case MonsterBrainType.Cleave: activeBrain = GetOrAddBrain<CleaveBrain>(); break;
                case MonsterBrainType.RingBurst: activeBrain = GetOrAddBrain<RingBurstBrain>(); break;
                default: activeBrain = GetOrAddBrain<DirectChaseBrain>(); break;
            }
            appliedBrainType = brainType;
            activeBrain.Initialize(this);
            if (spawned)
                activeBrain.OnMonsterSpawned();
        }

        private T GetOrAddBrain<T>() where T : MonsterBrain
        {
            return TryGetComponent<T>(out var brain) ? brain : gameObject.AddComponent<T>();
        }

        public void SetTarget(Transform target) => Target = target;

        public void Configure(
            MonsterStats monsterStats,
            MonsterBrainType type,
            MonsterDefinitionOverride.StatsOverride dataOverride = null)
        {
            stats = monsterStats;
            brainType = type;
            maxHealth = Override(dataOverride?.maxHealth ?? -1f, stats != null ? stats.MaxHealth : 0f);
            moveSpeed = Override(dataOverride?.moveSpeed ?? -1f, stats != null ? stats.MoveSpeed : 0f);
            contactDamage = Override(dataOverride?.contactDamage ?? -1f, stats != null ? stats.ContactDamage : 0f);
            contactInterval = Override(dataOverride?.contactInterval ?? -1f, stats != null ? stats.ContactInterval : 0.05f);
            if (Application.isPlaying && (activeBrain == null || appliedBrainType != type))
                ApplyBrain();
        }

        private static float Override(float candidate, float fallback) => candidate >= 0f ? candidate : fallback;

        public void PublishViewEvent(
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
            var context = new MonsterViewContext(this, viewEvent, target, position, direction, value, key, source,
                range, attackPlan);
            viewHub.Publish(context);
            ViewEvent?.Invoke(context);
        }

        public void BeginAttack(
            string key = null,
            Transform target = null,
            float windup = 0f,
            Vector3? direction = null,
            float range = 0f) =>
            PublishViewEvent(MonsterViewEvent.AttackWindup, target ?? Target, direction: direction,
                value: windup, key: key, range: range);

        public void StartAttack(
            string key = null,
            Transform target = null,
            Vector3? direction = null,
            float range = 0f) =>
            PublishViewEvent(MonsterViewEvent.AttackStarted, target ?? Target, direction: direction,
                key: key, range: range);

        public void ReportAttackHit(string key = null, Transform target = null, Vector3? position = null, float damage = 0f) =>
            PublishViewEvent(MonsterViewEvent.AttackHit, target ?? Target, position, value: damage, key: key);

        public void EndAttack(string key = null) => PublishViewEvent(MonsterViewEvent.AttackEnded, key: key);

        public void BeginAttack(in MonsterAttackPlan plan, Transform target = null) =>
            PublishViewEvent(MonsterViewEvent.AttackWindup, target ?? Target, plan.Origin, plan.Direction,
                plan.WindupDuration, plan.Key, this, plan.Range, plan);

        public void StartAttack(in MonsterAttackPlan plan, Transform target = null)
        {
            PublishViewEvent(MonsterViewEvent.AttackStarted, target ?? Target, plan.Origin, plan.Direction,
                plan.ActiveDuration, plan.Key, this, plan.Range, plan);
            attackResolver.Resolve(plan);
        }

        public void EndAttack(in MonsterAttackPlan plan) =>
            PublishViewEvent(MonsterViewEvent.AttackEnded, position: plan.Origin, direction: plan.Direction,
                key: plan.Key, range: plan.Range, attackPlan: plan);

        public void TakeDamage(float amount, object source = null)
        {
            if (!IsAlive || amount <= 0f)
                return;

            health -= amount;
            PublishViewEvent(MonsterViewEvent.Damaged, value: amount, source: source);
            if (health <= 0f)
            {
                health = 0f;
                hasDespawned = true;
                PublishViewEvent(MonsterViewEvent.Died, source: source);
                Died?.Invoke(this);
            }
        }

        public void OnPoolSpawned()
        {
            if (stats == null)
            {
                Debug.LogError($"{name} needs MonsterStats.", this);
                return;
            }

            health = MaxHealth;
            hasDespawned = false;
            if (appliedBrainType != brainType)
                ApplyBrain();
            spawned = true;
            foreach (var renderer in renderers)
                renderer.enabled = true;
            activeBrain.OnMonsterSpawned();
            PublishViewEvent(MonsterViewEvent.Spawned);
        }

        public void OnPoolDespawned()
        {
            Target = null;
            spawned = false;
            health = 0f;
            hasDespawned = true;
            if (activeBrain != null)
                activeBrain.OnMonsterDespawned();
            PublishViewEvent(MonsterViewEvent.Despawned);
            viewHub.ResetViews();
        }
    }
}
