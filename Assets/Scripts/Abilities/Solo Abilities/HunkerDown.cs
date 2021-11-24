using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HunkerDown : BaseAbility
{
    public override string GetDescription()
    {
        return "Hunker Down\nIncreases the efficiency of your cover, if you have one.";
    }

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
}
