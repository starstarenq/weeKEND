using UnityEngine;

namespace VamSurvivalLike.Monsters
{
    public sealed class OrbitBrain : MonsterBrain
    {
        [SerializeField, Min(0.1f)] private float orbitRadius = 3f;
        [SerializeField] private bool clockwise = true;

        public override void Tick()
        {
            if (Actor.Target == null || !Actor.IsAlive) return;
            var offset = TargetOffset();
            var radial = offset.sqrMagnitude > 0.001f ? offset.normalized : transform.forward;
            var tangent = Vector3.Cross(Vector3.up, radial) * (clockwise ? 1f : -1f);
            var correction = Mathf.Clamp((offset.magnitude - orbitRadius) / Mathf.Max(0.1f, orbitRadius), -1f, 1f);
            Move(tangent + radial * correction * 2f, Actor.Stats.MoveSpeed);
        }
    }
}
