using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseAllyAbility : BaseAbility
{
    public delegate void EndAbility(bool executed);
    public event EndAbility OnAbilityEnded;

    protected bool _uiConfirmed = false;
    protected bool _uiCancelled = false;

    protected GridBasedUnit _hoveredUnit = null;
    protected new AllyUnit _effector;

    public delegate void EventRequestDescriptionUpdate(BaseAllyAbility ability);
    public static event EventRequestDescriptionUpdate OnDescriptionUpdateRequest;

    public delegate void EventRequestTargetsUpdate(IEnumerable<GridBasedUnit> targets);
    public static event EventRequestTargetsUpdate OnTargetsUpdateRequest;

    public delegate void EventRequestTargetSymbolUpdate(GridBasedUnit unit);
    public static event EventRequestTargetSymbolUpdate OnTargetSymbolUpdateRequest;

    public void HoverPortrait(GridBasedUnit unit)
    {
        _hoveredUnit = unit;
        RequestDescriptionUpdate();
    }

    protected void RequestDescriptionUpdate()
    {
        if (OnDescriptionUpdateRequest != null) OnDescriptionUpdateRequest(this);
    }

    protected void RequestTargetsUpdate(IEnumerable<GridBasedUnit> targets)
    {
        if (OnTargetsUpdateRequest != null) OnTargetsUpdateRequest(targets);
    }

    protected void RequestTargetSymbolUpdate(GridBasedUnit unit)
    {
        if (OnTargetSymbolUpdateRequest != null) OnTargetSymbolUpdateRequest(unit);
    }

    protected void AttackHitOrMiss(AllyUnit source, EnemyUnit target, bool hit, AllyCharacter duo = null)
    {
        target.Missed();
        if (RelationshipEventsManager.Instance.AllyOnEnemyAttackHitOrMiss(source.AllyCharacter, hit, duo).interrupts)
        {
            Debug.Log("interrupted");
        }
    }

    protected void AttackDamage(AllyUnit source, EnemyUnit target, float damage, bool crit, AllyCharacter duo = null)
    {
        bool killed = target.TakeDamage(damage);
        if (RelationshipEventsManager.Instance.AllyOnEnemyAttackDamage(source.AllyCharacter, target.EnemyCharacter, damage, crit, duo).interrupts)
        {
            Debug.Log("interrupted");
        }

        if (killed && RelationshipEventsManager.Instance.KillEnemy(source.AllyCharacter, target.EnemyCharacter, duo).interrupts)
        {
            Debug.Log("interrupted");
        }
    }

    protected void FriendlyFireDamage(AllyUnit source, AllyUnit target, float damage, AllyCharacter duo = null)
    {
        bool killed = target.TakeDamage(damage);
        if (RelationshipEventsManager.Instance.FriendlyFireDamage(source.AllyCharacter, target.AllyCharacter, duo).interrupts)
        {
            Debug.Log("interrupted");
        }

        // TODO : kill ally ?
    }

    protected void Heal(AllyUnit source, AllyUnit target, float healAmount, AllyCharacter duo = null)
    {
        target.Heal(healAmount);
        if (RelationshipEventsManager.Instance.HealAlly(source.AllyCharacter, target.AllyCharacter, duo).interrupts)
        {
            Debug.Log("interrupted");
        }
    }

    protected bool TryBeginDuo(AllyUnit source, AllyUnit duo)
    {
        RelationshipEventsResult eventResult            = RelationshipEventsManager.Instance.BeginDuo(source.AllyCharacter, duo.AllyCharacter);
        RelationshipEventsResult invertedEventResult    = RelationshipEventsManager.Instance.BeginDuo(duo.AllyCharacter, source.AllyCharacter);

        if (eventResult.interrupts) // TODO : what happens if both interrupt ?
        {
            Debug.Log("interrupted");
        }

        return eventResult.refusedDuo || invertedEventResult.refusedDuo;
    }

    public override void SetEffector(GridBasedUnit effector)
    {
        _effector = effector as AllyUnit;
    }

    public virtual void UISelectUnit(GridBasedUnit unit)
    {
        RequestTargetSymbolUpdate(unit);
    }

    protected abstract void EnemyTargetingInput();

    protected virtual void FinalizeAbility(bool executed)
    {
        if (!executed) CombatGameManager.Instance.Camera.SwitchParenthood(_effector);

        _hoveredUnit = null;
        _effector = null;
        if (OnAbilityEnded != null) OnAbilityEnded(executed);
    }

    public virtual void InputControl()
    {
        EnemyTargetingInput();

        bool confirmed = _uiConfirmed || Input.GetKeyDown(KeyCode.Return);
        bool cancelled = _uiCancelled || Input.GetKeyDown(KeyCode.Escape);

        if (confirmed && CanExecute())
        {
            Execute();
            FinalizeAbility(true);
        }
        else if (confirmed)
        {
            Debug.Log("cant use this ability");
            FinalizeAbility(false);
        }
        else if (cancelled)
        {
            Debug.Log("cancelled ability");
            CombatGameManager.Instance.Camera.SwitchParenthood(_effector);
            FinalizeAbility(false);
        }

        _uiConfirmed = false;
        _uiCancelled = false;
    }

    public void UIConfirm()
    {
        _uiConfirmed = true;
    }

    public void UICancel()
    {
        _uiCancelled = true;
    }
}

