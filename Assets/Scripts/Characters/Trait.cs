using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Trait
{
    public abstract int GetSelfToAllySentimentGain(EnumSentiment sentiment, int baseSentiment);

    public abstract int GetAllyToSelfSentimentGain(EnumSentiment sentiment, int baseSentiment);
}
