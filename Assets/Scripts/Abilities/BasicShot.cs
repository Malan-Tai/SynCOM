using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BasicShot : BaseAbility
{
    private GridBasedUnit[] _possibleTargets;
    private int _targetIndex = -1;

    public override void SetEffector(AllyUnit effector)
    {
        _possibleTargets = new GridBasedUnit[effector.LinesOfSight.Count];
        effector.LinesOfSight.Keys.CopyTo(_possibleTargets, 0);

        if (_possibleTargets.Length > 0)
        {
            _targetIndex = 0;
            CombatGameManager.Instance.Camera.SwitchParenthood(_possibleTargets[_targetIndex]);
        }

        base.SetEffector(effector);
    }

    protected override bool CanExecute()
    {
        return _targetIndex >= 0;
    }

    protected override void EnemyTargetingInput()
    {
        if (_possibleTargets.Length <= 0) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitData;

        bool changedUnitThisFrame = false;

        if (Physics.Raycast(ray, out hitData, 1000))
        {
            var hitUnit = hitData.transform.GetComponent<EnemyUnit>();

            bool clicked = Input.GetMouseButtonUp(0);

            if (hitUnit != null && clicked)
            {
                int newIndex = Array.IndexOf(_possibleTargets, hitUnit);
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
            if (_targetIndex >= _possibleTargets.Length) _targetIndex = 0;
            changedUnitThisFrame = true;
        }

        if (changedUnitThisFrame) CombatGameManager.Instance.Camera.SwitchParenthood(_possibleTargets[_targetIndex]);
    }

    protected override void Execute()
    {
        Debug.Log("i am shooting at " + _possibleTargets[_targetIndex].GridPosition + " with cover " + (int)_effector.LinesOfSight[_possibleTargets[_targetIndex]].cover);
    }

    protected override void FinalizeAbility(bool executed)
    {
        _targetIndex = -1;
        _possibleTargets = null;
        base.FinalizeAbility(executed);
    }
}
