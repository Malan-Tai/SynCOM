using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cold : Trait
{
    public override int GetSelfToAllySentimentGain(EnumSentiment sentiment, int baseSentiment)
    {
        if (sentiment == EnumSentiment.Sympathy && baseSentiment > 0)
        {
            return (int)(baseSentiment * 0.5);
        }
        return baseSentiment;
    }

    public override int GetAllyToSelfSentimentGain(EnumSentiment sentiment, int baseSentiment)
    {
        if (sentiment == EnumSentiment.Sympathy && baseSentiment > 0)
        {
            return (int) (baseSentiment*0.5);
        }
        return baseSentiment;
    }

    public override string GetName()
    {
        return "Cold";
    }

}
