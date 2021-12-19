using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
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
        return 0.5f * (1 - (_owner.HealthPoints / _owner.MaxHealth));
    }

    public override string GetDescription()
    {
        return "Blood maddens them, making them stronger when hurt";
    }
}