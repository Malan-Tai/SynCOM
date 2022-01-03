using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct AbilityStats
{
    private float _innateAccuracy;
    private float _innateCrit;
    private float _innateDamage;
    private float _innateProtection;
    private AllyUnit _unit;

    private float _selfSuccessModifier;
    private float _selfMissModifier;
    private float _selfCritSuccessModifier;
    private float _selfCritMissModifier;
    private float _selfDamageModifier;
    private float _selfProtectionModifier;

    public AbilityStats(float innateAccuracy, float innateCrit, float innateDamage, float innateProtection, AllyUnit unit)
    {
        this._innateAccuracy = innateAccuracy;
        this._innateCrit = innateCrit;
        this._innateDamage = innateDamage;
        this._innateProtection = innateProtection;
        this._unit = unit;

        _selfSuccessModifier = 1;
        _selfMissModifier = 1;
        _selfCritSuccessModifier = 1;
        _selfCritMissModifier = 1;
        _selfDamageModifier = 1;
        _selfProtectionModifier = 1.6f;
    }

    /// <summary>
    /// Updates the modifiers depending on the emotions of the relationship
    /// Emotions taken into account are self to ally
    /// </summary>
    public void UpdateWithEmotionModifiers(AllyUnit ally)
    {
        Relationship relationship = this._unit.AllyCharacter.Relationships[ally.AllyCharacter];
        List<EnumEmotions> listEmotions = relationship.ListEmotions;

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
            switch (emotion)
            {
                case (EnumEmotions.Scorn):
                    Up(ref protLevelNeg, 2);
                    break;
                case (EnumEmotions.Esteem):
                    Up(ref protLevelPos, 2);
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

            _selfProtectionModifier = 1 + 0.2f * (protLevelPos - protLevelNeg);

            _selfDamageModifier = 1 + 0.25f * (damageLevelPos - damageLevelNeg);

            int accLevel = accLevelPos - accLevelNeg;
            if (accLevel < 0) _selfSuccessModifier = 1f + 0.25f * accLevel;
            else if (accLevel > 0) _selfMissModifier = 1f - 0.25f * accLevel;

            int critLevel = accLevelPos - accLevelNeg;
            if (critLevel < 0) _selfCritSuccessModifier = 1f + 0.25f * critLevel;
            else if (critLevel > 0) _selfCritMissModifier = 1f - 0.25f * critLevel;
        }
    }

    /// <summary>
    /// Returns the damage dealt by the ability
    /// </summary>
    public float GetDamage(bool considerBuffs = true, bool considerDebuffs = true)
    {
        float returnDamage = this._unit.Character.Damage * _selfDamageModifier * _innateDamage;
        float damageBuff = 0;

        foreach (Trait trait in this._unit.AllyCharacter.Traits)
        {
            damageBuff += trait.GetDamageModifier();
        }

        if (considerBuffs)
        {
            foreach (Buff buff in this._unit.Character.CurrentBuffs)
            {
                float bonus = buff.GetDamageModifier();
                if (bonus > 0) damageBuff += bonus;
            }
        }

        if (considerDebuffs)
        {
            foreach (Buff buff in this._unit.Character.CurrentBuffs)
            {
                float malus = buff.GetDamageModifier();
                if (malus < 0) damageBuff += malus;
            }
        }

        damageBuff = Mathf.Clamp(damageBuff, -1, 2);
        returnDamage *= (1 + damageBuff);
        return returnDamage;
    }

    /// <summary>
    /// Returns the chance to hit the target
    /// </summary>
    public float GetAccuracy(GridBasedUnit target, EnumCover cover, bool considerBuffs = true, bool considerDebuffs = true)
    {
        float finalAccuracy = this._unit.Character.Accuracy + _innateAccuracy - target.Character.GetDodge(cover);

        float hitMissReductionFromBuff = 1;
        float hitSucessReductionFromDebuff = 1;

        foreach (Trait trait in this._unit.AllyCharacter.Traits)
        {
            float mod = trait.GetHitRateModifier();
            if (mod > 0) hitMissReductionFromBuff *= 1 - mod;
            else hitSucessReductionFromDebuff *= 1 + mod;
        }

        if (considerBuffs)
        {
            foreach (Buff buff in this._unit.Character.CurrentBuffs)
            {
                float bonus = buff.GetHitRateModifier();
                if (bonus > 0) hitMissReductionFromBuff *= 1 - bonus;
            }
        }

        if (considerDebuffs)
        {
            foreach (Buff buff in this._unit.Character.CurrentBuffs)
            {
                float malus = buff.GetHitRateModifier();
                if (malus < 0) hitSucessReductionFromDebuff *= 1 + malus;
            }
        }

        // Apply relationship modifiers
        finalAccuracy = 100 - ((100 - (_selfSuccessModifier * finalAccuracy)) * _selfMissModifier);
        // Apply traits, buffs and debuffs
        finalAccuracy = 100 - ((100 - (hitSucessReductionFromDebuff * finalAccuracy)) * hitMissReductionFromBuff);

        return Mathf.Clamp(finalAccuracy, 5, 100);
    }

    /// <summary>
    /// Returns the chance to hit independantly from the target (ignores target's dodge and cover)
    /// </summary>
    public float GetAccuracy(bool considerBuffs = true, bool considerDebuffs = true)
    {
        float finalAccuracy = this._unit.Character.Accuracy + _innateAccuracy;

        float hitMissReductionFromBuff = 1;
        float hitSucessReductionFromDebuff = 1;

        foreach (Trait trait in this._unit.AllyCharacter.Traits)
        {
            float mod = trait.GetHitRateModifier();
            if (mod > 0) hitMissReductionFromBuff *= 1 - mod;
            else hitSucessReductionFromDebuff *= 1 + mod;
        }

        if (considerBuffs)
        {
            foreach (Buff buff in this._unit.CurrentBuffs)
            {
                float bonus = buff.GetHitRateModifier();
                if (bonus > 0) hitMissReductionFromBuff *= 1 - bonus;
            }
        }

        if (considerDebuffs)
        {
            foreach (Buff buff in this._unit.CurrentBuffs)
            {
                float malus = buff.GetHitRateModifier();
                if (malus < 0) hitSucessReductionFromDebuff *= 1 + malus;
            }
        }

        // Apply relationship modifiers
        finalAccuracy = 100 - ((100 - (_selfSuccessModifier * finalAccuracy)) * _selfMissModifier);
        // Apply traits, buffs and debuffs
        finalAccuracy = 100 - ((100 - (hitSucessReductionFromDebuff * finalAccuracy)) * hitMissReductionFromBuff);

        return Mathf.Clamp(finalAccuracy, 5, 100);
    }

    /// <summary>
    /// Returns the chance to get a critical hit, if the attack hits
    /// </summary>
    public float GetCritRate(bool considerBuffs = true, bool considerDebuffs = true)
    {
        float finalCritRate = this._unit.Character.CritChances + _innateCrit;

        float critMissReductionFromBuff = 1;
        float critSucessReductionFromDebuff = 1;

        foreach (Trait trait in this._unit.AllyCharacter.Traits)
        {
            float mod = trait.GetCritRateModifier();
            if (mod > 0) critMissReductionFromBuff *= 1 - mod;
            else critSucessReductionFromDebuff *= 1 + mod;
        }

        if (considerBuffs)
        {
            foreach (Buff buff in this._unit.Character.CurrentBuffs)
            {
                float bonus = buff.GetCritRateModifier();
                if (bonus > 0) critMissReductionFromBuff *= 1 - bonus;
            }
        }

        if (considerDebuffs)
        {
            foreach (Buff buff in this._unit.Character.CurrentBuffs)
            {
                float malus = buff.GetCritRateModifier();
                if (malus < 0) critSucessReductionFromDebuff *= 1 + malus;
            }
        }

        // Apply relationship modifiers
        finalCritRate = 100 - ((100 - (_selfCritSuccessModifier * finalCritRate)) * _selfCritMissModifier);
        // Apply traits, buffs and debuffs
        finalCritRate = 100 - ((100 - (critSucessReductionFromDebuff * finalCritRate)) * critMissReductionFromBuff);

        return Mathf.Clamp(finalCritRate, 0, 100);
    }

    /// <summary>
    /// Returns the protection, the value by which incoming damage will be multiplied
    /// </summary>
    public float GetProtection()
    {
        float finalProtection = _innateProtection * _selfProtectionModifier;
        return finalProtection;
    }

    /// <summary>
    /// Set the value of arg to the maximum between n and arg's current value
    /// </summary>
    private void Up(ref int arg, int n)
    {
        arg = Math.Max(arg, n);
    }
}
