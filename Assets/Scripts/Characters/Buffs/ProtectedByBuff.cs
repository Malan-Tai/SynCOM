
[System.Serializable]
public class ProtectedByBuff : Buff
{
    private AllyUnit _protector;

    public ProtectedByBuff(int duration, GridBasedUnit protectedUnit, AllyUnit protectorUnit, float protection): base("Protected", duration, protectedUnit, 0, 0, 0, protection)
    {
        _protector = protectorUnit;
    }

    public AllyUnit GetProtector() { return _protector; }
}
