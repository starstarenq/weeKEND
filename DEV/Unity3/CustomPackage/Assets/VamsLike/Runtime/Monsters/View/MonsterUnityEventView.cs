using System;
using UnityEngine;
using UnityEngine.Events;

namespace VamSurvivalLike.Monsters.View
{
    /// <summary>Inspector bridge for UI, animation and third-party effect components.</summary>
    public sealed class MonsterUnityEventView : MonsterViewBehaviour
    {
        [Serializable] public sealed class ContextEvent : UnityEvent<MonsterViewContext> { }

        [Serializable]
        private struct Binding
        {
            public MonsterViewEvent on;
            public string key;
            public ContextEvent response;
        }

        [SerializeField] private Binding[] bindings;

        public override void Present(in MonsterViewContext context)
        {
            foreach (var binding in bindings)
                if (binding.on == context.Event && (string.IsNullOrEmpty(binding.key) || binding.key == context.Key))
                    binding.response?.Invoke(context);
        }
    }
}