public abstract class BaseDuoAbility : BaseAllyAbility
{
    protected AllyUnit _temporaryChosenAlly = null;
    protected AllyUnit _chosenAlly = null;
    private List<AllyUnit> _possibleAllies = new List<AllyUnit>();

    protected abstract bool IsAllyCompatible(AllyUnit unit);
    protected abstract void ChooseAlly();

    public abstract string GetAllyDescription();

    public override void UISelectUnit(GridBasedUnit unit)
    {
        if (unit is AllyUnit && _chosenAlly == null)
        {
            _temporaryChosenAlly = unit as AllyUnit;
            CombatGameManager.Instance.Camera.SwitchParenthood(_temporaryChosenAlly);
            RequestDescriptionUpdate();
            RequestTargetSymbolUpdate(_temporaryChosenAlly);
        }
    }

    public override void SetEffector(GridBasedUnit effector)
    {
        base.SetEffector(effector);

        foreach (AllyUnit unit in CombatGameManager.Instance.ControllableUnits)
        {
            if (unit != effector && IsAllyCompatible(unit))
            {
                Relationship relationship = _effector.AllyCharacter.Relationships[unit.AllyCharacter];
                if (!relationship.CheckedDuoRefusal || relationship.AcceptedDuo)
                {
                    _possibleAllies.Add(unit);
                }
            }
        }

        if (_possibleAllies.Count > 0)
        {
            _temporaryChosenAlly = _possibleAllies[0];
            CombatGameManager.Instance.Camera.SwitchParenthood(_temporaryChosenAlly);

            RequestTargetsUpdate(_possibleAllies);
            RequestTargetSymbolUpdate(_temporaryChosenAlly);
        }
        else FinalizeAbility(false);
    }

    protected override void FinalizeAbility(bool executed)
    {
        if (_chosenAlly != null) _chosenAlly.StopUsingAbilityAsAlly(executed);

        _temporaryChosenAlly = null;
        _chosenAlly = null;
        _possibleAllies.Clear();
        base.FinalizeAbility(executed);
    }

    public override void InputControl()
    {
        if (_chosenAlly == null)
        {
            AllyTargetingInput();
        }
        else
        {
            EnemyTargetingInput();
        }

        bool confirmed = _uiConfirmed || Input.GetKeyDown(KeyCode.Return);
        bool cancelled = _uiCancelled || Input.GetKeyDown(KeyCode.Escape);

        if (confirmed && CanExecute())
        {
            Execute();
            FinalizeAbility(true);
        }
        else if (confirmed && _temporaryChosenAlly != null && _chosenAlly == null)
        {
            Relationship relationship = _effector.AllyCharacter.Relationships[_temporaryChosenAlly.AllyCharacter];
            Relationship invertedRelationship = _temporaryChosenAlly.AllyCharacter.Relationships[_effector.AllyCharacter];

            if ((relationship.CheckedDuoRefusal && relationship.AcceptedDuo) || !TryBeginDuo(_effector, _temporaryChosenAlly))
            {
                relationship.CheckedDuoRefusal = true;
                relationship.AcceptedDuo = true;

                invertedRelationship.CheckedDuoRefusal = true;
                invertedRelationship.AcceptedDuo = true;

                _chosenAlly = _temporaryChosenAlly;
                ChooseAlly();
                RequestDescriptionUpdate();
            }
            else
            {
                Debug.Log("refuse to cooperate");

                relationship.CheckedDuoRefusal = true;
                relationship.AcceptedDuo = false;

                invertedRelationship.CheckedDuoRefusal = true;
                invertedRelationship.AcceptedDuo = false;

                if (_possibleAllies.Count <= 1) FinalizeAbility(false);
                else
                {
                    AllyUnit refused = _temporaryChosenAlly;

                    int index = _possibleAllies.IndexOf(refused) + 1;
                    if (index >= _possibleAllies.Count) index = 0;
                    _temporaryChosenAlly = _possibleAllies[index];
                    CombatGameManager.Instance.Camera.SwitchParenthood(_temporaryChosenAlly);

                    _possibleAllies.Remove(refused);

                    RequestDescriptionUpdate();
                    RequestTargetsUpdate(_possibleAllies);
                    RequestTargetSymbolUpdate(_temporaryChosenAlly);
                }
            }
            // TODO: check if Emotion gives a free action
        }
        else if (confirmed)
        {
            Debug.Log("cant use this ability");
            FinalizeAbility(false);
        }
        else if (cancelled)
        {
            Debug.Log("cancelled ability");
            FinalizeAbility(false);
        }

        _uiConfirmed = false;
        _uiCancelled = false;
    }

