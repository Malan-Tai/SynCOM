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

}
