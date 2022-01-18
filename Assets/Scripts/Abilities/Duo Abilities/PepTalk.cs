using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PepTalk : BaseDuoAbility
{
    private List<GridBasedUnit> _possibleTargets;
    private int _targetIndex = -1;

    private AbilityStats _selfShotStats;

    public override string GetDescription()
    {
        return "Your words of encouragement strengthen you both, increasing damage, move and aim for the following 2 turn." +
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
        return "You ally's words of encouragement strengthen you, increasing damage, move and aim for the following 2 turn.";
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
       
    }

    protected override void EnemyTargetingInput()
    {
        
    }

    public override void Execute()
    {
        // Impact on the sentiments
        AllyToSelfModifySentiment(_chosenAlly, EnumSentiment.Sympathy, 5);
        SelfToAllyModifySentiment(_chosenAlly, EnumSentiment.Sympathy, 5);

        // Ally -> Self relationship
        _effector.Character.CurrentBuffs.Add(new Buff(duration: 4, _effector, moveBuff: 3, damageBuff: 0.2f, accuracyBuff: 0.5f));
        _chosenAlly.Character.CurrentBuffs.Add(new Buff(duration: 4, _chosenAlly, moveBuff: 3, damageBuff: 0.2f, accuracyBuff: 0.5f));
    }

    protected override bool IsAllyCompatible(AllyUnit unit)
    {
        return (unit.GridPosition - this._effector.GridPosition).magnitude <= 5;
    }

    public override void UISelectUnit(GridBasedUnit unit)
    {
        if (_chosenAlly != null)
        {
            _targetIndex = _possibleTargets.IndexOf(unit);
            CombatGameManager.Instance.Camera.SwitchParenthood(unit);
            RequestDescriptionUpdate();
            RequestTargetSymbolUpdate(unit);
        }
        else base.UISelectUnit(unit);
    }

}
