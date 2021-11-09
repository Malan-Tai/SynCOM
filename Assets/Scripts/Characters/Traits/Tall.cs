using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
}
