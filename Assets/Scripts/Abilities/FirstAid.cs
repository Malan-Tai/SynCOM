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

    protected override bool CanExecute()
    {
        return _chosenAlly != null; // && _chosenAlly.HP > 0
    }

    protected override void EnemyTargetingInput()
    {

    }

    protected override void Execute()
    {   
        // Impact on the sentiments

        // Self -> Ally relationship
        Relationship relationshipSelfToAlly = this._effector.Character.Relationships[_chosenAlly.Character];
        relationshipSelfToAlly.IncreaseSentiment(EnumSentiment.Sympathy, 5);
        
        // Ally -> Self relationship
        Relationship relationshipAllyToSelf = _chosenAlly.Character.Relationships[this._effector.Character];
        relationshipAllyToSelf.IncreaseSentiment(EnumSentiment.Trust, 5);

        // Actual effect of the ability
        // TODO : implementing heal
        Debug.Log(  "i am healing ally" +
                    "\nally -> self : TRU" + relationshipAllyToSelf.GetGaugeLevel(EnumSentiment.Trust) + " = " + relationshipAllyToSelf.GetGaugeValue(EnumSentiment.Trust) +
                    " | self -> ally : SYM" + relationshipSelfToAlly.GetGaugeLevel(EnumSentiment.Sympathy) + " = " + relationshipSelfToAlly.GetGaugeValue(EnumSentiment.Sympathy));
        
    }

    protected override bool IsAllyCompatible(AllyUnit unit)
    {
        return (unit.GridPosition - this._effector.GridPosition).magnitude <= 1;
    }
}
