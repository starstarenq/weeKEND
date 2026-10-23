namespace VamSurvivalLike.Combat
{
    /// <summary>Receives combat damage. Implement this on players, monsters, or destructible props.</summary>
    public interface IDamageable
    {
        bool IsAlive { get; }
        void TakeDamage(float amount, object source = null);
    }
}
