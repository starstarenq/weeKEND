using UnityEngine;

namespace SysKill.SkillIndicators
{
    /// <summary>Optional demonstration only. Disable this component when driving indicators from combat code.</summary>
    public sealed class SkillIndicatorDemo : MonoBehaviour
    {
        public SkillIndicator indicator;
        public Transform actor;
        public Transform hitEffect;
        [Min(0.1f)] public float chargeSeconds = 2.4f;
        [Min(0.1f)] public float activeSeconds = 0.65f;
        [Min(0f)] public float cooldownSeconds = 0.8f;
        public float timeOffset;
        public bool loop = true;
        public bool createDamageTarget = true;
        SkillDamageTarget demoTarget;
        float elapsed;
        bool triggered;
        Vector3 actorStart;

        void OnEnable()
        {
            elapsed = timeOffset;
            triggered = false;
            if (actor != null) actorStart = actor.localPosition;
            if (demoTarget != null) demoTarget.gameObject.SetActive(true);
        }

        void Update()
        {
            if (indicator == null) return;
            float charge = Mathf.Max(0.1f, chargeSeconds);
            float active = Mathf.Max(0.1f, activeSeconds);
            float cycle = charge + active + Mathf.Max(0, cooldownSeconds);
            elapsed += Time.deltaTime;
            if (elapsed >= cycle && loop) { elapsed %= cycle; triggered = false; }
            float phase = (elapsed - charge) / active;
            bool firing = phase >= 0 && phase < 1;
            indicator.progress = Mathf.Clamp01(elapsed / charge);
            indicator.opacity = elapsed < charge + active ? 1 : 0;
            indicator.impact = firing ? 1 - phase : 0;
            if (firing && !triggered)
            {
                if (demoTarget != null && demoTarget.CurrentHealth == 0) demoTarget.RestoreHealth();
                indicator.hitDuration = active;
                indicator.Trigger(); triggered = true;
            }
            if (actor != null)
                actor.localPosition = actorStart + (indicator.shape == SkillIndicator.Shape.Rectangle && firing
                    ? Vector3.forward * indicator.length * phase : Vector3.zero);
            if (hitEffect != null)
            {
                hitEffect.gameObject.SetActive(firing);
                float scale = indicator.shape == SkillIndicator.Shape.Circle
                    ? Mathf.Lerp(0.2f, indicator.radius * 1.8f, Mathf.Clamp01(phase)) : 0.5f;
                hitEffect.localScale = new Vector3(scale, 0.12f + Mathf.Sin(Mathf.Clamp01(phase) * Mathf.PI) * 0.7f, scale);
            }
            indicator.Apply();
        }

        void Start()
        {
            if (!createDamageTarget || indicator == null) return;
            var target = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            target.name = "Demo Damage Target";
            target.transform.SetParent(indicator.transform, false);
            float distance = indicator.shape == SkillIndicator.Shape.Rectangle ? indicator.length * 0.65f
                : indicator.shape == SkillIndicator.Shape.Donut ? indicator.radius * (1 + indicator.innerRadiusRatio) * 0.5f
                : indicator.radius * 0.5f;
            target.transform.localPosition = new Vector3(0, 0.45f, distance);
            target.transform.localScale = new Vector3(0.4f, 0.45f, 0.4f);
            demoTarget = target.AddComponent<SkillDamageTarget>();
            demoTarget.popupOffset = Vector3.up * 0.65f;
        }

        void OnDisable()
        {
            if (indicator != null) indicator.EndHitWindow();
            if (demoTarget != null) demoTarget.gameObject.SetActive(false);
        }
    }
}
