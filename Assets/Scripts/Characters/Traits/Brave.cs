using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Brave : Trait
{
    public override int GetSelfToAllySentimentGain(EnumSentiment sentiment, int baseSentiment)
    {
       return baseSentiment;
    }

    public override int GetAllyToSelfSentimentGain(EnumSentiment sentiment, int baseSentiment)
    {
        if (sentiment == EnumSentiment.Admiration && baseSentiment > 0)
        {
            return (int) (baseSentiment*1.5);
        }
            return baseSentiment;
    }

    public override string GetName()
    {
        return "Brave";
    }

    public override string GetDescription()
    {
        return "Their acts of courage will raise admiration within your ranks.";
    }
}
