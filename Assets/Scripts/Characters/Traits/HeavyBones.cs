using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class HeavyBones : Trait
{
    public override Trait GetClone(AllyCharacter allyCharacter)
    {
        Type t = GetType();
        Trait returnValue = (Trait)Activator.CreateInstance(t);
        returnValue.Owner = allyCharacter;
        returnValue.Owner.Weigth = 120;
        returnValue.Owner.MaxHealth = 30;
        returnValue.Owner.HealthPoints = 30;
        returnValue.Owner.Dodge = 5;
        return returnValue;
    }
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
        return "Heavy Bones";
    }

    public override string GetDescription()
    {
        return "I'm not fat, i have an heavy and solid skeletton !";
    }
}
