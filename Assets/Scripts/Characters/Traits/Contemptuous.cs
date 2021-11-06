using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Contemptuous : Trait
{
    public override int GetSelfToAllySentimentGain(EnumSentiment sentiment, int baseSentiment)
    {
        if (sentiment == EnumSentiment.Admiration && baseSentiment < 0)
        {
            return (int)(baseSentiment * 1.5);
        }
        return baseSentiment;
    }

    public override int GetAllyToSelfSentimentGain(EnumSentiment sentiment, int baseSentiment)
    {
        return baseSentiment;
    }
    
}
