using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RelationshipEvent", menuName = "Relationship Event")]
public class RelationshipEvent : ScriptableObject
{
    /// trigger
    public RelationshipEventTriggerType triggerType;

    // involved
    public bool onlyCheckDuoAlly;
    public bool dontCheckDuoAlly;
    public bool requiresEmotions;
    public EnumEmotions[] requiredEmotionsTowardsSource;
    public EnumEmotions[] requiredEmotionsTowardsTarget;

    // attack
    public bool targetsAlly;
    public bool onMiss;
    public bool onHit;
    public bool onCrit;
    public bool onDamage;
    [Tooltip("Fatal is to detect a fatal hit before it is dealt : e.g. to protect an ally, not to detect a kill")]
    public bool onFatal;

    // heal
    [Tooltip("Health Ratio of the surveyed unit, not the healed one")]
    [MinMaxSlider(0, 1)]
    public Vector2 minMaxHealthRatio = new Vector2(0, 1);

    /// effect
    public RelationshipEventEffectType effectType;
    public bool interrupts;

    // relationship gauge change
    [Tooltip("Reciprocal means below changes will be made on both sides of the relationship")]
    public bool reciprocal;
    public int admirationChange;
    public int trustChange;
    public int sympathyChange;
    [Tooltip("Source to Target is true if the effect causes a different gauge change from source to target than from target to source")]
    public bool sourceToTarget;
    public int admirationChangeSTT;
    public int trustChangeSTT;
    public int sympathyChangeSTT;


    public bool CorrespondsToTrigger(RelationshipEvent trigger, bool allyIsDuo, float healthRatio)
    {
        if (triggerType != trigger.triggerType) return false;
        if (onlyCheckDuoAlly && !allyIsDuo) return false;
        if (dontCheckDuoAlly && allyIsDuo) return false;

        switch (triggerType)
        {
            case RelationshipEventTriggerType.Attack:
                return  (targetsAlly == trigger.targetsAlly)    &&
                        ((onMiss     && trigger.onMiss)         ||
                        (onHit       && trigger.onHit)          ||
                        (onCrit      && trigger.onCrit)         ||
                        (onDamage    && trigger.onDamage)       ||
                        (onFatal     && trigger.onFatal));

            case RelationshipEventTriggerType.Heal:
                return minMaxHealthRatio.x <= healthRatio && healthRatio <= minMaxHealthRatio.y;

            default:
                // when nothing more than the status of the relationship is needed, returns true by default
                return true;
        }
    }

    private bool MeetsRelationshipRequirements(AllyCharacter source, AllyCharacter current)
    {
        if (!requiresEmotions || requiredEmotionsTowardsSource.Length == 0) return true;

        Relationship toSource = current.Relationships[source];

        foreach (EnumEmotions emotion in requiredEmotionsTowardsSource)
        {
            if (toSource.IsFeeling(emotion))
            {
                return true;
            }
        }

        return false;
    }

    public bool MeetsRelationshipRequirements(AllyCharacter source, AllyCharacter target, AllyCharacter current)
    {
        if (target == null) return MeetsRelationshipRequirements(source, current);
        if (!requiresEmotions) return true;

        Relationship toSource = current.Relationships[source];
        bool isSourceOk = requiredEmotionsTowardsSource.Length == 0; // true if no emtions needed, else false until checked
        Relationship toTarget = current.Relationships[target];
        bool isTargetOk = requiredEmotionsTowardsTarget.Length == 0;

        foreach (EnumEmotions emotion in requiredEmotionsTowardsSource)
        {
            if (toSource.IsFeeling(emotion))
            {
                isSourceOk = true;
                break;
            }
        }

        if (!isSourceOk) return false;

        foreach (EnumEmotions emotion in requiredEmotionsTowardsTarget)
        {
            if (toTarget.IsFeeling(emotion))
            {
                isTargetOk = true;
                break;
            }
        }

        return isTargetOk;
    }
}

/// Copy pasted from https://github.com/GucioDevs/SimpleMinMaxSlider because the import wasn't working
/// No modifications were made
/// I was forced to put it in this file because having it in a separate file wouldn't allow the [] attribute being found

public class MinMaxSliderAttribute : PropertyAttribute
{

    public float min;
    public float max;

    public MinMaxSliderAttribute(float min, float max)
    {
        this.min = min;
        this.max = max;
    }
}
