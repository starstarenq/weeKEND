using UnityEngine;

namespace VamSurvivalLike.Monsters
{
    /// <summary>Composable AI unit. Add a derived component to a monster prefab for a new behaviour.</summary>
    public abstract class MonsterBrain : MonoBehaviour
    {
        protected MonsterActor Actor { get; private set; }

        public virtual void Initialize(MonsterActor actor) => Actor = actor;
        public virtual void OnMonsterSpawned() { }
        public virtual void Tick() { }
        public virtual void OnMonsterDespawned() { }

        protected Vector3 TargetOffset()
        {
            var offset = Actor.Target.position - transform.position;
            offset.y = 0f;
            return offset;
        }

        protected void Move(Vector3 direction, float speed, float maxDistance = float.PositiveInfinity)
        {
            if (direction.sqrMagnitude < 0.0001f)
                return;
            direction.Normalize();
            transform.position += direction * Mathf.Min(Mathf.Max(0f, maxDistance), speed * Time.deltaTime);
            transform.rotation = Quaternion.RotateTowards(transform.rotation,
                Quaternion.LookRotation(direction), 720f * Time.deltaTime);
        }
    }
}
