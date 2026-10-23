using System.Collections;
using UnityEngine;

namespace VamSurvivalLike.Monsters.View
{
    /// <summary>Small scene-only driver that demonstrates the public MonsterActor view API.</summary>
    public sealed class MonsterViewDemoController : MonoBehaviour
    {
        [SerializeField] private MonsterActor actor;
        [SerializeField] private Transform target;
        [SerializeField] private string attackKey = "DemoAttack";
        [SerializeField, Min(0.1f)] private float windupDuration = 1.5f;
        [SerializeField, Min(0.1f)] private float activeDuration = 0.45f;
        [SerializeField, Min(0f)] private float repeatDelay = 0.75f;

        public void Configure(MonsterActor owner, Transform attackTarget, string key)
        {
            actor = owner;
            target = attackTarget;
            attackKey = key;
        }

        private IEnumerator Start()
        {
            while (actor != null)
            {
                var direction = DirectionToTarget();
                actor.BeginAttack(attackKey, target, windupDuration);
                yield return new WaitForSeconds(windupDuration);

                direction = DirectionToTarget();
                actor.StartAttack(attackKey, target, direction);
                yield return new WaitForSeconds(activeDuration);

                actor.EndAttack(attackKey);
                yield return new WaitForSeconds(repeatDelay);
            }
        }

        private Vector3 DirectionToTarget()
        {
            if (target == null)
                return actor.transform.forward;
            var direction = target.position - actor.transform.position;
            direction.y = 0f;
            return direction.sqrMagnitude > 0.0001f ? direction.normalized : actor.transform.forward;
        }
    }
}
