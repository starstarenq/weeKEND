namespace VamSurvivalLike.Monsters
{
    /// <summary>Stops near the target and emits a donut-shaped burst with a safe inner zone.</summary>
    public sealed class RingBurstBrain : TelegraphedAttackBrain
    {
        protected override string AttackKey => "RingBurst";
        protected override VamSurvivalLike.Combat.AttackShape Shape => VamSurvivalLike.Combat.AttackShape.Donut;
    }
}
