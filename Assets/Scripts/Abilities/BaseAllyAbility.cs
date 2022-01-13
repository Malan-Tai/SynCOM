using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseAllyAbility : BaseAbility
{
    public delegate void EndAbilityEvent(bool executed);
    public event EndAbilityEvent OnAbilityEnded;

    protected bool _uiConfirmed = false;
    protected bool _uiCancelled = false;

    protected bool _needsFinalization = false;
    protected bool _executed;
    protected bool _free = false;

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

    #region RelationshipEvents
    protected override void HandleRelationshipEventResult(RelationshipEventsResult result)
    {
        base.HandleRelationshipEventResult(result);
        _free = result.freeActionForSource;
    }

    protected void AttackHitOrMiss(AllyUnit source, EnemyUnit target, bool hit, AllyUnit duo = null)
    {
        if (!hit) target.Missed();
        HandleRelationshipEventResult(RelationshipEventsManager.Instance.AllyOnEnemyAttackHitOrMiss(source, hit, duo));
    }

    protected void AttackDamage(AllyUnit source, EnemyUnit target, float damage, bool crit, AllyUnit duo = null)
    {
        bool killed = target.TakeDamage(damage);
        HandleRelationshipEventResult(RelationshipEventsManager.Instance.AllyOnEnemyAttackDamage(source, target, damage, crit, duo));

        if (killed) HandleRelationshipEventResult(RelationshipEventsManager.Instance.KillEnemy(source, target, duo));
    }

    protected void FriendlyFireDamage(AllyUnit source, AllyUnit target, float damage, AllyUnit duo = null)
    {
        bool killed = target.TakeDamage(damage);
        HandleRelationshipEventResult(RelationshipEventsManager.Instance.FriendlyFireDamage(source, target, killed, duo));

        // TODO : kill ally ?
    }

    protected void Heal(AllyUnit source, AllyUnit target, float healAmount, AllyUnit duo = null)
    {
        target.Heal(healAmount);
        HandleRelationshipEventResult(RelationshipEventsManager.Instance.HealAlly(source, target, duo));
    }
    #endregion

    public override void SetEffector(GridBasedUnit effector)
    {
        base.SetEffector(effector);
        _effector = effector as AllyUnit;
    }

    public virtual void UISelectUnit(GridBasedUnit unit)
    {
        RequestTargetSymbolUpdate(unit);
    }

    protected abstract void EnemyTargetingInput();

    protected void FinalizeAbility(bool executed)
    {
        _needsFinalization = true;
        _executed = executed;
    }

    protected virtual void EndAbility()
    {
        if (!_executed || _free) CombatGameManager.Instance.Camera.SwitchParenthood(_effector);

        _hoveredUnit = null;
        _effector = null;
        _needsFinalization = false;
        if (OnAbilityEnded != null) OnAbilityEnded(_executed && !_free);
        _free = false;
    }

    public virtual void InputControl()
    {
        if (!_interruptionQueue.IsEmpty()) return;
        if (_needsFinalization)
        {
            EndAbility();
            return;
        }

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

    protected bool _freeForDuo = false;

    #region Relationship events
    protected override void HandleRelationshipEventResult(RelationshipEventsResult result)
    {
        base.HandleRelationshipEventResult(result);

        _freeForDuo = _freeForDuo || result.freeActionForDuo;

        if (_possibleAllies.Contains(result.stolenDuoUnit)) _chosenAlly = result.stolenDuoUnit;
    }

    protected bool TryBeginDuo(AllyUnit source, AllyUnit duo)
    {
        RelationshipEventsResult eventResult = RelationshipEventsManager.Instance.BeginDuo(source, duo);
        RelationshipEventsResult invertedEventResult = RelationshipEventsManager.Instance.BeginDuo(duo, source);

        HandleRelationshipEventResult(eventResult); // TODO : what happens if both interrupt ?

        return eventResult.refusedDuo || invertedEventResult.refusedDuo;
    }

    protected void ConfirmDuoExecution(AllyUnit source, AllyUnit duo)
    {
        HandleRelationshipEventResult(RelationshipEventsManager.Instance.ConfirmDuoExecution(source, duo));
    }

    protected void EndExecutedDuo(AllyUnit source, AllyUnit duo)
    {
        HandleRelationshipEventResult(RelationshipEventsManager.Instance.EndExecutedDuo(source, duo));
    }
    #endregion

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
        /*else Caused some cursed nullpointer exception because it wasn't updating UI
        {
            FinalizeAbility(false);
        }*/
    }

    protected override void EndAbility()
    {
        if (_executed) EndExecutedDuo(_effector, _chosenAlly);

        if (_chosenAlly != null) _chosenAlly.StopUsingAbilityAsAlly(_executed && !_freeForDuo);

        _temporaryChosenAlly = null;
        _chosenAlly = null;
        _freeForDuo = false;
        _possibleAllies.Clear();
        base.EndAbility();
    }

    public override void InputControl()
    {
        if (!_interruptionQueue.IsEmpty()) return;
        if (_needsFinalization)
        {
            EndAbility();
            return;
        }

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
            ConfirmDuoExecution(_effector, _chosenAlly);
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

    protected virtual void SelfShoot(GridBasedUnit target, AbilityStats selfShotStats, bool alwaysHit = false, bool canCrit = true)
    {
        int randShot = UnityEngine.Random.Range(0, 100); // between 0 and 99
        int randCrit = UnityEngine.Random.Range(0, 100);

        //Debug.Log("self to hit: " + randShot + " for " + selfShotStats.GetAccuracy(target, _effector.LinesOfSight[target].cover));

        if (alwaysHit || randShot < selfShotStats.GetAccuracy(target, _effector.LinesOfSight[target].cover))
        {
            AttackHitOrMiss(_effector, target as EnemyUnit, true, _chosenAlly);

            if (canCrit && randCrit < selfShotStats.GetCritRate())
            {
                AttackDamage(_effector, target as EnemyUnit, selfShotStats.GetDamage() * 1.5f, true, _chosenAlly);
            }
            else
            {
                AttackDamage(_effector, target as EnemyUnit, selfShotStats.GetDamage(), false, _chosenAlly);
            }
        }
        else
        {
            Debug.Log("self missed");
            AttackHitOrMiss(_effector, target as EnemyUnit, false, _chosenAlly);
        }
    }

    protected virtual void AllyShoot(GridBasedUnit target, AbilityStats allyShotStats, bool alwaysHit = false, bool canCrit = true)
    {
        int randShot = UnityEngine.Random.Range(0, 100); // between 0 and 99
        int randCrit = UnityEngine.Random.Range(0, 100);

        Debug.Log("ally to hit: " + randShot + " for " + allyShotStats.GetAccuracy(target, _chosenAlly.LinesOfSight[target].cover));

        if (alwaysHit || randShot < allyShotStats.GetAccuracy(target, _chosenAlly.LinesOfSight[target].cover))
        {
            AttackHitOrMiss(_chosenAlly, target as EnemyUnit, true, _effector);

            if (canCrit && randCrit < allyShotStats.GetCritRate())
            {
                AttackDamage(_chosenAlly, target as EnemyUnit, allyShotStats.GetDamage() * 1.5f, true, _effector);
            }
            else
            {
                AttackDamage(_chosenAlly, target as EnemyUnit, allyShotStats.GetDamage(), false, _effector);
            }
        }
        else
        {
            Debug.Log("ally missed");
            AttackHitOrMiss(_chosenAlly, target as EnemyUnit, false, _effector);
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
