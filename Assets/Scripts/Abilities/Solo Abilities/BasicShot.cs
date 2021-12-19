using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BasicShot : BaseAbility
{
    private List<GridBasedUnit> _possibleTargets;
    private int _targetIndex = -1;

    private AbilityStats _selfShotStats;

    public override string GetDescription()
    {
        string res = "Shoot at the target.";
        if (_hoveredUnit != null)
        {
            res += "\nAcc:" + (_effector.Character.Accuracy - _hoveredUnit.Character.GetDodge(_effector.LinesOfSight[_hoveredUnit].cover)) +
                    " | Crit:" + _effector.Character.CritChances +
                    " | Dmg:" + _effector.Character.Damage;
        }
        else if (_targetIndex >= 0)
        {
            GridBasedUnit target = _possibleTargets[_targetIndex];

            res += "\nAcc:" + (_effector.Character.Accuracy - target.Character.GetDodge(_effector.LinesOfSight[target].cover)) +
                    " | Crit:" + _effector.Character.CritChances +
                    " | Dmg:" + _effector.Character.Damage;
        }

        return res;
    }

    public override void SetEffector(AllyUnit effector)
    {
        _possibleTargets = new List<GridBasedUnit>();
        
        foreach (GridBasedUnit unit in effector.LinesOfSight.Keys)
        {
            float distance = Vector2.Distance(unit.GridPosition, effector.GridPosition);
            if (distance <= effector.Character.RangeShot)
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

        base.SetEffector(effector);
    }

    protected override bool CanExecute()
    {
        return _targetIndex >= 0;
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

        if (changedUnitThisFrame)
        {
            CombatGameManager.Instance.Camera.SwitchParenthood(_possibleTargets[_targetIndex]);
            RequestDescriptionUpdate();
            RequestTargetSymbolUpdate(_possibleTargets[_targetIndex]);
        }
    }

    protected override void Execute()
    {
        GridBasedUnit target = _possibleTargets[_targetIndex];

        //int randShot = UnityEngine.Random.Range(0, 100);
        //int randCrit = UnityEngine.Random.Range(0, 100);

        //if (_effector.Character.Accuracy - target.Character.GetDodge(_effector.LinesOfSight[target].cover) > randShot) {
        //    Debug.Log("i am shooting at " + _possibleTargets[_targetIndex].GridPosition + " with cover " + (int)_effector.LinesOfSight[target].cover);
        //    if (_effector.Character.CritChances > randCrit) {
        //        target.TakeDamage(_effector.Character.Damage * 1.5f);
        //        Debug.Log(this._effector.AllyCharacter.Name + " (self) : CRIT hit ! " + _effector.Character.Damage * 1.5f + "damage dealt");
        //    }
        //    else
        //    {
        //        target.TakeDamage(_effector.Character.Damage);
        //        Debug.Log(this._effector.AllyCharacter.Name + " (self) : hit ! " + _effector.Character.Damage + "damage dealt");
        //    }
        //    Debug.Log("Ennemy has" + _possibleTargets[_targetIndex].Character.HealthPoints + "HP left");
        //}
        //else
        //{
        //    Debug.Log("Dice got " + randShot + " and had to be lower than " + (_effector.Character.Accuracy - target.Character.GetDodge(_effector.LinesOfSight[target].cover)) + ": Missed");
        //}

        //////////////////////////////////////////////////////////////////////////////////////////
        
        // Set accuracy to 999 to facilitate debugging
        _selfShotStats = new AbilityStats(0, 0, 1f, 0, _effector);

        int randShot = UnityEngine.Random.Range(0, 100); // between 0 and 99
        int randCrit = UnityEngine.Random.Range(0, 100);

        Debug.Log("self to hit: " + randShot + " for " + _selfShotStats.GetAccuracy(target, _effector.LinesOfSight[target].cover));

        if (randShot < _selfShotStats.GetAccuracy(target, _effector.LinesOfSight[target].cover))
        {

            if (randCrit < _selfShotStats.GetCritRate())
            {
                target.Character.TakeDamage(_selfShotStats.GetDamage() * 1.5f);
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
            target.Missed();
            Debug.Log("Dice got " + randShot + " and had to be lower than " + (_effector.Character.Accuracy - target.Character.GetDodge(_effector.LinesOfSight[target].cover)) + ": Missed");
        }
    }

    protected override void FinalizeAbility(bool executed)
    {
        _targetIndex = -1;
        _possibleTargets = null;
        base.FinalizeAbility(executed);
    }

    public override string GetName()
    {
        return "Basic Attack";
    }

    public override void UISelectUnit(GridBasedUnit unit)
    {
        _targetIndex = _possibleTargets.IndexOf(unit);
        CombatGameManager.Instance.Camera.SwitchParenthood(unit);
        RequestDescriptionUpdate();
        RequestTargetSymbolUpdate(unit);
    }
}
