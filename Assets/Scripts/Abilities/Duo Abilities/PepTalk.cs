using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PepTalk : BaseDuoAbility
{
    public override string GetDescription()
    {
        return "Your words of encouragement strengthen you both, increasing damage, move and aim for the next 2 turn." +
               "\nDMG +20%" +
               "\nACC +50%" +
               "\nMOVE +3";
    }

    public override string GetShortDescription()
    {
        return "A small stat buff to you and your ally.";
    }

    public override string GetAllyDescription()
    {
        return "You ally's words of encouragement strengthen you, increasing damage, move and aim for the next 2 turn.";
    }
    public override string GetName()
    {
        return "Pep Talk";
    }

    public override bool CanExecute()
    {
        return _chosenAlly != null;
    }

    protected override void ChooseAlly()
    {
        _ignoreEnemyTargeting = true;
    }

    protected override void EnemyTargetingInput()
    {
        
    }

    public override void Execute()
    {
        AddBuff(_effector, new Buff("Pumped", duration: 6, _effector, moveBuff: 3, damageBuff: 0.2f, accuracyBuff: 0.5f));
        AddBuff(_chosenAlly, new Buff("Pumped", duration: 6, _effector, moveBuff: 3, damageBuff: 0.2f, accuracyBuff: 0.5f));
    }

    protected override bool IsAllyCompatible(AllyUnit unit)
    {
        return (unit.GridPosition - this._effector.GridPosition).magnitude <= 5;
    }
}
