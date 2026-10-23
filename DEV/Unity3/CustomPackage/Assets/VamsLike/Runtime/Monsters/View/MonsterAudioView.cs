using System;
using UnityEngine;

namespace VamSurvivalLike.Monsters.View
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class MonsterAudioView : MonsterViewBehaviour
    {
        [Serializable]
        private struct Cue
        {
            public MonsterViewEvent on;
            public string key;
            public AudioClip clip;
            [Range(0f, 1f)] public float volume;
        }

        [SerializeField] private AudioSource source;
        [SerializeField] private Cue[] cues;

        private void Awake()
        {
            if (source == null)
                source = GetComponent<AudioSource>();
        }

        public override void Present(in MonsterViewContext context)
        {
            foreach (var cue in cues)
            {
                if (cue.on != context.Event || (!string.IsNullOrEmpty(cue.key) && cue.key != context.Key) || cue.clip == null)
                    continue;

                source.PlayOneShot(cue.clip, cue.volume <= 0f ? 1f : cue.volume);
            }
        }

        public override void ResetView() => source.Stop();
    }
}
