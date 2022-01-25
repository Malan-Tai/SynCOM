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

        //if (killed)
        //{
        //    RelationshipEventsResult killResult = RelationshipEventsManager.Instance.EnemyKillAlly(target);

        //    if (killResult.freeAttack && killResult.freeAttacker.LinesOfSight.ContainsKey(_effector) &&
        //       (killResult.freeAttacker.GridPosition - _effector.GridPosition).magnitude <= killResult.freeAttacker.Character.RangeShot)
        //    {
        //        AddInterruptionAfterResult(ref killResult, new InterruptionParameters
        //        {
        //            interruptionType = InterruptionType.FocusTargetForGivenTimeAndFireTextFeedback,
        //            target = killResult.freeAttacker,
        //            time = Interruption.FOCUS_TARGET_TIME,
        //            text = "Free Attack"
        //        });

        //        var shoot = new AbilityStats(0f, 0f, 0.75f, 0f, 0f, killResult.freeAttacker);
        //        shoot.UpdateWithEmotionModifiers(target);

        //        int randShot = UnityEngine.Random.Range(0, 100); // between 0 and 99
        //        int randCrit = UnityEngine.Random.Range(0, 100);

        //        if (randShot < shoot.GetAccuracy(target, killResult.freeAttacker.LinesOfSight[_effector].cover))
        //        {
        //            float freeDmg = shoot.GetDamage();
        //            if (randCrit < shoot.GetCritRate())
        //            {
        //                freeDmg *= 1.5f;
        //            }

        //            _effector.TakeDamage(ref freeDmg, false);
        //            AddInterruptionAfterResult(ref killResult, new InterruptionParameters
        //            {
        //                interruptionType = InterruptionType.FocusTargetForGivenTimeAndFireTextFeedback,
        //                target = _effector,
        //                time = Interruption.FOCUS_TARGET_TIME,
        //                text = "-" + freeDmg
        //            });
        //        }
        //        else
        //        {
        //            AddInterruptionAfterResult(ref killResult, new InterruptionParameters
        //            {
        //                interruptionType = InterruptionType.FocusTargetForGivenTimeAndFireTextFeedback,
        //                target = _effector,
        //                time = Interruption.FOCUS_TARGET_TIME,
        //                text = "Miss"
        //            });
        //        }
        //    }

        //    HandleRelationshipEventResult(killResult);
        //}
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
