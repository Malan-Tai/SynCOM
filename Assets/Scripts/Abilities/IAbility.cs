using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAbility
{
    /// <summary>
    /// used for single user abilities and as a parent for the duo ability interface
    /// </summary>

    public void UpdateTargeting();

    public void Execute();
}

public interface IDuoAbility : IAbility
{
    public void UpdateAllyTargeting();
}
