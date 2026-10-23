using UnityEngine;

namespace VamSurvivalLike.Monsters.View
{
    /// <summary>Convenient base class for inspector-authored monster views.</summary>
    public abstract class MonsterViewBehaviour : MonoBehaviour, IMonsterView
    {
        protected MonsterActor Actor { get; private set; }

        public virtual void Initialize(MonsterActor actor) => Actor = actor;
        public abstract void Present(in MonsterViewContext context);
        public virtual void ResetView() { }
    }
}
