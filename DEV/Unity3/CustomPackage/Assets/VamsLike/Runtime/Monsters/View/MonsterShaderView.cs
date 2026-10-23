using System;
using UnityEngine;

namespace VamSurvivalLike.Monsters.View
{
    /// <summary>Writes Shader Graph exposed float properties without instantiating materials.</summary>
    public sealed class MonsterShaderView : MonsterViewBehaviour
    {
        [Serializable]
        private struct Cue
        {
            public MonsterViewEvent on;
            public string key;
            public string property;
            public float value;
            public float resetValue;
        }

        [SerializeField] private Renderer[] renderers;
        [SerializeField] private Cue[] cues;
        private readonly MaterialPropertyBlock block = new();

        private void Awake()
        {
            if (renderers == null || renderers.Length == 0)
                renderers = GetComponentsInChildren<Renderer>(true);
        }

        public override void Present(in MonsterViewContext context)
        {
            foreach (var cue in cues)
            {
                if (cue.on != context.Event || string.IsNullOrEmpty(cue.property) ||
                    (!string.IsNullOrEmpty(cue.key) && cue.key != context.Key))
                    continue;

                var propertyId = Shader.PropertyToID(cue.property);
                foreach (var targetRenderer in renderers)
                {
                    if (targetRenderer == null)
                        continue;
                    targetRenderer.GetPropertyBlock(block);
                    block.SetFloat(propertyId, cue.value);
                    targetRenderer.SetPropertyBlock(block);
                }
            }
        }

        public override void ResetView()
        {
            foreach (var cue in cues)
            {
                if (string.IsNullOrEmpty(cue.property))
                    continue;
                var propertyId = Shader.PropertyToID(cue.property);
                foreach (var targetRenderer in renderers)
                {
                    if (targetRenderer == null)
                        continue;
                    targetRenderer.GetPropertyBlock(block);
                    block.SetFloat(propertyId, cue.resetValue);
                    targetRenderer.SetPropertyBlock(block);
                }
            }
        }
    }
}
