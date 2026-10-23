namespace VamSurvivalLike.Monsters.View
{
    /// <summary>Implement this interface to plug any presentation system into a monster.</summary>
    public interface IMonsterView
    {
        void Initialize(MonsterActor actor);
        void Present(in MonsterViewContext context);
        void ResetView();
    }
}
