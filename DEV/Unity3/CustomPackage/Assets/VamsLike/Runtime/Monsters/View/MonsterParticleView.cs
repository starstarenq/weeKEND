using System;
using UnityEngine;

namespace VamSurvivalLike.Monsters.View
{
    public sealed class MonsterParticleView : MonsterViewBehaviour
    {
        [Serializable]
        private struct Cue
        {
            public MonsterViewEvent on;
            public string key;
            public ParticleSystem particles;
            public bool stopInsteadOfPlay;
        }

        [SerializeField] private Cue[] cues;

        public override void Present(in MonsterViewContext context)
        {
            foreach (var cue in cues)
            {
                if (cue.on != context.Event || (!string.IsNullOrEmpty(cue.key) && cue.key != context.Key) || cue.particles == null)
                    continue;

                if (cue.stopInsteadOfPlay)
                    cue.particles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                else
                    cue.particles.Play(true);
            }
        }

        public override void ResetView()
        {
            foreach (var cue in cues)
                if (cue.particles != null)
                    cue.particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}
