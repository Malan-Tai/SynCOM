using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseAbility
{
    public delegate void EndAbility(bool executed);
    public event EndAbility OnAbilityEnded;

    protected abstract void EnemyTargetingInput();
    protected abstract bool CanExecute();
    protected abstract void Execute();

    protected void FinalizeAbility(bool executed)
    {
        if (OnAbilityEnded != null) OnAbilityEnded(executed);
    }

    public void InputControl()
    {
        EnemyTargetingInput();

        if (Input.GetKeyDown(KeyCode.Return) && CanExecute())
        {
            Execute();
            FinalizeAbility(true);
        }
    }
}

public abstract class BaseDuoAbility : BaseAbility
{
    protected bool _choseAlly = false;

    protected abstract void AllyTargetingInput();

    protected new void FinalizeAbility(bool executed)
    {
        _choseAlly = false;
        base.FinalizeAbility(executed);
    }

    public new void InputControl()
    {
        if (!_choseAlly)
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
    }
}
