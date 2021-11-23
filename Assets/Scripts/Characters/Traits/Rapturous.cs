using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rapturous : Trait
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
        return "Rapturous";
    }

    public override float GetDamageModifier()
    {
        return base.GetDamageModifier();
    }
}