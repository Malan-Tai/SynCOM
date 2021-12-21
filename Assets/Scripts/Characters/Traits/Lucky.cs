using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Lucky : Trait
{
    public override Trait GetClone(AllyCharacter allyCharacter)
    {
        Type t = GetType();
        Trait returnValue = (Trait)Activator.CreateInstance(t);
        returnValue.Owner = allyCharacter;
        returnValue.Owner.CritChances = 30;
        return returnValue;
    }
    public override int GetSelfToAllySentimentGain(EnumSentiment sentiment, int baseSentiment)
    {
       return baseSentiment;
    }

    public override int GetAllyToSelfSentimentGain(EnumSentiment sentiment, int baseSentiment)
    {
       return baseSentiment;
    }

    public override string GetName()
    {
        return "Lucky";
    }

    public override string GetDescription()
    {
        return "Always keep a four-leaf clover, therefore are more likely to eliminates enemies.";
    }
}
