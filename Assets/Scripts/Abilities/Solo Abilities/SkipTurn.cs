using System.Collections;
using System.Collections.Generic;
using UnityEngine;


class SkipTurn : BaseAbility
{
    public override string GetDescription()
    {
        return "Do nothing and end your turn.";
    }

    public override string GetName()
    {
        return "Skip Turn";
    }

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
        Debug.Log("[" + _effector.AllyCharacter.Name + "] SKIPS their turn");
    }
}

