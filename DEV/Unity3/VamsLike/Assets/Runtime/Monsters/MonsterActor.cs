using System;
using TTest.UnityUtilities.Pooling;
using UnityEngine;
using VamSurvivalLike.Combat;

namespace VamSurvivalLike.Monsters
{
    /// <summary>Health, pool lifecycle, and enum-selected AI. Only the selected brain is ticked.</summary>
    public sealed class MonsterActor : MonoBehaviour, IDamageable, IPoolable
    {
        [SerializeField] private MonsterStats stats;
        [SerializeField] private MonsterBrainType brainType;
        [SerializeField] private Renderer[] renderers;

        private float health;
        private bool hasDespawned;
        private bool spawned;
        private MonsterBrain activeBrain;
        private MonsterBrainType appliedBrainType;

        public MonsterBrainType BrainType => brainType;

        public MonsterStats Stats => stats;
        public Transform Target { get; private set; }
        public bool IsAlive => health > 0f && !hasDespawned;
        public event Action<MonsterActor> Died;

        private void Awake()
        {
            if (renderers == null || renderers.Length == 0)
                renderers = GetComponentsInChildren<Renderer>();

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
            if (Application.isPlaying && (activeBrain == null || appliedBrainType != type))
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

        public void TakeDamage(float amount, object source = null)
        {
            if (!IsAlive || amount <= 0f)
                return;

            health -= amount;
            if (health <= 0f)
            {
                health = 0f;
                hasDespawned = true;
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

            health = stats.MaxHealth;
            hasDespawned = false;
            if (appliedBrainType != brainType)
                ApplyBrain();
            spawned = true;
            foreach (var renderer in renderers)
                renderer.enabled = true;
            activeBrain.OnMonsterSpawned();
        }

        public void OnPoolDespawned()
        {
            Target = null;
            spawned = false;
            health = 0f;
            hasDespawned = true;
            if (activeBrain != null)
                activeBrain.OnMonsterDespawned();
        }
    }
}
