using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Duo Ability <para/>
/// Heals an adjacent ally by a fixed amount
/// </summary>
public class FirstAid : BaseDuoAbility
{
    private AbilityStats _healStats;

    protected override void ChooseAlly()
    {
        _healStats = new AbilityStats(0, 0, 0, 0, 5, _effector);
        _healStats.UpdateWithEmotionModifiers(_chosenAlly);
    }

    public override bool CanExecute()
    {
        return _chosenAlly != null; // && _chosenAlly.HP > 0
    }

    protected override void EnemyTargetingInput()
    {

    }

    public override void Execute()
    {
        Heal(_effector, _chosenAlly, _healStats.GetHeal(), _chosenAlly);
    }

    protected override bool IsAllyCompatible(AllyUnit unit)
    {
        return (unit.GridPosition - this._effector.GridPosition).magnitude <= 2;
    }

    public override string GetName()
    {
        return "First Aid";
    }

    public override string GetAllyDescription()
    {
        return "Get healed.";
    }

    public override string GetDescription()
    {
        return "You heal an injured ally and take them out of critical state.";
    }

    public override string GetShortDescription()
    {
        return "A small heal";
    }
}
