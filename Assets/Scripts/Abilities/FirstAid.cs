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
        SelfToAllyModifySentiment(_chosenAlly, EnumSentiment.Sympathy, 5);
        
        // Ally -> Self relationship
        AllyToSelfModifySentiment(_chosenAlly, EnumSentiment.Trust, 5);

        // Actual effect of the ability
        // TODO : implementing heal
        Relationship relationshipAllyToSelf = _chosenAlly.AllyCharacter.Relationships[this._effector.AllyCharacter];
        Relationship relationshipSelfToAlly = this._effector.AllyCharacter.Relationships[_chosenAlly.AllyCharacter];
        Debug.Log(  "i am healing ally" +
                    "\nally -> self : TRU" + relationshipAllyToSelf.GetGaugeLevel(EnumSentiment.Trust) + " = " + relationshipAllyToSelf.GetGaugeValue(EnumSentiment.Trust) +
                    " | self -> ally : SYM" + relationshipSelfToAlly.GetGaugeLevel(EnumSentiment.Sympathy) + " = " + relationshipSelfToAlly.GetGaugeValue(EnumSentiment.Sympathy));
        
    }

    protected override bool IsAllyCompatible(AllyUnit unit)
    {
        return (unit.GridPosition - this._effector.GridPosition).magnitude <= 1;
    }

    public override string GetAllyDescription()
    {
        string res = "First Aid\nHeal your ally.";

        return res;
    }

    public override string GetDescription()
    {
        string res = "First Aid\nGet healed.";

        return res;
    }
}