    protected void AllyTargetingInput()
    {
        if (_possibleAllies.Count < 1) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitData;

        bool changedUnitThisFrame = false;

        if (Physics.Raycast(ray, out hitData, 1000))
        {
            var hitUnit = hitData.transform.GetComponent<AllyUnit>();

            bool clicked = Input.GetMouseButtonUp(0);

            if (hitUnit != null && clicked && hitUnit != _effector && IsAllyCompatible(hitUnit))
            {
                _temporaryChosenAlly = hitUnit;
                changedUnitThisFrame = true;
            }
        }

        if (Input.GetKeyDown(KeyCode.Tab) && !changedUnitThisFrame)
        {
            int index = _possibleAllies.IndexOf(_temporaryChosenAlly);
            index++;
            if (index >= _possibleAllies.Count) index = 0;
            _temporaryChosenAlly = _possibleAllies[index];
            changedUnitThisFrame = true;
        }

        if (changedUnitThisFrame)
        {
            CombatGameManager.Instance.Camera.SwitchParenthood(_temporaryChosenAlly);
            RequestDescriptionUpdate();
            RequestTargetSymbolUpdate(_temporaryChosenAlly);
        }
    }

    /// <summary>
    /// shouldn't be used for hit or miss or other generic events, but for very specific stuff
    /// </summary>
    protected void SelfToAllyModifySentiment(AllyUnit ally, EnumSentiment sentiment, int gain)
    {
        Relationship relationshipSelfToAlly = this._effector.AllyCharacter.Relationships[ally.AllyCharacter];
        relationshipSelfToAlly.IncreaseSentiment(sentiment, gain);
    }

    protected void AllyToSelfModifySentiment(AllyUnit ally, EnumSentiment sentiment, int gain)
    {
        Relationship relationshipAllyToSelf = ally.AllyCharacter.Relationships[this._effector.AllyCharacter];
        relationshipAllyToSelf.IncreaseSentiment(sentiment, gain);
    }

    protected virtual void SelfShoot(GridBasedUnit target, AbilityStats selfShotStats)
    {
        int randShot = UnityEngine.Random.Range(0, 100); // between 0 and 99
        int randCrit = UnityEngine.Random.Range(0, 100);

        Debug.Log("self to hit: " + randShot + " for " + selfShotStats.GetAccuracy(target, _effector.LinesOfSight[target].cover));

        if (randShot < selfShotStats.GetAccuracy(target, _effector.LinesOfSight[target].cover))
        {
            AttackHitOrMiss(_effector, target as EnemyUnit, true, _chosenAlly.AllyCharacter);

            if (randCrit < selfShotStats.GetCritRate())
            {
                AttackDamage(_effector, target as EnemyUnit, selfShotStats.GetDamage() * 1.5f, true, _chosenAlly.AllyCharacter);
            }
            else
            {
                AttackDamage(_effector, target as EnemyUnit, selfShotStats.GetDamage(), false, _chosenAlly.AllyCharacter);
            }
        }
        else
        {
            Debug.Log("self missed");
            AttackHitOrMiss(_effector, target as EnemyUnit, false, _chosenAlly.AllyCharacter);
        }
    }

    protected virtual void AllyShoot(GridBasedUnit target, AbilityStats allyShotStats)
    {
        int randShot = UnityEngine.Random.Range(0, 100); // between 0 and 99
        int randCrit = UnityEngine.Random.Range(0, 100);

        Debug.Log("ally to hit: " + randShot + " for " + allyShotStats.GetAccuracy(target, _chosenAlly.LinesOfSight[target].cover));

        if (randShot < allyShotStats.GetAccuracy(target, _chosenAlly.LinesOfSight[target].cover))
        {
            //SelfToAllyModifySentiment(_chosenAlly, EnumSentiment.Admiration, 5);
            AttackHitOrMiss(_chosenAlly, target as EnemyUnit, true, _effector.AllyCharacter);

            if (randCrit < allyShotStats.GetCritRate())
            {
                //target.Character.TakeDamage(allyShotStats.GetDamage() * 1.5f);
                //SelfToAllyModifySentiment(_chosenAlly, EnumSentiment.Sympathy, 5);
                AttackDamage(_chosenAlly, target as EnemyUnit, allyShotStats.GetDamage() * 1.5f, true, _effector.AllyCharacter);
            }
            else
            {
                //target.Character.TakeDamage(allyShotStats.GetDamage());
                AttackDamage(_chosenAlly, target as EnemyUnit, allyShotStats.GetDamage(), false, _effector.AllyCharacter);
            }
        }
        else
        {
            Debug.Log("ally missed");
            //SelfToAllyModifySentiment(_chosenAlly, EnumSentiment.Admiration, -5);
            AttackHitOrMiss(_chosenAlly, target as EnemyUnit, false, _effector.AllyCharacter);
        }
    }

    public Sprite GetSelfPortrait()
    {
        if (_effector == null) return CombatGameManager.Instance.CurrentUnit.GetPortrait();
        return _effector.GetPortrait();
    }

    public Sprite GetAllyPortrait()
    {
        if (_chosenAlly != null) return _chosenAlly.GetPortrait();
        else if (_hoveredUnit != null) return _hoveredUnit.GetPortrait();
        else if (_temporaryChosenAlly != null) return _temporaryChosenAlly.GetPortrait();
        else return null;
    }
}
