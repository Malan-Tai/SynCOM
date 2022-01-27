using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Handsome : Trait
{
    public override int GetSelfToAllySentimentGain(EnumSentiment sentiment, int baseSentiment)
    {
       return baseSentiment;
    }

    public override int GetAllyToSelfSentimentGain(EnumSentiment sentiment, int baseSentiment)
    {
        if (sentiment == EnumSentiment.Trust && baseSentiment > 0)
        {
            return (int) (baseSentiment*1.5);
        }
            return baseSentiment;
    }

    public override string GetName()
    {
        return "Handsome";
    }

    public override string GetDescription()
    {
        return "Their beauty makes them strangely easy to trust.";
    }
}
