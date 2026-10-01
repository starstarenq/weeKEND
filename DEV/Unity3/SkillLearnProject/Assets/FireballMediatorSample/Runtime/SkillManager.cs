using System;
using UnityEngine;

namespace FireballMediatorSample
{
    [DefaultExecutionOrder(-100)]
    public sealed class SkillManager : MonoBehaviour
    {
        public static SkillManager Instance { get; private set; }
        [SerializeField] SkillData[] skills;
        [SerializeField] Transform caster;
        [SerializeField] AudioSource audioSource;
        [SerializeField, Min(1)] float maxMana = 100;
        [SerializeField, Min(0)] float manaPerSecond = 8;
        [SerializeField, Range(0, 1)] float sfxVolume = 0.5f;
        sealed class SkillState
        {
            public SkillData data;
            public bool unlocked;
            public float damage;
            public float lastUsedTime = float.NegativeInfinity;
        }
        SkillState[] states;
        public float Mana { get; private set; }
        public float MaxMana => maxMana;
        public event Action<int> OnSkillUsed;
        public event Action<int, string> OnSkillRejected;

        public void Configure(SkillData data, Transform origin, AudioSource source, float mana, float regeneration)
        {
            skills = new[] { data }; caster = origin; audioSource = source;
            maxMana = mana; manaPerSecond = regeneration;
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            Mana = maxMana;
            states = new SkillState[skills == null ? 0 : skills.Length];
            for (int i = 0; i < states.Length; i++)
                if (skills[i] != null)
                    states[i] = new SkillState { data = skills[i], unlocked = skills[i].initiallyUnlocked, damage = skills[i].damage };
        }

        void Update() => Mana = Mathf.Min(maxMana, Mana + manaPerSecond * Time.deltaTime);
        void OnDestroy() { if (Instance == this) Instance = null; }
        bool Valid(int id) => states != null && id >= 0 && id < states.Length && states[id] != null;
        public float CooldownDuration(int id) => Valid(id) ? states[id].data.cooldown : 0;
        public float RemainCooldown(int id) => Valid(id)
            ? Mathf.Max(0, states[id].lastUsedTime + states[id].data.cooldown - Time.time) : 0;

        public bool CanUse(int id, out string reason)
        {
            if (!Valid(id)) reason = "Invalid skill";
            else if (!states[id].unlocked) reason = "Locked";
            else if (RemainCooldown(id) > 0) reason = "Cooldown";
            else if (Mana < states[id].data.manaCost) reason = "Not enough mana";
            else if (caster == null || !states[id].data.IsConfigured) reason = "Missing cast configuration";
            else { reason = string.Empty; return true; }
            return false;
        }

        public void UseSkill(int id)
        {
            if (!CanUse(id, out string reason)) { OnSkillRejected?.Invoke(id, reason); return; }
            SkillState state = states[id];
            state.lastUsedTime = Time.time;
            Mana -= state.data.manaCost;
            state.data.Cast(caster, state.damage);
            PlaySound(state.data.castSound);
            OnSkillUsed?.Invoke(id);
        }

        public void PlaySound(AudioClip clip)
        {
            if (clip != null && audioSource != null) audioSource.PlayOneShot(clip, sfxVolume);
        }
    }
}
