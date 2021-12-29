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

    public override bool CanExecute()
    {
        return _chosenAlly != null;
    }

    protected override void EnemyTargetingInput()
    {

    }

    public override void Execute()
    {
        // Impact on the sentiments

        // Self -> Ally relationship


        // Ally -> Self relationship
        //AllyToSelfModifySentiment(_chosenAlly, EnumSentiment.Trust, -3);
        //AllyToSelfModifySentiment(_chosenAlly, EnumSentiment.Sympathy, -3);
        //AllyToSelfModifySentiment(_chosenAlly, EnumSentiment.Admiration, -3);

        FriendlyFireDamage(_effector, _chosenAlly, 1, _chosenAlly.AllyCharacter);

        var parameters = new InterruptionParameters { interruptionType = InterruptionType.FocusTargetForGivenTime, target = _chosenAlly, time = FOCUS_TARGET_TIME };
        _interruptionQueue.Enqueue(Interruption.GetInitializedInterruption(parameters));

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
        return "A secrete technique that only the chosen ones can use. " +
                "You express your rage against your useless ally in the form of a single perfectly placed slap";
    }

    public override string GetAllyDescription()
    {
        return "Get slapped, dislike your ally.";
    }
}
