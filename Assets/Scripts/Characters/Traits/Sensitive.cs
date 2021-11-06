using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    
}
