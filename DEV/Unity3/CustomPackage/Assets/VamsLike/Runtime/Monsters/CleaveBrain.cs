namespace VamSurvivalLike.Monsters
{
    /// <summary>Locks its facing during windup and performs a stationary frontal sector attack.</summary>
    public sealed class CleaveBrain : TelegraphedAttackBrain
    {
        protected override string AttackKey => "Cleave";
        protected override VamSurvivalLike.Combat.AttackShape Shape => VamSurvivalLike.Combat.AttackShape.Sector;
    }
}
