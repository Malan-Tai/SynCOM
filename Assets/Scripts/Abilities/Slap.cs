using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Duo Ability <param/>
/// Decreases all sentiments felt from the ally towards me. For debugging only
/// </summary>
public class Slap : BaseDuoAbility
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
        

        // Ally -> Self relationship
        Relationship relationshipAllyToSelf = _chosenAlly.Character.Relationships[this._effector.Character];
        relationshipAllyToSelf.IncreaseSentiment(EnumSentiment.Trust, -3);
        relationshipAllyToSelf.IncreaseSentiment(EnumSentiment.Sympathy, -3);
        relationshipAllyToSelf.IncreaseSentiment(EnumSentiment.Admiration, -3);

        // Actual effect of the ability
        // TODO : implementing heal
        Debug.Log("take that you idiot" +
            "\nally -> self : ADM" + relationshipAllyToSelf.GetGaugeLevel(EnumSentiment.Admiration) + " = " + relationshipAllyToSelf.GetGaugeValue(EnumSentiment.Admiration) +
            " | ally -> self : TRU" + relationshipAllyToSelf.GetGaugeLevel(EnumSentiment.Trust) + " = " + relationshipAllyToSelf.GetGaugeValue(EnumSentiment.Trust) +
            " | ally -> self : SYM" + relationshipAllyToSelf.GetGaugeLevel(EnumSentiment.Sympathy) + " = " + relationshipAllyToSelf.GetGaugeValue(EnumSentiment.Sympathy));

    }

    protected override bool IsAllyCompatible(AllyUnit unit)
    {
        return true;
    }
}
