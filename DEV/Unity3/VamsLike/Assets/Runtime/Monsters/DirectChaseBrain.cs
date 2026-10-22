using UnityEngine;

namespace VamSurvivalLike.Monsters
{
    /// <summary>Simple direct pursuit: ideal as the first capsule/cube enemy AI.</summary>
    public sealed class DirectChaseBrain : MonsterBrain
    {
        [SerializeField, Min(0f)] private float stoppingDistance = 1.1f;
        [SerializeField, Min(0f)] private float turnSpeed = 720f;

        public override void Tick()
        {
            if (Actor.Target == null || !Actor.IsAlive)
                return;

            var direction = Actor.Target.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude <= stoppingDistance * stoppingDistance)
                return;

            direction.Normalize();
            transform.position += direction * (Actor.Stats.MoveSpeed * Time.deltaTime);
            var rotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, turnSpeed * Time.deltaTime);
        }
    }
}
