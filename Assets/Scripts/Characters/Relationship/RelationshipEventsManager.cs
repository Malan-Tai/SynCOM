using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelationshipEventsManager : MonoBehaviour
{
    #region Singleton
    private static RelationshipEventsManager _instance;
    public static RelationshipEventsManager Instance { get { return _instance; } }
    private bool _toNullify = true;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            _toNullify = false;
            Destroy(this);
        }
    }

    private void OnDestroy()
    {
        if (_toNullify) _instance = null;
    }
    #endregion

    [SerializeField]
    private RelationshipEvent[] _allEvents;

    private Dictionary<EnemyUnit, Dictionary<AllyCharacter, float>> _damageOnEnemies;

    private void Start()
    {
        _damageOnEnemies = new Dictionary<EnemyUnit, Dictionary<AllyCharacter, float>>();
    }

    private RelationshipEventsResult CheckTriggersAndExecute(RelationshipEvent dummyTrigger, AllyUnit sourceUnit, AllyUnit allyTargetUnit = null, AllyUnit duoUnit = null, EnemyUnit enemyTargetUnit = null)
    {
        RelationshipEventsResult result = new RelationshipEventsResult
        {
            refusedDuo = false,
            interruptions = new List<InterruptionParameters>(),
            buffs = new List<Buff>()
        };

        AllyCharacter   sourceCharacter         = sourceUnit        == null ? null : sourceUnit.AllyCharacter;
        AllyCharacter   allyTargetCharacter     = allyTargetUnit    == null ? null : allyTargetUnit.AllyCharacter;
        AllyCharacter   duoCharacter            = duoUnit           == null ? null : duoUnit.AllyCharacter;

        foreach (RelationshipEvent relationshipEvent in _allEvents)
        {
            foreach (AllyUnit allyUnit in CombatGameManager.Instance.AllAllyUnits)
            {
                AllyCharacter allyCharacter = allyUnit.AllyCharacter;
                if (allyCharacter == sourceCharacter) continue;
                bool isDuo = allyCharacter == duoCharacter;
                bool isTarget = allyTargetCharacter != null && allyCharacter == allyTargetCharacter;
                bool isBestDamager = allyCharacter == GetBestDamagerOf(enemyTargetUnit);

                if (relationshipEvent.CorrespondsToTrigger(dummyTrigger, isDuo, isTarget, allyCharacter.HealthPoints / allyCharacter.MaxHealth, isBestDamager) &&
                    relationshipEvent.MeetsRelationshipRequirements(sourceCharacter, allyTargetCharacter, allyCharacter))
                {
                    if (Execute(relationshipEvent, sourceUnit, allyUnit, ref result) && relationshipEvent.triggersOnlyOnce) break;
                }
            }
        }

        return result;
    }

    private bool Execute(RelationshipEvent relationshipEvent, AllyUnit source, AllyUnit currentUnit, ref RelationshipEventsResult result)
    {
        bool actuallyExecuted = true;

        switch (relationshipEvent.effectType)
        {
            case RelationshipEventEffectType.RelationshipGaugeChange:
                Relationship currentToSource = currentUnit.AllyCharacter.Relationships[source.AllyCharacter];
                currentToSource.IncreaseSentiment(EnumSentiment.Admiration, relationshipEvent.admirationChange);
                currentToSource.IncreaseSentiment(EnumSentiment.Trust, relationshipEvent.trustChange);
                currentToSource.IncreaseSentiment(EnumSentiment.Sympathy, relationshipEvent.sympathyChange);

                if (relationshipEvent.reciprocal)
                {
                    Relationship sourceToCurrent = source.AllyCharacter.Relationships[currentUnit.AllyCharacter];
                    sourceToCurrent.IncreaseSentiment(EnumSentiment.Admiration, relationshipEvent.admirationChange);
                    sourceToCurrent.IncreaseSentiment(EnumSentiment.Trust, relationshipEvent.trustChange);
                    sourceToCurrent.IncreaseSentiment(EnumSentiment.Sympathy, relationshipEvent.sympathyChange);
                }
                else if (relationshipEvent.sourceToCurrent)
                {
                    Relationship sourceToCurrent = source.AllyCharacter.Relationships[currentUnit.AllyCharacter];
                    sourceToCurrent.IncreaseSentiment(EnumSentiment.Admiration, relationshipEvent.admirationChangeSTC);
                    sourceToCurrent.IncreaseSentiment(EnumSentiment.Trust, relationshipEvent.trustChangeSTC);
                    sourceToCurrent.IncreaseSentiment(EnumSentiment.Sympathy, relationshipEvent.sympathyChangeSTC);
                }

                break;

            case RelationshipEventEffectType.RefuseToDuo:
                result.refusedDuo = RandomEngine.Instance.Range(0f, 1f) < relationshipEvent.chance;
                actuallyExecuted = result.refusedDuo;
                break;

            case RelationshipEventEffectType.StealDuo:
                actuallyExecuted = RandomEngine.Instance.Range(0f, 1f) < relationshipEvent.chance;
                if (actuallyExecuted) result.stolenDuoUnit = currentUnit;
                break;

            case RelationshipEventEffectType.FreeAction:
                actuallyExecuted = RandomEngine.Instance.Range(0f, 1f) < relationshipEvent.chance;
                result.freeActionForSource  = result.freeActionForSource    || (relationshipEvent.freeAction        && actuallyExecuted);
                result.freeActionForDuo     = result.freeActionForDuo       || (relationshipEvent.freeActionForDuo  && actuallyExecuted);
                break;

            case RelationshipEventEffectType.Sacrifice:
                bool rangeOk = Vector2.Distance(source.GridPosition, currentUnit.GridPosition) <= relationshipEvent.maxRange;
                bool rolledOk = RandomEngine.Instance.Range(0f, 1f) < relationshipEvent.chance;
                actuallyExecuted = rangeOk && rolledOk;
                if (actuallyExecuted) result.sacrificedTarget = currentUnit;
                break;

            case RelationshipEventEffectType.Buff:
                foreach (BaseBuffScriptableObject buff in relationshipEvent.buffsOnSource)
                {
                    result.buffs.Add(buff.GetBuff(source));
                }
                foreach (BaseBuffScriptableObject buff in relationshipEvent.buffsOnTarget)
                {
                    result.buffs.Add(buff.GetBuff(currentUnit));
                }
                break;

            case RelationshipEventEffectType.ChangeAction:
                actuallyExecuted = RandomEngine.Instance.Range(0f, 1f) < relationshipEvent.chance;
                if (actuallyExecuted)
                    result.changedActionTo = relationshipEvent.changeActionTo;
                else
                    result.changedActionTo = ChangeActionTypes.DidntChange;
                break;

            case RelationshipEventEffectType.FreeAttack:
                actuallyExecuted = RandomEngine.Instance.Range(0f, 1f) < relationshipEvent.chance;
                if (actuallyExecuted)
                {
                    result.freeAttack = true;
                    result.freeAttacker = currentUnit;
                }
                break;

            default:
                break;
        }

        if (relationshipEvent.interrupts && actuallyExecuted)
        {
            foreach (InterruptionScriptableObject interruption in relationshipEvent.interruptionsOnCurrent)
            {
                result.interruptions.Add(interruption.ToParameters(currentUnit, source));
            }
            foreach (InterruptionScriptableObject interruption in relationshipEvent.interruptionsOnSource)
            {
                result.interruptions.Add(interruption.ToParameters(currentUnit, source, false));
            }
        }

        print("executed " + relationshipEvent + " : " + actuallyExecuted);
        return actuallyExecuted;
    }

    private AllyCharacter GetBestDamagerOf(EnemyUnit enemy)
    {
        if (enemy == null) return null;

        float bestDmg = -1f;
        AllyCharacter bestDamager = null;

        if (_damageOnEnemies.ContainsKey(enemy))
        {
            var innerDict = _damageOnEnemies[enemy];
            foreach (var pair in innerDict)
            {
                if (pair.Value > bestDmg)
                {
                    bestDmg = pair.Value;
                    bestDamager = pair.Key;
                }
            }
        }

        return bestDamager;
    }

    public RelationshipEventsResult AllyOnEnemyAttackDamage(AllyUnit source, EnemyUnit target, float damage, bool crit, AllyUnit duo = null)
    {
        if (_damageOnEnemies.ContainsKey(target))
        {
            var innerDict = _damageOnEnemies[target];
            if (innerDict.ContainsKey(source.AllyCharacter)) innerDict[source.AllyCharacter] += damage;
            else innerDict.Add(source.AllyCharacter, damage);
        }
        else _damageOnEnemies.Add(target, new Dictionary<AllyCharacter, float> { { source.AllyCharacter, damage } });

        RelationshipEvent dummyTrigger  = ScriptableObject.CreateInstance("RelationshipEvent") as RelationshipEvent;
        dummyTrigger.triggerType        = RelationshipEventTriggerType.Attack;
        dummyTrigger.targetsAlly        = false;
        dummyTrigger.onDamage           = true;
        dummyTrigger.onCrit             = crit;                             // TODO : should be ok ? how can an ally detect your crit if no dmg is done
        dummyTrigger.onFatal            = target.Character.HealthPoints <= damage;    // TODO : is ok ? kill is a different event but fatal checks it before actual death

        return CheckTriggersAndExecute(dummyTrigger, source, duoUnit: duo);
    }

    public RelationshipEventsResult EnemyOnAllyAttackDamage(AllyUnit target, float damage, bool crit)
    {
        RelationshipEvent dummyTrigger = ScriptableObject.CreateInstance("RelationshipEvent") as RelationshipEvent;
        dummyTrigger.triggerType = RelationshipEventTriggerType.Attack;
        dummyTrigger.targetsAlly = true;
        dummyTrigger.onDamage = true;
        dummyTrigger.onCrit = crit;                             // TODO : should be ok ? how can an ally detect your crit if no dmg is done
        dummyTrigger.onFatal = target.Character.HealthPoints <= damage;    // TODO : is ok ? kill is a different event but fatal checks it before actual death

        return CheckTriggersAndExecute(dummyTrigger, target);
    }

    public RelationshipEventsResult FriendlyFireDamage(AllyUnit source, AllyUnit target, bool fatal, AllyUnit duo = null)
    {
        RelationshipEvent dummyTrigger = ScriptableObject.CreateInstance("RelationshipEvent") as RelationshipEvent;
        dummyTrigger.triggerType = RelationshipEventTriggerType.FriendlyFire;
        dummyTrigger.onFatal = fatal;

        return CheckTriggersAndExecute(dummyTrigger, source, allyTargetUnit: target, duoUnit: duo);
    }

    public RelationshipEventsResult AllyOnEnemyAttackHitOrMiss(AllyUnit source, bool hit, AllyUnit duo = null)
    {
        RelationshipEvent dummyTrigger  = ScriptableObject.CreateInstance("RelationshipEvent") as RelationshipEvent;
        dummyTrigger.triggerType        = RelationshipEventTriggerType.Attack;
        dummyTrigger.targetsAlly        = false;
        dummyTrigger.onHit              = hit;
        dummyTrigger.onMiss             = !hit;

        return CheckTriggersAndExecute(dummyTrigger, source, duoUnit: duo);
    }

    public RelationshipEventsResult AllyOnAllyAttackHitOrMiss(AllyUnit source, AllyUnit target, bool hit, AllyUnit duo = null)
    {
        RelationshipEvent dummyTrigger = ScriptableObject.CreateInstance("RelationshipEvent") as RelationshipEvent;
        dummyTrigger.triggerType = RelationshipEventTriggerType.Attack;
        dummyTrigger.targetsAlly = true;
        dummyTrigger.onHit = hit;
        dummyTrigger.onMiss = !hit;

        return CheckTriggersAndExecute(dummyTrigger, source, allyTargetUnit: target, duoUnit: duo);
    }

    public RelationshipEventsResult EnemyOnAllyAttackHitOrMiss(AllyUnit target, bool hit)
    {
        RelationshipEvent dummyTrigger = ScriptableObject.CreateInstance("RelationshipEvent") as RelationshipEvent;
        dummyTrigger.triggerType = RelationshipEventTriggerType.Attack;
        dummyTrigger.targetsAlly = true;
        dummyTrigger.onHit = hit;
        dummyTrigger.onMiss = !hit;

        return CheckTriggersAndExecute(dummyTrigger, target);
    }

    public RelationshipEventsResult HealAlly(AllyUnit source, AllyUnit target, AllyUnit duo = null)
    {
        RelationshipEvent dummyTrigger = ScriptableObject.CreateInstance("RelationshipEvent") as RelationshipEvent;
        dummyTrigger.triggerType = RelationshipEventTriggerType.Heal;

        return CheckTriggersAndExecute(dummyTrigger, source, allyTargetUnit: target, duoUnit: duo);
    }

    public RelationshipEventsResult KillEnemy(AllyUnit source, EnemyUnit target, AllyUnit duo = null)
    {
        RelationshipEvent dummyTrigger = ScriptableObject.CreateInstance("RelationshipEvent") as RelationshipEvent;
        dummyTrigger.triggerType = RelationshipEventTriggerType.Kill;
        dummyTrigger.targetsAlly = false;

        return CheckTriggersAndExecute(dummyTrigger, source, duoUnit: duo, enemyTargetUnit: target);
    }

    public RelationshipEventsResult EnemyKillAlly(AllyUnit target)
    {
        RelationshipEvent dummyTrigger = ScriptableObject.CreateInstance("RelationshipEvent") as RelationshipEvent;
        dummyTrigger.triggerType = RelationshipEventTriggerType.Kill;
        dummyTrigger.targetsAlly = true;

        return CheckTriggersAndExecute(dummyTrigger, target);
    }

    public RelationshipEventsResult BeginDuo(AllyUnit source, AllyUnit duo)
    {
        RelationshipEvent dummyTrigger = ScriptableObject.CreateInstance("RelationshipEvent") as RelationshipEvent;
        dummyTrigger.triggerType = RelationshipEventTriggerType.BeginDuo;

        return CheckTriggersAndExecute(dummyTrigger, source, duoUnit: duo);
    }

    public RelationshipEventsResult EndExecutedDuo(AllyUnit source, AllyUnit duo)
    {
        RelationshipEvent dummyTrigger = ScriptableObject.CreateInstance("RelationshipEvent") as RelationshipEvent;
        dummyTrigger.triggerType = RelationshipEventTriggerType.EndExecutedDuo;

        return CheckTriggersAndExecute(dummyTrigger, source, duoUnit: duo);
    }

    public RelationshipEventsResult ConfirmDuoExecution(AllyUnit source, AllyUnit duo)
    {
        RelationshipEvent dummyTrigger = ScriptableObject.CreateInstance("RelationshipEvent") as RelationshipEvent;
        dummyTrigger.triggerType = RelationshipEventTriggerType.ConfirmDuoExecution;

        return CheckTriggersAndExecute(dummyTrigger, source, allyTargetUnit: duo, duoUnit: duo);
    }

    public RelationshipEventsResult StartAction(ActionTypes action, AllyUnit source, AllyUnit duo)
    {
        RelationshipEvent dummyTrigger = ScriptableObject.CreateInstance("RelationshipEvent") as RelationshipEvent;
        dummyTrigger.triggerType = RelationshipEventTriggerType.StartAction;
        dummyTrigger.startedAction = action;

        return CheckTriggersAndExecute(dummyTrigger, duo, duoUnit: source); // inverted because of reaction paradigm
    }
}
