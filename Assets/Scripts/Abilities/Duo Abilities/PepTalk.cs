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
        return "Your words of encouragement strengthen you both, increasing damage, move and aim for the following turn.";
    }

    public override string GetName()
    {
        return "Pep Talk";
    }

    protected override bool CanExecute()
    {
        return _chosenAlly != null && _targetIndex >= 0;
    }

    protected override void ChooseAlly()
    {
        _possibleTargets = new List<GridBasedUnit>();
        var tempTargets = new GridBasedUnit[_effector.LinesOfSight.Count];
        _effector.LinesOfSight.Keys.CopyTo(tempTargets, 0);

        foreach (GridBasedUnit unit in tempTargets)
        {
            if (    (_chosenAlly.LinesOfSight.ContainsKey(unit)) 
                &&  ((unit.GridPosition - this._effector.GridPosition).magnitude <= 3))
            {
                _possibleTargets.Add(unit);
            }
        }

        RequestTargetsUpdate(_possibleTargets);

        if (_possibleTargets.Count > 0)
        {
            _targetIndex = 0;
            CombatGameManager.Instance.Camera.SwitchParenthood(_possibleTargets[_targetIndex]);
            RequestTargetSymbolUpdate(_possibleTargets[_targetIndex]);
        }
        else RequestTargetSymbolUpdate(null);

        _selfShotStats = new AbilityStats(200, 0, 1.5f, 0, _effector);

        _selfShotStats.UpdateWithEmotionModifiers(_chosenAlly);
    }

    protected override void EnemyTargetingInput()
    {
        
    }

    protected override void Execute()
    {
        // Impact on the sentiments
        // Ally -> Self relationship
        AllyToSelfModifySentiment(_chosenAlly, EnumSentiment.Trust, -10);

        // Actual effect of the ability
        GridBasedUnit target = _possibleTargets[_targetIndex];

        Debug.Log("DEVOURING : we are shooting at " + target.GridPosition + " with cover " + (int)_effector.LinesOfSight[target].cover);
        SelfShoot(target);
        _effector.Character.Heal(6);
    }

    private void SelfShoot(GridBasedUnit target)
    {
        int randShot = UnityEngine.Random.Range(0, 100); // between 0 and 99
        int randCrit = UnityEngine.Random.Range(0, 100);

        Debug.Log("self to hit: " + randShot + " for " + _selfShotStats.GetAccuracy(target, _effector.LinesOfSight[target].cover));

        if (randShot < _selfShotStats.GetAccuracy(target, _effector.LinesOfSight[target].cover))
        {
            AllyToSelfModifySentiment(_chosenAlly, EnumSentiment.Admiration, 5);

            if (randCrit < _selfShotStats.GetCritRate())
            {
                target.Character.TakeDamage(_selfShotStats.GetDamage() * 1.5f);
                SelfToAllyModifySentiment(_chosenAlly, EnumSentiment.Sympathy, 5);
                AllyToSelfModifySentiment(_chosenAlly, EnumSentiment.Sympathy, 5);
            }
            else
            {
                target.Character.TakeDamage(_selfShotStats.GetDamage());
            }
        }
        else
        {
            Debug.Log("self missed");
            SelfToAllyModifySentiment(_chosenAlly, EnumSentiment.Admiration, -5);
            AllyToSelfModifySentiment(_chosenAlly, EnumSentiment.Admiration, -5);
        }
    }

    protected override bool IsAllyCompatible(AllyUnit unit)
    {
        return (unit.GridPosition - this._effector.GridPosition).magnitude <= 1;
    }

    public override string GetAllyDescription()
    {
        return "The feasting spectacle is terrifying.";
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
