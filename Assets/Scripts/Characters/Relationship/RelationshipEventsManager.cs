using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelationshipEventsManager : MonoBehaviour
{
    #region Singleton
    private static RelationshipEventsManager instance;
    public static RelationshipEventsManager Instance { get { return instance; } }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
    #endregion

    [SerializeField]
    private RelationshipEvent[] _allEvents;

    private Dictionary<EnemyCharacter, Dictionary<AllyCharacter, float>> _damageOnEnemies;

    private void Start()
    {
        _damageOnEnemies = new Dictionary<EnemyCharacter, Dictionary<AllyCharacter, float>>();
    }

    private bool CheckTriggersAndExecute(RelationshipEvent dummyTrigger, AllyCharacter source, AllyCharacter target = null, AllyCharacter duo = null)
    {
        bool interrupts = false;

        foreach (AllyUnit ally in CombatGameManager.Instance.AllAllyUnits)
        {
            AllyCharacter allyCharacter = ally.Character as AllyCharacter;
            bool isDuo = allyCharacter == duo;

            foreach (RelationshipEvent relationshipEvent in _allEvents)
            {
                if (relationshipEvent.CorrespondsToTrigger(dummyTrigger, isDuo, allyCharacter.HealthPoints / allyCharacter.MaxHealth) &&
                    relationshipEvent.MeetsRelationshipRequirements(source, target, allyCharacter))
                {
                    interrupts = interrupts || relationshipEvent.interrupts;
                    Execute(relationshipEvent, source, allyCharacter);
                }
            }
        }

        return interrupts;
    }

    private void Execute(RelationshipEvent relationshipEvent, AllyCharacter source, AllyCharacter current)
    {
        print("executed " + relationshipEvent);

        switch (relationshipEvent.effectType)
        {
            case RelationshipEventEffectType.RelationshipGaugeChange:
                Relationship currentToSource = current.Relationships[source];
                currentToSource.IncreaseSentiment(EnumSentiment.Admiration, relationshipEvent.admirationChange);
                currentToSource.IncreaseSentiment(EnumSentiment.Trust, relationshipEvent.trustChange);
                currentToSource.IncreaseSentiment(EnumSentiment.Sympathy, relationshipEvent.sympathyChange);

                if (relationshipEvent.reciprocal)
                {
                    Relationship sourceToCurrent = source.Relationships[current];
                    sourceToCurrent.IncreaseSentiment(EnumSentiment.Admiration, relationshipEvent.admirationChange);
                    sourceToCurrent.IncreaseSentiment(EnumSentiment.Trust, relationshipEvent.trustChange);
                    sourceToCurrent.IncreaseSentiment(EnumSentiment.Sympathy, relationshipEvent.sympathyChange);
                }
                else if (relationshipEvent.sourceToTarget)
                {
                    Relationship sourceToCurrent = source.Relationships[current];
                    sourceToCurrent.IncreaseSentiment(EnumSentiment.Admiration, relationshipEvent.admirationChangeSTT);
                    sourceToCurrent.IncreaseSentiment(EnumSentiment.Trust, relationshipEvent.trustChangeSTT);
                    sourceToCurrent.IncreaseSentiment(EnumSentiment.Sympathy, relationshipEvent.sympathyChangeSTT);
                }

                break;

            default:
                break;
        }
    }

    private AllyCharacter GetBestDamagerOf(EnemyCharacter enemy)
    {
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

    public bool AllyOnEnemyAttackDamage(AllyCharacter source, EnemyCharacter target, float damage, bool crit, AllyCharacter duo = null)
    {
        if (_damageOnEnemies.ContainsKey(target))
        {
            var innerDict = _damageOnEnemies[target];
            if (innerDict.ContainsKey(source)) innerDict[source] += damage;
            else innerDict.Add(source, damage);
        }
        else _damageOnEnemies.Add(target, new Dictionary<AllyCharacter, float> { { source, damage } });

        RelationshipEvent dummyTrigger  = ScriptableObject.CreateInstance("RelationshipEvent") as RelationshipEvent;
        dummyTrigger.triggerType        = RelationshipEventTriggerType.Attack;
        dummyTrigger.targetsAlly        = false;
        dummyTrigger.onDamage           = true;
        dummyTrigger.onCrit             = crit;                             // TODO : should be ok ? how can an ally detect your crit if no dmg is done
        dummyTrigger.onFatal            = target.HealthPoints <= damage;    // TODO : is ok ? or kill should be a different event ?

        return CheckTriggersAndExecute(dummyTrigger, source, duo: duo);
    }

    public bool AllyOnEnemyAttackHitOrMiss(AllyCharacter source, bool hit, AllyCharacter duo = null)
    {
        RelationshipEvent dummyTrigger  = ScriptableObject.CreateInstance("RelationshipEvent") as RelationshipEvent;
        dummyTrigger.triggerType        = RelationshipEventTriggerType.Attack;
        dummyTrigger.targetsAlly        = false;
        dummyTrigger.onHit              = hit;
        dummyTrigger.onMiss             = !hit;

        return CheckTriggersAndExecute(dummyTrigger, source, duo: duo);
    }

    public bool Heal(AllyCharacter source, AllyCharacter target, AllyCharacter duo = null)
    {
        RelationshipEvent dummyTrigger = ScriptableObject.CreateInstance("RelationshipEvent") as RelationshipEvent;
        dummyTrigger.triggerType = RelationshipEventTriggerType.Heal;

        return CheckTriggersAndExecute(dummyTrigger, source, target, duo);
    }
}
