using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BasicShot : BaseAllyAbility
{
    private List<GridBasedUnit> _possibleTargets;
    private int _targetIndex = -1;

    private AbilityStats _selfShotStats;

    public override string GetDescription()
    {
        string res = "Shoot at the target.";
        if (_hoveredUnit != null)
        {
            res += "\nAcc:" + _selfShotStats.GetAccuracy(_hoveredUnit, _effector.LinesOfSight[_hoveredUnit].cover) + // (_effector.Character.Accuracy - _hoveredUnit.Character.GetDodge(_effector.LinesOfSight[_hoveredUnit].cover)) +
                    "% | Crit:" + _selfShotStats.GetCritRate() + // + _effector.Character.CritChances +
                    "% | Dmg:" + _selfShotStats.GetDamage();// _effector.Character.Damage;
        }
        else if (_targetIndex >= 0)
        {
            GridBasedUnit target = _possibleTargets[_targetIndex];
            if (target == null) Debug.Log("BLIP BLOUP");

            res += "\nAcc:" + _selfShotStats.GetAccuracy(target, _effector.LinesOfSight[target].cover) + //(_effector.Character.Accuracy - target.Character.GetDodge(_effector.LinesOfSight[target].cover)) +
                    "% | Crit:" + _selfShotStats.GetCritRate() + // + _effector.Character.CritChances +
                    "% | Dmg:" + _selfShotStats.GetDamage();// _effector.Character.Damage;
        }

        return res;
    }

    public override void SetEffector(GridBasedUnit effector)
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

        _selfShotStats = new AbilityStats(0, 0, 1f, 0, 0, _effector);
    }

    public override bool CanExecute()
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

    public override void Execute()
    {
        GridBasedUnit target = _possibleTargets[_targetIndex];

        int randShot = UnityEngine.Random.Range(0, 100); // between 0 and 99
        int randCrit = UnityEngine.Random.Range(0, 100);

        if (randShot < _selfShotStats.GetAccuracy(target, _effector.LinesOfSight[target].cover))
        {
            AttackHitOrMiss(_effector, target as EnemyUnit, true);

            if (randCrit < _selfShotStats.GetCritRate())
            {
                AttackDamage(_effector, target as EnemyUnit, _effector.Character.Damage * 1.5f, true);
                HistoryConsole.AddEntry(EntryBuilder.GetDamageEntry(_effector, target, this, _effector.Character.Damage * 1.5f, true));
            }
            else
            {
                AttackDamage(_effector, target as EnemyUnit, _effector.Character.Damage, false);
                HistoryConsole.AddEntry(EntryBuilder.GetDamageEntry(_effector, target, this, _effector.Character.Damage, false));
            }

        }
        else
        {
            AttackHitOrMiss(_effector, target as EnemyUnit, false);
            HistoryConsole.AddEntry(EntryBuilder.GetMissedEntry(_effector, target, this));
        }
    }

    protected override void EndAbility()
    {
        _targetIndex = -1;
        _possibleTargets = null;
        base.EndAbility();
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

    public override string GetShortDescription()
    {
        return "A basic attack";
    }
}
