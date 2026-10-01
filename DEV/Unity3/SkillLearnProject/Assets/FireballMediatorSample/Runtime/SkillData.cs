using UnityEngine;

namespace FireballMediatorSample
{
    public abstract class SkillData : ScriptableObject
    {
        [Min(0)] public float cooldown = 1.2f;
        [Min(0)] public float manaCost = 20;
        [Min(0)] public float damage = 25;
        public bool initiallyUnlocked = true;
        public AudioClip castSound;
        public abstract bool IsConfigured { get; }
        public abstract void Cast(Transform caster, float runtimeDamage);
    }
}
