using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Sensitive : Trait
{
    public override int GetSelfToAllySentimentGain(EnumSentiment sentiment, int baseSentiment)
    {
        return (int)(baseSentiment*1.5);
    }

    public override int GetAllyToSelfSentimentGain(EnumSentiment sentiment, int baseSentiment)
    {
        return baseSentiment;
    }

    public override string GetName()
    {
        return "Sensitive";
    }

    public override string GetDescription()
    {
        return "Everything they feel is strengthened, whether it is positive or not.";
    }
}
