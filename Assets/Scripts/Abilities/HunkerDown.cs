using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HunkerDown : BaseAbility
{
    protected override bool CanExecute()
    {
        return true;
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
