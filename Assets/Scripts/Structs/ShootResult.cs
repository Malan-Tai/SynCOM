public readonly struct ShootResult
{
    public readonly bool Cancelled;
    public readonly bool Landed;
    public readonly float Damage;
    public readonly bool Critical;

    public ShootResult(bool canceled, bool landed, float damage, bool critical)
    {
        Cancelled = canceled;
        Landed = landed;
        Damage = damage;
        Critical = critical;
    }
}
