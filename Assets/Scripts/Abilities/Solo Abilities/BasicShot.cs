using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BasicShot : BaseAbility
{
    private GridBasedUnit[] _possibleTargets;
    private int _targetIndex = -1;

    public override string GetDescription()
    {
        string res = "Shot\nShoot at the target.";
        if (_targetIndex >= 0)
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
        float distance = Vector2.Distance(_possibleTargets[_targetIndex].GridPosition, _effector.GridPosition);
        Debug.Log("Distance between shot and shooted =" + distance);
        return _targetIndex >= 0 && distance < _effector.Character.RangeShot;
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
        GridBasedUnit target = _possibleTargets[_targetIndex];
        int randShot = UnityEngine.Random.Range(0, 100);
        int randCrit = UnityEngine.Random.Range(0, 100);

        if (_effector.Character.Accuracy - target.Character.GetDodge(_effector.LinesOfSight[target].cover) > randShot) {
            Debug.Log("i am shooting at " + _possibleTargets[_targetIndex].GridPosition + " with cover " + (int)_effector.LinesOfSight[target].cover);
            if (_effector.Character.CritChances > randCrit) {
                target.TakeDamage(_effector.Character.Damage * 1.5f);
            }
            else
            {
                target.TakeDamage(_effector.Character.Damage);
            }
            Debug.Log("Ennemy has" + _possibleTargets[_targetIndex].Character.HealthPoints + "HP left");
        }
        else
        {
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
}
