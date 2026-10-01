using UnityEngine;

namespace FireballMediatorSample
{
    [CreateAssetMenu(menuName = "Skills/Fireball Sample")]
    public sealed class FireballSkillData : SkillData
    {
        public FireballProjectile projectilePrefab;
        [Min(1)] public float speed = 12;
        [Min(0.1f)] public float lifetime = 3;
        public override bool IsConfigured => projectilePrefab != null;

        public override void Cast(Transform caster, float runtimeDamage)
        {
            var projectile = Instantiate(projectilePrefab, caster.position, caster.rotation);
            projectile.Launch(speed, lifetime, runtimeDamage);
        }
    }
}
