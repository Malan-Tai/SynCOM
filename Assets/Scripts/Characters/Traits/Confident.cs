using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Confident : Trait
{
    public override Trait GetClone(AllyCharacter allyCharacter)
    {
        Type t = GetType();
        Trait returnValue = (Trait)Activator.CreateInstance(t);
        returnValue.Owner = allyCharacter;
        returnValue.Owner.Accuracy += 10;
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
        return "Confident";
    }

    public override string GetDescription()
    {
        return "Self-esteem is a pleasant ally : when you're in good shape, it gets you in greater";
    }
}
