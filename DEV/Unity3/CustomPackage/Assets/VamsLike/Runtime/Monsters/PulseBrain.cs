namespace VamSurvivalLike.Monsters
{
    /// <summary>Approaches, stops, then emits a circular pulse centered on the monster.</summary>
    public sealed class PulseBrain : TelegraphedAttackBrain
    {
        protected override string AttackKey => "Pulse";
        protected override VamSurvivalLike.Combat.AttackShape Shape => VamSurvivalLike.Combat.AttackShape.Circle;
    }
}
