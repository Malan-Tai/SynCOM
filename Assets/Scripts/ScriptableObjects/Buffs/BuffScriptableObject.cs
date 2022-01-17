using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "BasicBuff", menuName = "Buff/Basic Buff")]
public class BuffScriptableObject : BaseBuffScriptableObject
{
    public float damageBuff;
    public float critBuff;
    public float accuracyBuff;
    public float mitigationBuff;
    public float moveBuff;
    public float dodgeBuff;

    public override Buff GetBuff(GridBasedUnit unit)
    {
        return new Buff(duration, unit, damageBuff, critBuff, accuracyBuff, mitigationBuff, moveBuff, dodgeBuff);
    }
}
