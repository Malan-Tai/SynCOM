using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Duo Ability <para/>
/// Heals an adjacent ally by a fixed amount
/// </summary>
public class FirstAid : BaseDuoAbility
{
    protected override void ChooseAlly()
    {
        
    }

    public override bool CanExecute()
    {
        return _chosenAlly != null; // && _chosenAlly.HP > 0
    }

    protected override void EnemyTargetingInput()
    {

    }

    public override void Execute()
    {
        Heal(_effector, _chosenAlly, 5, _chosenAlly.AllyCharacter);

        Interruption interruption = Interruption.GetInterruption(InterruptionType.FocusTargetForGivenTime);
        interruption.Init(new InterruptionParameters { target = _chosenAlly, time = FOCUS_TARGET_TIME });
        _interruptionQueue.Enqueue(interruption);

        Relationship relationshipAllyToSelf = _chosenAlly.AllyCharacter.Relationships[this._effector.AllyCharacter];
        Relationship relationshipSelfToAlly = this._effector.AllyCharacter.Relationships[_chosenAlly.AllyCharacter];
        Debug.Log(  "i am healing ally" +
                    "\nally -> self : TRU" + relationshipAllyToSelf.GetGaugeLevel(EnumSentiment.Trust) + " = " + relationshipAllyToSelf.GetGaugeValue(EnumSentiment.Trust) +
                    " | self -> ally : SYM" + relationshipSelfToAlly.GetGaugeLevel(EnumSentiment.Sympathy) + " = " + relationshipSelfToAlly.GetGaugeValue(EnumSentiment.Sympathy));
        
    }

    protected override bool IsAllyCompatible(AllyUnit unit)
    {
        return (unit.GridPosition - this._effector.GridPosition).magnitude <= 2;
    }

    public override string GetName()
    {
        return "First Aid";
    }

    public override string GetAllyDescription()
    {
        return "Get healed.";
    }

    public override string GetDescription()
    {
        return "You heal an injured ally and take them out of critical state.";
    }
}
