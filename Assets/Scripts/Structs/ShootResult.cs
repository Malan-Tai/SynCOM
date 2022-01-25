public readonly struct ShootResult
{
    public readonly bool Landed;
    public readonly float Damage;
    public readonly bool Critical;

    public ShootResult(bool landed, float damage, bool critical)
    {
        Landed = landed;
        Damage = damage;
        Critical = critical;
    }
}
