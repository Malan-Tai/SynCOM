using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemyShot : BaseEnemyAbility
{
    public GridBasedUnit BestTarget { get; private set; }

    public override bool CanExecute()
    {
        return BestTarget != null;
    }

    public override void Execute()
    {
        if (!CanExecute())
        {
            throw new UnityException("Can't execute enemy ability if it has no target, please check if ability can be executed before calling this method.");
        }

        int randShot = Random.Range(0, 100);
        int randCrit = Random.Range(0, 100);

        float accuratyShot = _effector.Character.Accuracy - BestTarget.Character.GetDodge(_effector.LinesOfSight[BestTarget].cover);
        if (accuratyShot > randShot)
        {
            AttackHitOrMiss(BestTarget as AllyUnit, true);
            Debug.Log($"i am shooting at {BestTarget.GridPosition} with cover {(int)_effector.LinesOfSight[BestTarget].cover}");

            AttackDamage(BestTarget as AllyUnit, _effector.Character.CritChances > randCrit ? _effector.Character.Damage * 1.5f : _effector.Character.Damage, randCrit < _effector.Character.CritChances);
        }
        else
        {
            AttackHitOrMiss(BestTarget as AllyUnit, false);
            Debug.Log($"Dice got {randShot} and had to be lower than {accuratyShot}: Missed");
        }
    }

    public override void CalculateBestTarget()
    {
        BestTarget = null;

        float minDist = float.MaxValue;
        foreach (GridBasedUnit unit in _effector.LinesOfSight.Keys)
        {
            float distance = Vector2.Distance(unit.GridPosition, _effector.GridPosition);
            if (distance <= _effector.Character.RangeShot && distance < minDist)
            {
                /// TODO calculate best target depending on priority
                /// Currently pick closest target
                minDist = distance;
                BestTarget = unit;
                Priority = 0;
            }
        }

    }

    public override string GetDescription()
    {
        return $"Shoot at the target.\nAcc:{_effector.Character.Accuracy}" +
            $" | Crit:{_effector.Character.CritChances} | Dmg:{_effector.Character.Damage}";
    }

    public override string GetName()
    {
        return "Basic Attack";
    }
}
