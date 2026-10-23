using System.Collections.Generic;
using UnityEngine;

namespace VamSurvivalLike.Combat
{
    /// <summary>Deals periodic damage to all valid targets within a circle around this object.</summary>
    public sealed class AreaDamageEmitter : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float radius = 2.5f;
        [SerializeField, Min(0f)] private float damagePerTick = 10f;
        [SerializeField, Min(0.01f)] private float tickInterval = 0.5f;
        [SerializeField] private LayerMask targetLayers;
        [SerializeField] private bool damageOnEnable = true;

        private readonly Collider[] hits = new Collider[64];
        private readonly HashSet<IDamageable> uniqueTargets = new();
        private float nextTickTime;

        private void OnEnable()
        {
            nextTickTime = damageOnEnable ? Time.time : Time.time + tickInterval;
        }

        private void Update()
        {
            if (Time.time < nextTickTime)
                return;

            nextTickTime = Time.time + tickInterval;
            DamageTargets();
        }

        private void DamageTargets()
        {
            var count = Physics.OverlapSphereNonAlloc(transform.position, radius, hits, targetLayers,
                QueryTriggerInteraction.Collide);
            uniqueTargets.Clear();

            for (var index = 0; index < count; index++)
            {
                var behaviours = hits[index].GetComponentsInParent<MonoBehaviour>();
                foreach (var behaviour in behaviours)
                {
                    if (behaviour is IDamageable damageable && damageable.IsAlive)
                    {
                        uniqueTargets.Add(damageable);
                        break;
                    }
                }
            }

            foreach (var target in uniqueTargets)
                target.TakeDamage(damagePerTick, this);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.25f, 0.1f, 0.25f);
            Gizmos.DrawSphere(transform.position, radius);
        }
    }
}
