using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseEnemyAbility : BaseAbility
{
    protected new EnemyUnit _effector;

    public int Priority
    {
        get;
        protected set;
    }

    public override void SetEffector(GridBasedUnit effector)
    {
        _effector = effector as EnemyUnit;
    }

    public abstract void CalculateBestTarget();
}
