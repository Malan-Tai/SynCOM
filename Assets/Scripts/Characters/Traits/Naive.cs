using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Naive : Trait
{
    public override int GetSelfToAllySentimentGain(EnumSentiment sentiment, int baseSentiment)
    {
        if (sentiment == EnumSentiment.Trust && baseSentiment>0)
        {
            return (int) (baseSentiment * 1.5);
        }
        return baseSentiment;
    }

    public override int GetAllyToSelfSentimentGain(EnumSentiment sentiment, int baseSentiment)
    {
        if (sentiment == EnumSentiment.Trust && baseSentiment > 0)
        {
            return (int)(baseSentiment * 0.5);
        }
        return baseSentiment;
    }

    public override string GetName()
    {
        return "Naive";
    }

    public override string GetDescription()
    {
        return "Trusts very easily, but others won't be as keen on them.";
    }
}
