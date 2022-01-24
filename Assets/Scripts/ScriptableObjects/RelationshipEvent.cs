using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RelationshipEvent", menuName = "Relationship Event")]
public class RelationshipEvent : ScriptableObject
{
    /// trigger
    public RelationshipEventTriggerType triggerType;
    [Tooltip("True if the event should only trigger once per opportunity : e.g. don't trigger multiple protection (it is therefore useless if you only check for duo ally)")]
    public bool triggersOnlyOnce;

    // involved
    public bool onlyCheckDuoAlly;
    public bool dontCheckDuoAlly;
    public bool onlyCheckTarget;
    public bool dontCheckTarget;
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
    //public bool isBeingProtected;

    // heal
    [Tooltip("Health Ratio of the surveyed unit, not the healed one")]
    [MinMaxSlider(0, 1)]
    public Vector2 minMaxHealthRatio = new Vector2(0, 1);

    // kill
    [Tooltip("Kill Steal true means the trigger will only check the best damager of the killed entity")]
    public bool killSteal;

    // start action
    public ActionTypes startedAction;

    /// effect
    public RelationshipEventEffectType effectType;
    public bool interrupts;

    // relationship gauge change
    [Tooltip("Reciprocal means below changes will be made on both sides of the relationship")]
    public bool reciprocal;
    public int admirationChange;
    public int trustChange;
    public int sympathyChange;
    [Tooltip("Source to Current is true if the effect causes a different gauge change from source to current than from current to source")]
    public bool sourceToCurrent;
    public int admirationChangeSTC;
    public int trustChangeSTC;
    public int sympathyChangeSTC;

    // generic chance field
    [Range(0, 1)]
    public float chance;

    // interruption
    public InterruptionScriptableObject[] interruptionsOnCurrent;
    public InterruptionScriptableObject[] interruptionsOnSource;

    // free action
    public bool freeAction;
    public bool freeActionForDuo;

    // sacrifice
    public float maxRange;

    // buff
    public BaseBuffScriptableObject[] buffsOnSource;
    public BaseBuffScriptableObject[] buffsOnTarget;

    // change action
    public ChangeActionTypes changeActionTo;

    public bool CorrespondsToTrigger(RelationshipEvent trigger, bool allyIsDuo, bool allyIsTarget, float healthRatio, bool isBestDamager) //, bool isProtected)
    {
        if (triggerType != trigger.triggerType) return false;
        if (onlyCheckDuoAlly && !allyIsDuo) return false;
        if (dontCheckDuoAlly && allyIsDuo) return false;
        if (onlyCheckTarget && !allyIsTarget) return false;
        if (dontCheckTarget && allyIsTarget) return false;

        switch (triggerType)
        {
            case RelationshipEventTriggerType.Attack:
                return  (targetsAlly == trigger.targetsAlly)    &&
                        ((onMiss     && trigger.onMiss)         ||
                        (onHit       && trigger.onHit)          ||
                        (onCrit      && trigger.onCrit)         ||
                        (onDamage    && trigger.onDamage)       ||
                        (onFatal     && trigger.onFatal));//       &&
                        //(isBeingProtected == isProtected);

            case RelationshipEventTriggerType.Heal:
                bool inRange = minMaxHealthRatio.x <= healthRatio && healthRatio <= minMaxHealthRatio.y;
                return inRange;

            case RelationshipEventTriggerType.Kill:
                return (!killSteal || isBestDamager) && trigger.targetsAlly == targetsAlly;

            case RelationshipEventTriggerType.FriendlyFire:
                return onFatal == trigger.onFatal;

            case RelationshipEventTriggerType.StartAction:
                return startedAction == trigger.startedAction;

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
        if (target == current || target == null) return MeetsRelationshipRequirements(source, current);
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
