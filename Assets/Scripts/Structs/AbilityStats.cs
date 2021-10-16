using System;
using System.Collections;
using System.Collections.Generic;

public struct AbilityStats
{
    public float accuracy;      // [5 ; 95]
    public float crit;          // [5 ; 95]
    public float damage;        // 
    public float protection;    // [1 ; 2]

    private float _selfSuccessModifier;
    private float _selfMissModifier;
    private float _selfCritSuccessModifier;
    private float _selfCritMissModifier;
    private float _selfDamageModifier;

    public AbilityStats(float accuracy, float crit, float damage, float protection)
    {
        this.accuracy = accuracy;
        this.crit = crit;
        this.damage = damage;
        this.protection = protection;

        _selfSuccessModifier = 1;
        _selfMissModifier = 1;
        _selfCritSuccessModifier = 1;
        _selfCritMissModifier = 1;
        _selfDamageModifier = 1;
    }

    public void UpdateWithSelfToAllyRelationship(Relationship relationship)
    {
        List<EnumEmotions> listEmotions = relationship.ListEmotions;
        //AbilityStats bestModifiers = new AbilityStats(0, 0, 0, 0); // modifiers to chance of missing [0.5, 1], chance of not critting [0.5, 1], damage, and protection [1.2, 2]
        //AbilityStats worstModifiers = new AbilityStats(0, 0, 0, 0); // modifiers to chance of hitting [0.5, 1], chance of critting [0.5, 1], damage, and protection [1.2, 2]

        int protLevelPos = 0;
        int protLevelNeg = 0;
        int damageLevelNeg = 0;
        int damageLevelPos = 0; // not used for now : no Emotion gives a positive damage buff
        int critLevelPos = 0;
        int critLevelNeg = 0;
        int accLevelPos = 0;
        int accLevelNeg = 0;

        foreach (EnumEmotions emotion in listEmotions)
        {
            /*
            switch (emotion)
            {
                case (EnumEmotions.Scorn):
                    float protMod = -0.4f;
                    break;
                case (EnumEmotions.Esteem):
                    protMod = 0.4f;
                    break;
                case (EnumEmotions.Prejudice):
                    protMod = -0.4f;
                    break;
                case (EnumEmotions.Submission):
                    protMod = 0.2f;
                    float successMod = -0.25f;
                    float critMod = -0.25f;
                    break;
                case (EnumEmotions.Terror):
                    successMod = -0.25f;
                    break;
                case (EnumEmotions.ConflictedFeelings):
                    break;
                case (EnumEmotions.Faith):
                    float missMod = -0.5f;
                    protMod = 0.4f;
                    break;
                case (EnumEmotions.Respect):
                    missMod = -0.5f;
                    break;
                case (EnumEmotions.Condescension):
                    protMod = -0.6f;
                    float dmgMod = -0.5f;
                    break;
                case (EnumEmotions.Recognition):
                    missMod = -0.25f;
                    protMod = 0.2f;
                    dmgMod = -0.5f;
                    break;
                case (EnumEmotions.Hate):
                    dmgMod = -0.25f;
                    successMod = -0.75f;
                    break;
                case (EnumEmotions.ReluctantTrust):
                    missMod = -0.5f;
                    dmgMod = -0.5f;
                    break;
                case (EnumEmotions.Hostility):
                    dmgMod = -0.5f;
                    break;
                case (EnumEmotions.Pity):
                    protMod = 0.2f;
                    break;
                case (EnumEmotions.Devotion):
                    protMod = 0.4f;
                    break;
                case (EnumEmotions.Apprehension):
                    successMod = 0.25f;
                    float failCritMod = -0.25f;
                    break;
                case (EnumEmotions.Friendship):
                    missMod = -0.25f;
                    break;
                case (EnumEmotions.Empathy):
                    // action gratuite, pas ici
                    break;
                default:
                    break;
            }*/
            switch (emotion)
            {
                case (EnumEmotions.Scorn):
                    Up(ref protLevelPos, 2);
                    break;
                case (EnumEmotions.Esteem):
                    Up(ref protLevelNeg, 2);
                    break;
                case (EnumEmotions.Prejudice):
                    Up(ref protLevelNeg, 2);
                    break;
                case (EnumEmotions.Submission):
                    Up(ref protLevelPos, 1);
                    Up(ref accLevelNeg, 1);
                    Up(ref critLevelNeg, 1);
                    break;
                case (EnumEmotions.Terror):
                    Up(ref accLevelNeg, 1);
                    break;
                case (EnumEmotions.ConflictedFeelings):
                    // no effect
                    break;
                case (EnumEmotions.Faith):
                    Up(ref accLevelPos, 2);
                    Up(ref protLevelPos, 2);
                    break;
                case (EnumEmotions.Respect):
                    Up(ref accLevelPos, 2);
                    break;
                case (EnumEmotions.Condescension):
                    Up(ref protLevelNeg, 3);
                    Up(ref damageLevelNeg, 2);
                    break;
                case (EnumEmotions.Recognition):
                    Up(ref accLevelPos, 1);
                    Up(ref protLevelPos, 1);
                    Up(ref damageLevelNeg, 2);
                    break;
                case (EnumEmotions.Hate):
                    Up(ref damageLevelNeg, 1);
                    Up(ref accLevelNeg, 1);
                    break;
                case (EnumEmotions.ReluctantTrust):
                    Up(ref accLevelPos, 2);
                    Up(ref damageLevelNeg, 2);
                    break;
                case (EnumEmotions.Hostility):
                    Up(ref damageLevelNeg, 2);
                    break;
                case (EnumEmotions.Pity):
                    Up(ref protLevelPos, 1);
                    break;
                case (EnumEmotions.Devotion):
                    Up(ref protLevelPos, 2);
                    break;
                case (EnumEmotions.Apprehension):
                    Up(ref accLevelNeg, 1);
                    Up(ref critLevelPos, 1);
                    break;
                case (EnumEmotions.Friendship):
                    Up(ref accLevelPos, 1);
                    break;
                case (EnumEmotions.Empathy):
                    // chance of free action, not calculated here
                    break;
                default:
                    break;
            }

            protection = 1.6f + 0.2f * (protLevelPos - protLevelNeg);

            _selfDamageModifier = 1 + 0.25f * (damageLevelPos - damageLevelNeg);

            int accLevel = accLevelPos - accLevelNeg;
            if (accLevel < 0) _selfSuccessModifier = 1f + 0.25f * accLevel;
            else if (accLevel > 0) _selfMissModifier = 1f - 0.25f * accLevel;

            int critLevel = accLevelPos - accLevelNeg;
            if (critLevel < 0) _selfCritSuccessModifier = 1f + 0.25f * critLevel;
            else if (critLevel > 0) _selfCritMissModifier = 1f - 0.25f * critLevel;
        }
    }

    public void CalculateDamage(AllyUnit unit)
    {
        //damage = unit.Character.Damage * _selfDamageModifier;
    }

    private void Up(ref int arg, int n)
    {
        arg = Math.Max(arg, n);
    }
}
