using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseAbility
{
    protected GridBasedUnit _effector;

    public virtual void SetEffector(GridBasedUnit effector)
    {
        _effector = effector;
    }

    public abstract bool CanExecute();
    public abstract void Execute();

    /// <summary>
    /// Returns the name of the ability.
    /// The name of the ability is not an attribute : it's determined by the return value of this function.
    /// </summary>
    public abstract string GetName();

    /// <summary>
    /// Returns a short description of the ability.
    /// The description of the ability is not an attribute : it's determined by the return value of this function.
    /// </summary>
    public abstract string GetDescription();
}
