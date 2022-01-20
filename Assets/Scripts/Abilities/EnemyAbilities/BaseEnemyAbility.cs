using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseEnemyAbility : BaseAbility
{
    protected new EnemyUnit _effector;

    public int Priority
    {
        get;
        protected set;
    }

    #region RelationshipEvents
    protected void AttackHitOrMiss(AllyUnit target, bool hit)
    {
        RelationshipEventsResult result = RelationshipEventsManager.Instance.EnemyOnAllyAttackHitOrMiss(target, hit);
        if (!hit)
        {
            AddInterruptionBeforeResult(ref result, new InterruptionParameters
            {
                interruptionType = InterruptionType.FocusTargetForGivenTimeAndFireTextFeedback,
                target = target,
                time = Interruption.FOCUS_TARGET_TIME,
                text = "Miss"
            });
            //target.Missed();
        }

        HandleRelationshipEventResult(result);

        // TODO : change target ?
    }

    protected void AttackDamage(AllyUnit target, float damage, bool crit)
    {
        RelationshipEventsResult result = RelationshipEventsManager.Instance.EnemyOnAllyAttackDamage(target, damage, crit);

        AllyUnit sacrificed = result.sacrificedTarget as AllyUnit;
        target = sacrificed == null ? target : sacrificed;

        bool killed = target.TakeDamage(ref damage, false);
        AddInterruptionAfterResult(ref result, new InterruptionParameters
        {
            interruptionType = InterruptionType.FocusTargetForGivenTimeAndFireTextFeedback,
            target = target,
            time = Interruption.FOCUS_TARGET_TIME,
            text = "-" + damage
        });

        HandleRelationshipEventResult(result);

        // TODO : if killed ?
    }
    #endregion

    public override void SetEffector(GridBasedUnit effector)
    {
        base.SetEffector(effector);
        _effector = effector as EnemyUnit;
    }

    public abstract void CalculateBestTarget();

    public override string GetShortDescription()
    {
        return "Enemy ability";
    }
}
