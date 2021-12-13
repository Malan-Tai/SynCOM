using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Ugly : Trait
{
    public override int GetSelfToAllySentimentGain(EnumSentiment sentiment, int baseSentiment)
    {
       return baseSentiment;
    }

    public override int GetAllyToSelfSentimentGain(EnumSentiment sentiment, int baseSentiment)
    {
        if (sentiment == EnumSentiment.Trust && baseSentiment < 0)
        {
            return (int) (baseSentiment*1.5);
        }
            return baseSentiment;
    }

    public override string GetName()
    {
        return "Ugly";
    }

    public override string GetDescription()
    {
        return "They're so ugly everyone fears them.";
    }
}
