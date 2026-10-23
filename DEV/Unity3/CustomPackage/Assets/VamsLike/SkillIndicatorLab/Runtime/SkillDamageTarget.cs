using System;
using UnityEngine;

namespace SysKill.SkillIndicators
{
    /// <summary>Add to a target root with a Collider on itself or a child.</summary>
    public class SkillDamageTarget : MonoBehaviour
    {
        [Min(1)] public int maxHealth = 100;
        [Min(0)] public int armor;
        public Vector3 popupOffset = Vector3.up;
        public int CurrentHealth { get; private set; }
        public event Action<int, int> HealthChanged;
        public event Action Died;

        protected virtual void Awake() => RestoreHealth();
        public void RestoreHealth()
        {
            CurrentHealth = Mathf.Max(1, maxHealth);
            HealthChanged?.Invoke(CurrentHealth, CurrentHealth);
        }

        // Override to bridge an existing combat system; return the actual HP removed.
        public virtual int ApplyDamage(int amount)
        {
            if (CurrentHealth <= 0) return 0;
            int actual = Mathf.Min(CurrentHealth, Mathf.Max(0, Mathf.Max(0, amount) - Mathf.Max(0, armor)));
            CurrentHealth -= actual;
            HealthChanged?.Invoke(CurrentHealth, Mathf.Max(1, maxHealth));
            if (CurrentHealth == 0) Died?.Invoke();
            return actual;
        }
    }
}
