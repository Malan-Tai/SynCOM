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
        AllyToSelfModifySentiment(_chosenAlly, EnumSentiment.Trust, -3);
        AllyToSelfModifySentiment(_chosenAlly, EnumSentiment.Sympathy, -3);
        AllyToSelfModifySentiment(_chosenAlly, EnumSentiment.Admiration, -3);

        // Actual effect of the ability
        Relationship relationshipAllyToSelf = _chosenAlly.AllyCharacter.Relationships[this._effector.AllyCharacter];
        Debug.Log("take that you idiot" +
            "\nally -> self : ADM" + relationshipAllyToSelf.GetGaugeLevel(EnumSentiment.Admiration) + " = " + relationshipAllyToSelf.GetGaugeValue(EnumSentiment.Admiration) +
            " | ally -> self : TRU" + relationshipAllyToSelf.GetGaugeLevel(EnumSentiment.Trust) + " = " + relationshipAllyToSelf.GetGaugeValue(EnumSentiment.Trust) +
            " | ally -> self : SYM" + relationshipAllyToSelf.GetGaugeLevel(EnumSentiment.Sympathy) + " = " + relationshipAllyToSelf.GetGaugeValue(EnumSentiment.Sympathy));

    }

    protected override bool IsAllyCompatible(AllyUnit unit)
    {
        return true;
    }

    public override string GetName()
    {
        return "Slap";
    }

    public override string GetDescription()
    {
        return  "A secrete technique that only the chosen ones can use. " +
                "You express your rage against your useless ally in the form of a single perfectly placed slap";
    }
}
