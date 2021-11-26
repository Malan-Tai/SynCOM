using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicDuoShot : BaseDuoAbility
{
    private List<GridBasedUnit> _possibleTargets;
    private int _targetIndex = -1;

    private AbilityStats _selfShotStats;
    private AbilityStats _allyShotStats;

    protected override void ChooseAlly()
    {
        _possibleTargets = new List<GridBasedUnit>();
        var tempTargets = new GridBasedUnit[_effector.LinesOfSight.Count];
        _effector.LinesOfSight.Keys.CopyTo(tempTargets, 0);

        foreach (GridBasedUnit unit in tempTargets)
        {
            float distanceToSelf = Vector2.Distance(unit.GridPosition, _effector.GridPosition);
            float distanceToAlly = Vector2.Distance(unit.GridPosition, _chosenAlly.GridPosition);
            if (distanceToSelf <= _effector.Character.RangeShot && distanceToAlly <= _effector.Character.RangeShot && _chosenAlly.LinesOfSight.ContainsKey(unit))
            {
                _possibleTargets.Add(unit);
            }
        }

        if (_possibleTargets.Count > 0)
        {
            _targetIndex = 0;
            CombatGameManager.Instance.Camera.SwitchParenthood(_possibleTargets[_targetIndex]);
        }

        _selfShotStats = new AbilityStats(0, 0, 1.5f, 0, _effector);
        _allyShotStats = new AbilityStats(0, 0, 1.5f, 0, _chosenAlly);

        _selfShotStats.UpdateWithEmotionModifiers(_chosenAlly);
        _allyShotStats.UpdateWithEmotionModifiers(_effector);
    }

    protected override bool CanExecute()
    {
        return _chosenAlly != null && _targetIndex >= 0;
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
        GridBasedUnit target = _possibleTargets[_targetIndex];

        Debug.Log("we are shooting at " + target.GridPosition + " with cover " + (int)_effector.LinesOfSight[target].cover);
        SelfShoot(target, _selfShotStats);
        AllyShoot(target, _allyShotStats);
    }

    protected override bool IsAllyCompatible(AllyUnit unit)
    {
        return true;
    }

    public override string GetName()
    {
        return "Relentless Assault";
    }

    public override string GetDescription()
    {
        string res = "Duo Shot\nTake a shot at the target with augmented damage.";
        if (_targetIndex >= 0 && _chosenAlly != null)
        {
            GridBasedUnit target = _possibleTargets[_targetIndex];

            res += "\nAcc:" + _selfShotStats.GetAccuracy(target, _effector.LinesOfSight[target].cover) +
                    " | Crit:" + _selfShotStats.GetCritRate() +
                    " | Dmg:" + _selfShotStats.GetDamage();
        }

        return res;
    }

    public override string GetAllyDescription()
    {
        string res = "Duo Shot\nTake a shot at the target with augmented damage.";
        if (_targetIndex >= 0 && _chosenAlly != null)
        {
            GridBasedUnit target = _possibleTargets[_targetIndex];

            res += "\nAcc:" + _allyShotStats.GetAccuracy(target, _chosenAlly.LinesOfSight[target].cover) +
                    " | Crit:" + _allyShotStats.GetCritRate() +
                    " | Dmg:" + _allyShotStats.GetDamage();
        }

        return res;
    }
}
