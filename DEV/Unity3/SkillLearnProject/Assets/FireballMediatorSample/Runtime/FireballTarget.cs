using UnityEngine;

namespace FireballMediatorSample
{
    public sealed class FireballTarget : MonoBehaviour
    {
        public float Health { get; private set; } = 100;
        public int Hits { get; private set; }
        float resetAt;
        Vector3 originalScale;
        void Awake() => originalScale = transform.localScale;
        public void TakeDamage(float damage)
        {
            if (Health <= 0) return;
            Health = Mathf.Max(0, Health - damage); Hits++;
            if (Health <= 0) resetAt = Time.time + 2;
        }
        void Update()
        {
            if (Health <= 0 && Time.time >= resetAt) Health = 100;
            transform.localScale = originalScale * (Health <= 0 ? 0.25f : 1);
        }
    }
}
