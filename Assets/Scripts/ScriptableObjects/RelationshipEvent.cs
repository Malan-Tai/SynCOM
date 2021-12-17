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

    /// effect
    public RelationshipEventEffectType effectType;
    public bool interrupts;

    // relationship gauge change
    public bool reciprocal;
    public int admirationChange;
    public int trustChange;
    public int sympathyChange;


    public bool CorrespondsToTrigger(RelationshipEvent trigger, bool allyIsDuo)
    {
        if (triggerType != trigger.triggerType) return false;
        if (onlyCheckDuoAlly && !allyIsDuo) return false;

        switch (triggerType)
        {
            case RelationshipEventTriggerType.Attack:
                return  (targetsAlly == trigger.targetsAlly)    &&
                        ((onMiss     && trigger.onMiss)         ||
                        (onHit       && trigger.onHit)          ||
                        (onCrit      && trigger.onCrit)         ||
                        (onDamage    && trigger.onDamage)       ||
                        (onFatal     && trigger.onFatal));
            default:
                // when nothing more than the status of the relationship is needed, returns true by default
                return true;
        }
    }

    public bool MeetsRelationshipRequirements(AllyCharacter source, AllyCharacter current)
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
