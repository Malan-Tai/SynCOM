using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Trait
{
    protected AllyCharacter _owner;
    public AllyCharacter Owner
    {
        get { return _owner; }
        set { _owner = value; }
    }
    public abstract int GetSelfToAllySentimentGain(EnumSentiment sentiment, int baseSentiment);

    public abstract int GetAllyToSelfSentimentGain(EnumSentiment sentiment, int baseSentiment);

    public abstract string GetName();

    /// <summary>
    /// Returns the multiplicative Damage modifier. For instance, a +50% buff
    /// is represented by a return value of +0.5f, and a -25% debuff by -0.25f.
    /// Buffs and debuffs stacks additively.
    /// </summary>
    public virtual float GetDamageModifier()
    {
        return 0;
    }

    public Trait GetClone(AllyCharacter allyCharacter)
    {
        Type t = GetType();
        Trait returnValue = (Trait)Activator.CreateInstance(t);
        returnValue.Owner = allyCharacter;
        return returnValue;
    }
}
