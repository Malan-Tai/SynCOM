using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseAbility
{
    public delegate void EndAbility();
    public event EndAbility OnAbilityEnded;

    protected abstract void EnemyTargetingInput();
    protected abstract bool CanExecute();
    protected abstract void Execute();

    protected void FinalizeAbility()
    {
        if (OnAbilityEnded != null) OnAbilityEnded();
    }

    public void InputControl()
    {
        EnemyTargetingInput();

        if (Input.GetKeyDown(KeyCode.Return) && CanExecute())
        {
            Execute();
            FinalizeAbility();
        }
    }
}

public abstract class BaseDuoAbility : BaseAbility
{
    protected bool _choseAlly = false;

    protected abstract void AllyTargetingInput();

    protected new void FinalizeAbility()
    {
        _choseAlly = false;
        base.FinalizeAbility();
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
            FinalizeAbility();
        }
    }
}
