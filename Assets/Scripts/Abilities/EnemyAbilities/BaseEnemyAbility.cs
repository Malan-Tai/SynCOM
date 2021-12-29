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
        HandleRelationshipEventResult(RelationshipEventsManager.Instance.EnemyOnAllyAttackHitOrMiss(target.AllyCharacter, hit));

        // TODO : change target ?
        if (!hit) target.Missed();
    }

    protected void AttackDamage(AllyUnit target, float damage, bool crit)
    {
        HandleRelationshipEventResult(RelationshipEventsManager.Instance.EnemyOnAllyAttackDamage(target.AllyCharacter, damage, crit)) ;

        // TODO : change target if protection
        bool killed = target.TakeDamage(damage);

        // TODO : if killed ?
    }
    #endregion

    public override void SetEffector(GridBasedUnit effector)
    {
        base.SetEffector(effector);
        _effector = effector as EnemyUnit;
    }

    public abstract void CalculateBestTarget();
}
