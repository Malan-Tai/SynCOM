using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public abstract class Trait
{
    protected AllyCharacter _owner;
    public AllyCharacter Owner
    {
        get { return _owner; }
        set { _owner = value; }
    }

    public Trait GetClone(AllyCharacter allyCharacter)
    {
        Type t = GetType();
        Trait returnValue = (Trait)Activator.CreateInstance(t);
        returnValue.Owner = allyCharacter;
        return returnValue;
    }
    public abstract int GetSelfToAllySentimentGain(EnumSentiment sentiment, int baseSentiment);

    public abstract int GetAllyToSelfSentimentGain(EnumSentiment sentiment, int baseSentiment);

    public abstract string GetName();
    public abstract string GetDescription();

    /// <summary>
    /// Returns the multiplicative damage modifier. For instance, a +50% buff
    /// is represented by a return value of +0.5f, and a -25% debuff by -0.25f.
    /// Buffs and debuffs stacks additively.
    /// </summary>
    public virtual float GetDamageModifier()
    {
        return 0;
    }

    /// <summary>
    /// Returns the multiplicative crit rate modifier. For instance, a +50% buff
    /// is represented by a return value of +0.5f, and a -25% debuff by -0.25f.
    /// Buffs and debuffs stacks additively.
    /// </summary>
    public virtual float GetCritRateModifier()
    {
        return 0;
    }

    /// <summary>
    /// Returns the multiplicative hit rate modifier. For instance, a +50% buff
    /// is represented by a return value of +0.5f, and a -25% debuff by -0.25f.
    /// Buffs and debuffs stacks additively.
    /// </summary>
    public virtual float GetHitRateModifier()
    {
        return 0;
    }

    /// <summary>
    /// Returns the additive move modifier. Can be positive or negative.
    /// Buffs and debuffs stacks additively.
    /// </summary>
    public virtual float GetMoveModifier()
    {
        return 0;
    }

    /// <summary>
    /// Returns the multiplicative mitigation (reduction of incoming damage) modifier.
    /// For instance, a +50% buff is represented by a return value of +0.5f, and a -25% debuff by -0.25f.
    /// Buffs and debuffs stacks additively.
    /// </summary>
    public virtual float GetMitigationModifier()
    {
        return 0;
    }

    /// <summary>
    /// Returns the multiplicative dodge modifier.
    /// For instance, a +50% buff is represented by a return value of +0.5f, and a -25% debuff by -0.25f.
    /// Buffs and debuffs stacks additively.
    /// </summary>
    public virtual float GetDodgeModifier()
    {
        return 0;
    }
}
