using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Tall : Trait
{
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
        return "Tall";
    }

    public override string GetDescription()
    {
        return "They are very tall and thus can't benefit from high covers.";
    }
}
