using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseAbility
{
    protected GridBasedUnit _effector;
    protected InterruptionQueue _interruptionQueue;

    public virtual void SetEffector(GridBasedUnit effector)
    {
        _effector = effector;
        _interruptionQueue = effector.InterruptionQueue;
    }

    protected virtual void HandleRelationshipEventResult(RelationshipEventsResult result)
    {
        foreach (InterruptionParameters param in result.interruptions)
        {
            _interruptionQueue.Enqueue(Interruption.GetInitializedInterruption(param));
        }

        foreach (Buff buff in result.buffs)
        {
            buff.Owner.CurrentBuffs.Add(buff);
        }
    }

    protected void AddInterruptionBeforeResult(ref RelationshipEventsResult result, InterruptionParameters parameters)
    {
        result.interruptions.Insert(0, parameters);
    }

    protected void AddInterruptionAfterResult(ref RelationshipEventsResult result, InterruptionParameters parameters)
    {
        result.interruptions.Add(parameters);
    }

    public abstract bool CanExecute();
    public abstract void Execute();

    /// <summary>
    /// Returns the name of the ability.
    /// The name of the ability is not an attribute : it's determined by the return value of this function.
    /// </summary>
    public abstract string GetName();

    /// <summary>
    /// Returns a short description of the ability.
    /// The description of the ability is not an attribute : it's determined by the return value of this function.
    /// </summary>
    public abstract string GetDescription();

    /// <summary>
    /// The description for UI list
    /// </summary>
    public abstract string GetShortDescription();


    protected class AbilityResult
    {
        public bool Miss = false;
        public float Damage = 0;
        public float Heal = 0;
        public bool Critical = false;

        public bool AllyMiss = false;
        public float AllyDamage = 0;
        public float AllyHeal = 0;
        public bool AllyCritical = false;
    }

    /// <summary>
    /// Logs the ability result in the history console when the ability is successful
    /// </summary>
    protected abstract void SendResultToHistoryConsole(AbilityResult result);
}
