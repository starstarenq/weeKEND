using UnityEngine;

namespace FireballMediatorSample
{
    public sealed class FireballSampleHUD : MonoBehaviour
    {
        [SerializeField] SkillManager manager;
        [SerializeField] FireballTarget target;
        string status = "Ready. Use the key shown above the hotbar or click its slot.";
        int casts;
        public void Configure(SkillManager mediator, FireballTarget dummy) { manager = mediator; target = dummy; }
        void OnEnable()
        {
            if (manager == null) return;
            manager.OnSkillUsed += Used;
            manager.OnSkillRejected += Rejected;
        }
        void OnDisable()
        {
            if (manager == null) return;
            manager.OnSkillUsed -= Used;
            manager.OnSkillRejected -= Rejected;
        }
        void Used(int id) { casts++; status = "OnSkillUsed(0): Fireball launched"; }
        void Rejected(int id, string reason) => status = "Rejected: " + reason;
        void OnGUI()
        {
            if (manager == null) return;
            GUILayout.BeginArea(new Rect(16, 16, 440, 240), GUI.skin.box);
            GUILayout.Label("2D FIREBALL / INPUT SYSTEM MEDIATOR");
            GUILayout.Label("Use the hotbar binding: fire right (+X) at the target");
            GUILayout.Label("performed > UseSkill(0) > CanUse > Cast > OnSkillUsed");
            GUILayout.Space(10);
            GUILayout.Label($"Mana {manager.Mana:0}/{manager.MaxMana:0}   Cooldown {manager.RemainCooldown(0):0.0}s");
            manager.CanUse(0, out string reason);
            GUILayout.Label("Gate: " + (string.IsNullOrEmpty(reason) ? "Ready" : reason));
            GUILayout.Label($"Successful casts: {casts}");
            if (target != null) GUILayout.Label($"Target HP: {target.Health:0}/100   Hits: {target.Hits} (respawns after 2s)");
            GUILayout.Space(10);
            GUILayout.Label(status);
            GUILayout.EndArea();
        }
    }
}
