using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WildCharge : BaseDuoAbility
{
    public override string GetName()
    {
        return "Wild Charge";
    }

    public override string GetShortDescription()
    {
        return "Charge in a direction and defend yourself the next turn.";
    }

    public override string GetDescription()
    {
        string res = "You both charge in a direction : you hold a shield, defending both of you for the following turn.";
        if (_chosenAlly != null)
        {
            res += "";
        }
        else if (_effector != null && _temporaryChosenAlly != null)
        {
            res += "\nDMG if fail: ";
        }
        return res;
    }

    public override string GetAllyDescription()
    {
        throw new System.NotImplementedException();
    }

    protected override bool IsAllyCompatible(AllyUnit unit)
    {
        return (unit.GridPosition - _effector.GridPosition).magnitude <= 1;
    }

    protected override void ChooseAlly()
    {
        throw new System.NotImplementedException();
    }

    protected override void EnemyTargetingInput()
    {
        throw new System.NotImplementedException();
    }

    public override void Execute()
    {
        throw new System.NotImplementedException();
    }

    public override bool CanExecute()
    {
        return _chosenAlly != null;
    }

    protected override void SendResultToHistoryConsole(AbilityResult result)
    {
        
    }
}
