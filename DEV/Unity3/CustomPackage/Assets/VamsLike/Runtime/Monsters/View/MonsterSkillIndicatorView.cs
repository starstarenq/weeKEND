using System.Collections;
using SysKill.SkillIndicators;
using UnityEngine;
using VamSurvivalLike.Combat;

namespace VamSurvivalLike.Monsters.View
{
    /// <summary>
    /// Presentation-only bridge from monster attack events to SkillIndicatorLab.
    /// Hit detection is opt-in because gameplay damage normally belongs to the combat layer.
    /// </summary>
    public sealed class MonsterSkillIndicatorView : MonsterViewBehaviour
    {
        [SerializeField] private SkillIndicator indicator;
        [SerializeField] private string attackKey;
        [SerializeField] private Vector3 groundOffset = new(0f, 0.02f, 0f);
        [SerializeField] private bool followActorDuringWindup = true;
        [SerializeField] private bool syncRangeFromAttack = true;
        [SerializeField] private bool pinToWindupWorldPosition = true;

        private Coroutine windupRoutine;
        private Vector3 attackDirection;
        private Transform attackTarget;
        private Vector3 indicatorOrigin;
        private bool directionIsLocked;
        private bool attackVisible;

        public void Configure(
            SkillIndicator targetIndicator,
            string key,
            bool followDuringWindup = true,
            bool syncRange = true,
            bool pinToWorld = true)
        {
            indicator = targetIndicator;
            attackKey = key;
            followActorDuringWindup = followDuringWindup;
            syncRangeFromAttack = syncRange;
            pinToWindupWorldPosition = pinToWorld;
        }

        public override void Initialize(MonsterActor actor)
        {
            base.Initialize(actor);
            if (indicator != null)
            {
                indicator.owner = actor.transform;
                Hide();
            }
        }

        public override void Present(in MonsterViewContext context)
        {
            if (indicator == null || !Matches(context))
                return;

            switch (context.Event)
            {
                case MonsterViewEvent.Spawned:
                    Hide();
                    break;
                case MonsterViewEvent.AttackWindup:
                    BeginWindup(context);
                    break;
                case MonsterViewEvent.AttackStarted:
                    StartAttack(context);
                    break;
                case MonsterViewEvent.AttackEnded:
                case MonsterViewEvent.Despawned:
                case MonsterViewEvent.Died:
                    Hide();
                    break;
            }
        }

        public override void ResetView() => Hide();

        private void OnDisable()
        {
            if (indicator != null)
                Hide();
        }

        private bool Matches(in MonsterViewContext context) =>
            string.IsNullOrEmpty(attackKey) || attackKey == context.Key;

        private void BeginWindup(in MonsterViewContext context)
        {
            StopWindup();
            attackTarget = context.Target;
            attackDirection = ResolveDirection(context);
            directionIsLocked = context.HasDirection;
            indicatorOrigin = Actor.transform.position + groundOffset;
            ApplyAttackGeometry(context);
            PlaceIndicator();
            indicator.gameObject.SetActive(true);
            indicator.opacity = 1f;
            indicator.impact = 0f;
            indicator.SetProgress(0f);
            attackVisible = true;
            windupRoutine = StartCoroutine(AnimateWindup(Mathf.Max(0.01f, context.Value)));
        }

        private IEnumerator AnimateWindup(float duration)
        {
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                if (followActorDuringWindup && !directionIsLocked)
                {
                    if (attackTarget != null)
                    {
                        attackDirection = attackTarget.position - Actor.transform.position;
                        attackDirection.y = 0f;
                    }
                    indicatorOrigin = Actor.transform.position + groundOffset;
                    PlaceIndicator();
                }
                indicator.SetProgress(elapsed / duration);
                yield return null;
            }

            indicator.SetProgress(1f);
            windupRoutine = null;
        }

        private void StartAttack(in MonsterViewContext context)
        {
            StopWindup();
            if (!attackVisible)
                indicatorOrigin = Actor.transform.position + groundOffset;
            attackDirection = ResolveDirection(context);
            directionIsLocked = context.HasDirection;
            ApplyAttackGeometry(context);
            PlaceIndicator();
            indicator.gameObject.SetActive(true);
            attackVisible = true;
            indicator.opacity = 1f;
            indicator.SetProgress(1f);

            indicator.impact = 1f;
            indicator.Apply();
        }

        private Vector3 ResolveDirection(in MonsterViewContext context)
        {
            var direction = context.Direction;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f && context.Target != null)
            {
                direction = context.Target.position - Actor.transform.position;
                direction.y = 0f;
            }
            return direction.sqrMagnitude > 0.0001f ? direction.normalized : Actor.transform.forward;
        }

        private void PlaceIndicator()
        {
            if (indicator != null && Actor != null)
                indicator.Place(indicatorOrigin, attackDirection);
        }

        private void LateUpdate()
        {
            if (attackVisible && pinToWindupWorldPosition && directionIsLocked)
                PlaceIndicator();
        }

        private void ApplyRange(float range)
        {
            if (!syncRangeFromAttack || indicator == null || range <= 0f)
                return;
            if (indicator.shape == SkillIndicator.Shape.Rectangle)
                indicator.length = range;
            else
                indicator.radius = range;
        }

        private void ApplyAttackGeometry(in MonsterViewContext context)
        {
            if (!context.HasAttackPlan)
            {
                ApplyRange(context.Range);
                return;
            }

            var plan = context.AttackPlan;
            indicator.shape = (SkillIndicator.Shape)plan.Shape;
            indicator.width = plan.Width;
            indicator.angle = plan.Angle;
            indicator.innerRadiusRatio = plan.InnerRadiusRatio;
            if (plan.Shape == AttackShape.Rectangle)
                indicator.length = plan.Range;
            else
                indicator.radius = plan.Range;
            indicator.Apply();
        }

        private void Hide()
        {
            StopWindup();
            attackTarget = null;
            attackVisible = false;
            directionIsLocked = false;
            if (indicator == null)
                return;
            indicator.EndHitWindow();
            indicator.progress = 0f;
            indicator.impact = 0f;
            indicator.opacity = 0f;
            indicator.Apply();
            indicator.gameObject.SetActive(false);
        }

        private void StopWindup()
        {
            if (windupRoutine == null)
                return;
            StopCoroutine(windupRoutine);
            windupRoutine = null;
        }
    }
}
