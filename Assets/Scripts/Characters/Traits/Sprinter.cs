using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Sprinter : Trait
{
    public override Trait GetClone(AllyCharacter allyCharacter)
    {
        Type t = GetType();
        Trait returnValue = (Trait)Activator.CreateInstance(t);
        returnValue.Owner = allyCharacter;
        returnValue.Owner.MovementPoints = 30;
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
        return "Sprinter";
    }

    public override string GetDescription()
    {
        return "They would have won the Olympics, but couldn't afford to leave New York.";
    }
}
