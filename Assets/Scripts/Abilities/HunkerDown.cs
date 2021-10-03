using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HunkerDown : BaseAbility
{
    protected override bool CanExecute()
    {
        return CombatGameManager.Instance.gridMap.GetBestCoverAt(_effector.GridPosition) != EnumCover.None;
    }

    protected override void EnemyTargetingInput()
    {
        // no target needed
    }

    protected override void Execute()
    {
        Debug.Log("i hunker down");
    }
}
