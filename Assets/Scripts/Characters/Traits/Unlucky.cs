using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Unlucky : Trait
{
    public override Trait GetClone(AllyCharacter allyCharacter)
    {
        Type t = GetType();
        Trait returnValue = (Trait)Activator.CreateInstance(t);
        returnValue.Owner = allyCharacter;
        returnValue.Owner.CritChances = 8;  
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
        return "Unlucky";
    }

    public override string GetDescription()
    {
        return "No one knows why, but if something could go wring for them, it would.";
    }
}
