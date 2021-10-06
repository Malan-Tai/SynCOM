using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseAbility
{
    public delegate void EndAbility(bool executed);
    public event EndAbility OnAbilityEnded;

    protected GridBasedUnit _effector;
    public virtual void SetEffector(GridBasedUnit effector)
    {
        _effector = effector;
    }

    protected abstract void EnemyTargetingInput();
    protected abstract bool CanExecute();
    protected abstract void Execute();

    protected virtual void FinalizeAbility(bool executed)
    {
        if (OnAbilityEnded != null) OnAbilityEnded(executed);
    }

    public virtual void InputControl()
    {
        EnemyTargetingInput();

        if (Input.GetKeyDown(KeyCode.Return) && CanExecute())
        {
            Execute();
            FinalizeAbility(true);
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log("cant use this ability");
            FinalizeAbility(false);
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("cancelled ability");
            CombatGameManager.Instance.Camera.SwitchParenthood(_effector);
            FinalizeAbility(false);
        }
    }
}

public abstract class BaseDuoAbility : BaseAbility
{
    private AllyUnit _temporaryChosenAlly = null;
    protected AllyUnit _chosenAlly = null;
    private List<AllyUnit> _possibleAllies = null;

    protected abstract bool IsAllyCompatible(AllyUnit unit);
    protected abstract void ChooseAlly();

    public override void SetEffector(GridBasedUnit effector)
    {
        base.SetEffector(effector);

        _possibleAllies = new List<AllyUnit>();
        foreach (AllyUnit unit in CombatGameManager.Instance.ControllableUnits)
        {
            if (unit != effector && IsAllyCompatible(unit))
            {
                _possibleAllies.Add(unit);
            }
        }

        if (_possibleAllies.Count > 0)
        {
            _temporaryChosenAlly = _possibleAllies[0];
            CombatGameManager.Instance.Camera.SwitchParenthood(_temporaryChosenAlly);
        }
    }

    protected override void FinalizeAbility(bool executed)
    {
        _temporaryChosenAlly = null;
        _chosenAlly = null;
        _possibleAllies = null;
        base.FinalizeAbility(executed);
    }

    public override void InputControl()
    {
        if (_chosenAlly == null)
        {
            AllyTargetingInput();
        }
        else
        {
            EnemyTargetingInput();
        }

        if (Input.GetKeyDown(KeyCode.Return) && CanExecute())
        {
            Execute();
            FinalizeAbility(true);
        }
        else if (Input.GetKeyDown(KeyCode.Return) && _temporaryChosenAlly != null && _chosenAlly == null)
        {
            _chosenAlly = _temporaryChosenAlly;
            _chosenAlly.UseAbilityAsAlly(this);
            ChooseAlly();
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log("cant use this ability");
            FinalizeAbility(false);
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("cancelled ability");
            CombatGameManager.Instance.Camera.SwitchParenthood(_effector);
            FinalizeAbility(false);
        }
    }

    protected void AllyTargetingInput()
    {
        if (_possibleAllies.Count < 1) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitData;

        bool changedUnitThisFrame = false;

        if (Physics.Raycast(ray, out hitData, 1000))
        {
            var hitUnit = hitData.transform.GetComponent<AllyUnit>();

            bool clicked = Input.GetMouseButtonUp(0);

            if (hitUnit != null && clicked && hitUnit != _effector && IsAllyCompatible(hitUnit))
            {
                _temporaryChosenAlly = hitUnit;
                changedUnitThisFrame = true;
            }
        }

        if (Input.GetKeyDown(KeyCode.Tab) && !changedUnitThisFrame)
        {
            int index = _possibleAllies.IndexOf(_temporaryChosenAlly);
            index++;
            if (index >= _possibleAllies.Count) index = 0;
            _temporaryChosenAlly = _possibleAllies[index];
            changedUnitThisFrame = true;
        }

        if (changedUnitThisFrame) CombatGameManager.Instance.Camera.SwitchParenthood(_temporaryChosenAlly);
    }
}
