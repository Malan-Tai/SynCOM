using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HunkerDown : BaseAbility
{
    protected override bool CanExecute()
    {
        return CombatGameManager.Instance.GridMap.GetBestCoverAt(_effector.GridPosition) != EnumCover.None;
    }

    protected override void EnemyTargetingInput()
    {
        // no target needed
    }

    protected override void Execute()
    {
        Debug.Log("i hunker down");
    }

    public override string GetName()
    {
        return "Hunker Down";
    }

    public override string GetDescription()
    {
        return "Hunker down behind a cover to gain a protection against incoming damage";
    }
}
