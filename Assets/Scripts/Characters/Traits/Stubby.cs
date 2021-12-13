using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Stubby : Trait
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
        return "Stubby";
    }

    public override string GetDescription()
    {
        return "As a small being, every cover is a high cover to them.";
    }
}
