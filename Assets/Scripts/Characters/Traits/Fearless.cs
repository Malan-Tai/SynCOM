using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Fearless : Trait
{
    public override int GetSelfToAllySentimentGain(EnumSentiment sentiment, int baseSentiment)
    {
        if (sentiment == EnumSentiment.Trust && baseSentiment < 0)
        {
            return 0;
        }
        return baseSentiment;
    }

    public override int GetAllyToSelfSentimentGain(EnumSentiment sentiment, int baseSentiment)
    {
        return baseSentiment;
    }

    public override string GetName()
    {
        return "Fearless";
    }

}
