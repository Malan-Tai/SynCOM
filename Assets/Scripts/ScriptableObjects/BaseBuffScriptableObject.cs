using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseBuffScriptableObject : ScriptableObject
{
    public int duration;
    public abstract Buff GetBuff(GridBasedUnit unit);
}
