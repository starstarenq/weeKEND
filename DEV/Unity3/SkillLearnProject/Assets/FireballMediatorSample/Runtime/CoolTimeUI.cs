using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FireballMediatorSample
{
    public sealed class CoolTimeUI : MonoBehaviour
    {
        [SerializeField] SkillManager manager;
        [SerializeField] int skillId;
        [SerializeField] Image cooldownFill;
        [SerializeField] TextMeshProUGUI remainingText;
        bool subscribed;
        bool counting;

        public void Configure(SkillManager source, int id, Image fill, TextMeshProUGUI label)
        {
            Unsubscribe();
            manager = source; skillId = id; cooldownFill = fill; remainingText = label;
            if (isActiveAndEnabled) Subscribe();
        }
        void OnEnable() => Subscribe();
        void OnDisable() => Unsubscribe();
        void Subscribe()
        {
            if (subscribed || manager == null) return;
            manager.OnSkillUsed += OnSkillUsed;
            subscribed = true;
            Refresh(); // Restore the remaining cooldown if this UI was temporarily hidden.
        }
        void Unsubscribe()
        {
            if (subscribed && manager != null) manager.OnSkillUsed -= OnSkillUsed;
            subscribed = false;
        }
        // Public callback can also be wired to an int UnityEvent by other presenters.
        public void OnSkillUsed(int usedSkillId)
        {
            if (usedSkillId == skillId) Refresh();
        }
        void Update() { if (counting) Refresh(); }
        void Refresh()
        {
            float remaining = manager != null ? manager.RemainCooldown(skillId) : 0;
            float duration = manager != null ? manager.CooldownDuration(skillId) : 0;
            counting = remaining > 0;
            if (cooldownFill != null)
            {
                cooldownFill.fillAmount = duration > 0 ? Mathf.Clamp01(remaining / duration) : 0;
                cooldownFill.enabled = counting;
            }
            if (remainingText != null)
                remainingText.text = counting ? (Mathf.Ceil(remaining * 10) / 10).ToString("0.0") + "s" : string.Empty;
        }
    }
}
