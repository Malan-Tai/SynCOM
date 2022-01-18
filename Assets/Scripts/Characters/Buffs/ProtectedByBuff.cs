
[System.Serializable]
public class ProtectedByBuff : Buff
{
    private GridBasedUnit _protector;

    public ProtectedByBuff(int duration, GridBasedUnit protectedUnit, GridBasedUnit protectorUnit, float protection): base(duration, protectedUnit, 0, 0, 0, protection)
    {
        _protector = protectorUnit;
    }


}
