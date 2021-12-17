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
    public bool onMiss;
    public bool onHit;
    public bool onCrit;

    /// effect
    public RelationshipEventEffectType effectType;

    // relationship gauge change
    public bool reciprocal;
    public int admirationChange;
    public int trustChange;
    public int sympathyChange;
}
