using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Devouring : BaseDuoAbility
{
    private List<GridBasedUnit> _possibleTargets;
    private int _targetIndex = -1;

    private AbilityStats _selfShotStats;

    public override string GetDescription()
    {
        return  "Devouring\nYou ask an ally to hold an enemy while you feed on them, " +
                "restoring your health and extending your Frenzy state.";
    }

    public override string GetName()
    {
        return "Devouring";
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
                &&  ((unit.GridPosition - this._effector.GridPosition).magnitude <= 1))
            {
                _possibleTargets.Add(unit);
            }
        }

        if (_possibleTargets.Count > 0)
        {
            _targetIndex = 0;
            CombatGameManager.Instance.Camera.SwitchParenthood(_possibleTargets[_targetIndex]);
        }

        _selfShotStats = new AbilityStats(200, 0, 1.5f, 0, _effector);

        _selfShotStats.UpdateWithEmotionModifiers(_chosenAlly);
    }

    protected override void EnemyTargetingInput()
    {
        if (_possibleTargets.Count <= 0) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitData;

        bool changedUnitThisFrame = false;

        if (Physics.Raycast(ray, out hitData, 1000))
        {
            var hitUnit = hitData.transform.GetComponent<EnemyUnit>();

            bool clicked = Input.GetMouseButtonUp(0);

            if (hitUnit != null && clicked)
            {
                int newIndex = _possibleTargets.IndexOf(hitUnit);
                if (newIndex >= 0)
                {
                    _targetIndex = newIndex;
                    changedUnitThisFrame = true;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Tab) && !changedUnitThisFrame)
        {
            _targetIndex++;
            if (_targetIndex >= _possibleTargets.Count) _targetIndex = 0;
            changedUnitThisFrame = true;
        }

        if (changedUnitThisFrame) CombatGameManager.Instance.Camera.SwitchParenthood(_possibleTargets[_targetIndex]);
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
        _effector.CurrentBuffs.Add(new Buff(3, _effector, damageBuff: 2f, critBuff: 0.5f, mitigationBuff: -0.5f));
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
                Debug.Log(this._effector.AllyCharacter.Name + " (self) : CRIT hit ! " + _selfShotStats.GetDamage() * 1.5f + "damage dealt");
            }
            else
            {
                target.Character.TakeDamage(_selfShotStats.GetDamage());
                Debug.Log(this._effector.AllyCharacter.Name + " (self) : hit ! " + _selfShotStats.GetDamage() + "damage dealt");
            }
        }
        else
        {
            Debug.Log(this._effector.AllyCharacter.Name + " (self) : missed");
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
        return "Devouring\nGet eaten, it is terrifying.";
    }
}
