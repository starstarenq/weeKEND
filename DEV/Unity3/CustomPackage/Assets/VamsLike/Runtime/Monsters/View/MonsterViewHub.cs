using System;
using System.Collections.Generic;
using UnityEngine;

namespace VamSurvivalLike.Monsters.View
{
    /// <summary>
    /// Fan-out point between gameplay and presentation. Views may live on the prefab or
    /// be registered at runtime by an external UI/effect service.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MonsterViewHub : MonoBehaviour
    {
        [SerializeField] private bool includeInactiveChildren = true;

        private readonly List<IMonsterView> views = new();
        private MonsterActor actor;

        public event Action<MonsterViewContext> Presented;

        public void Initialize(MonsterActor owner)
        {
            actor = owner;
            views.Clear();

            var behaviours = GetComponentsInChildren<MonoBehaviour>(includeInactiveChildren);
            foreach (var behaviour in behaviours)
            {
                if (behaviour is IMonsterView view && !ReferenceEquals(view, this))
                    Register(view);
            }
        }

        public void Register(IMonsterView view)
        {
            if (view == null || views.Contains(view))
                return;

            views.Add(view);
            view.Initialize(actor);
        }

        public void Unregister(IMonsterView view) => views.Remove(view);

        public void Publish(in MonsterViewContext context)
        {
            // A view is allowed to disable/destroy another view while handling a message.
            for (var index = views.Count - 1; index >= 0; index--)
            {
                var view = views[index];
                if (view is UnityEngine.Object unityObject && unityObject == null)
                {
                    views.RemoveAt(index);
                    continue;
                }

                view.Present(context);
            }

            Presented?.Invoke(context);
        }

        public void ResetViews()
        {
            foreach (var view in views)
                view.ResetView();
        }
    }
}
