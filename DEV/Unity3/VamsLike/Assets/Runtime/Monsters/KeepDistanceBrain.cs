using UnityEngine;

namespace VamSurvivalLike.Monsters
{
    public sealed class KeepDistanceBrain : MonsterBrain
    {
        [SerializeField, Min(0f)] private float desiredDistance = 4f;
        [SerializeField, Min(0f)] private float tolerance = 0.4f;

        public override void Tick()
        {
            if (Actor.Target == null || !Actor.IsAlive) return;
            var offset = TargetOffset();
            var error = offset.magnitude - desiredDistance;
            if (Mathf.Abs(error) <= tolerance) return;
            var direction = offset.sqrMagnitude > 0.001f ? offset.normalized : transform.forward;
            Move(direction * Mathf.Sign(error), Actor.Stats.MoveSpeed, Mathf.Abs(error) - tolerance);
        }
    }
}
