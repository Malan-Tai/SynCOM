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
        RelationshipEventsResult result = RelationshipEventsManager.Instance.AllyOnEnemyAttackHitOrMiss(source, hit, duo);
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
    }

    protected void AttackOnAllyHitOrMiss(AllyUnit source, AllyUnit target, bool hit, AllyUnit duo = null)
    {
        RelationshipEventsResult result = RelationshipEventsManager.Instance.AllyOnAllyAttackHitOrMiss(source, target, hit, duo);
        if (!hit)
        {
            AddInterruptionBeforeResult(ref result, new InterruptionParameters
            {
                interruptionType = InterruptionType.FocusTargetForGivenTimeAndFireTextFeedback,
                target = target,
                time = Interruption.FOCUS_TARGET_TIME,
                text = "Miss"
            });
        }

        HandleRelationshipEventResult(result);
    }

    protected void AttackDamage(AllyUnit source, EnemyUnit target, float damage, bool crit, AllyUnit duo = null)
    {
        bool killed = target.TakeDamage(ref damage, false);

        RelationshipEventsResult result = RelationshipEventsManager.Instance.AllyOnEnemyAttackDamage(source, target, damage, crit, duo);
        AddInterruptionBeforeResult(ref result, new InterruptionParameters
        {
            interruptionType = InterruptionType.FocusTargetForGivenTimeAndFireTextFeedback,
            target = target,
            time = Interruption.FOCUS_TARGET_TIME,
            text = "-" + damage
        });

        HandleRelationshipEventResult(result);

        if (killed) HandleRelationshipEventResult(RelationshipEventsManager.Instance.KillEnemy(source, target, duo));
    }

    protected void FriendlyFireDamage(AllyUnit source, AllyUnit target, float damage, AllyUnit duo = null)
    {
        bool killed = target.TakeDamage(ref damage, false);

        RelationshipEventsResult result = RelationshipEventsManager.Instance.FriendlyFireDamage(source, target, killed, duo);
        AddInterruptionBeforeResult(ref result, new InterruptionParameters
        {
            interruptionType = InterruptionType.FocusTargetForGivenTimeAndFireTextFeedback,
            target = target,
            time = Interruption.FOCUS_TARGET_TIME,
            text = "-" + damage
        });

        HandleRelationshipEventResult(result);

        // TODO : kill ally ?
    }

    protected void Heal(AllyUnit source, AllyUnit target, float healAmount, AllyUnit duo = null)
    {
        target.Heal(ref healAmount, false);

        RelationshipEventsResult result = RelationshipEventsManager.Instance.HealAlly(source, target, duo);
        AddInterruptionBeforeResult(ref result, new InterruptionParameters
        {
            interruptionType = InterruptionType.FocusTargetForGivenTimeAndFireTextFeedback,
            target = target,
            time = Interruption.FOCUS_TARGET_TIME,
            text = "+" + healAmount
        });

        HandleRelationshipEventResult(result);
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

    protected void AddBuff(GridBasedUnit unit, Buff buff)
    {
        unit.Character.CurrentBuffs.Add(buff);

        var parameters = new InterruptionParameters
        {
            interruptionType = InterruptionType.FocusTargetForGivenTimeAndFireTextFeedback,
            target = unit,
            time = Interruption.FOCUS_TARGET_TIME,
            text = buff.GetName()
        };
        _interruptionQueue.Enqueue(Interruption.GetInitializedInterruption(parameters));
    }
}

public abstract class BaseDuoAbility : BaseAllyAbility
{
    protected bool _ignoreEnemyTargeting = false;

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

    /// <summary>
    /// returns true if the action has been changed
    /// </summary>
    protected bool StartAction(ActionTypes action, AllyUnit source, AllyUnit duo)
    {
        RelationshipEventsResult result = RelationshipEventsManager.Instance.StartAction(action, source, duo);
        HandleRelationshipEventResult(result);

        bool changedAction = result.changedActionTo != ChangeActionTypes.DidntChange;
        switch (result.changedActionTo)
        {
            case ChangeActionTypes.AttackAlly:
                changedAction = AllyOnAllyShot(source, duo);
                break;

            case ChangeActionTypes.Positive:
                break;

            default:
                break;
        }

        return changedAction;
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

        bool forceConfirm = false;
        if (_chosenAlly == null)
        {
            AllyTargetingInput();
        }
        else if (!_ignoreEnemyTargeting)
        {
            EnemyTargetingInput();
        }
        else forceConfirm = true;

        bool confirmed = _uiConfirmed || Input.GetKeyDown(KeyCode.Return) || forceConfirm;
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

        var parameters = new InterruptionParameters
        {
            interruptionType = InterruptionType.FocusTargetForGivenTimeAndFireTextFeedback,
            target = _effector,
            time = Interruption.FOCUS_TARGET_TIME,
            text = gain >= 0 ? ":)" : ":("
        };
        _interruptionQueue.Enqueue(Interruption.GetInitializedInterruption(parameters));
    }

    protected void AllyToSelfModifySentiment(AllyUnit ally, EnumSentiment sentiment, int gain)
    {
        Relationship relationshipAllyToSelf = ally.AllyCharacter.Relationships[this._effector.AllyCharacter];
        relationshipAllyToSelf.IncreaseSentiment(sentiment, gain);

        var parameters = new InterruptionParameters
        {
            interruptionType = InterruptionType.FocusTargetForGivenTimeAndFireTextFeedback,
            target = ally,
            time = Interruption.FOCUS_TARGET_TIME,
            text = gain >= 0 ? ":)" : ":("
        };
        _interruptionQueue.Enqueue(Interruption.GetInitializedInterruption(parameters));
    }

    protected virtual ShootResult SelfShoot(GridBasedUnit target, AbilityStats selfShotStats, bool alwaysHit = false, bool canCrit = true)
    {
        if (StartAction(ActionTypes.Attack, _effector, _chosenAlly)) return new ShootResult(false, 0f, false); // TODO : fix this return

        int randShot = UnityEngine.Random.Range(0, 100); // between 0 and 99
        int randCrit = UnityEngine.Random.Range(0, 100);

        if (alwaysHit || randShot < selfShotStats.GetAccuracy(target, _effector.LinesOfSight[target].cover))
        {
            AttackHitOrMiss(_effector, target as EnemyUnit, true, _chosenAlly);

            if (canCrit && randCrit < selfShotStats.GetCritRate())
            {
                AttackDamage(_effector, target as EnemyUnit, selfShotStats.GetDamage() * 1.5f, true, _chosenAlly);
                return new ShootResult(true, selfShotStats.GetDamage() * 1.5f, true);
            }
            else
            {
                AttackDamage(_effector, target as EnemyUnit, selfShotStats.GetDamage(), false, _chosenAlly);
                return new ShootResult(true, selfShotStats.GetDamage(), false);
            }
        }
        else
        {
            AttackHitOrMiss(_effector, target as EnemyUnit, false, _chosenAlly);
            return new ShootResult(false, 0f, false);
        }
    }

    protected virtual ShootResult AllyShoot(GridBasedUnit target, AbilityStats allyShotStats, bool alwaysHit = false, bool canCrit = true)
    {
        if (StartAction(ActionTypes.Attack, _chosenAlly, _effector)) return new ShootResult(false, 0f, false); // TODO : fix this return

        int randShot = UnityEngine.Random.Range(0, 100); // between 0 and 99
        int randCrit = UnityEngine.Random.Range(0, 100);

        if (alwaysHit || randShot < allyShotStats.GetAccuracy(target, _chosenAlly.LinesOfSight[target].cover))
        {
            AttackHitOrMiss(_chosenAlly, target as EnemyUnit, true, _effector);

            if (canCrit && randCrit < allyShotStats.GetCritRate())
            {
                AttackDamage(_chosenAlly, target as EnemyUnit, allyShotStats.GetDamage() * 1.5f, true, _effector);
                return new ShootResult(true, allyShotStats.GetDamage() * 1.5f, true);
            }
            else
            {
                AttackDamage(_chosenAlly, target as EnemyUnit, allyShotStats.GetDamage(), false, _effector);
                return new ShootResult(true, allyShotStats.GetDamage(), false);
            }
        }
        else
        {
            AttackHitOrMiss(_chosenAlly, target as EnemyUnit, false, _effector);
            return new ShootResult(false, 0f, false);
        }
    }

    protected bool AllyOnAllyShot(AllyUnit shooterUnit, AllyUnit shotUnit)
    {
        Dictionary<GridBasedUnit, LineOfSight> los = shooterUnit.GetLineOfSights(false);
        if (!los.ContainsKey(shotUnit) || (shooterUnit.GridPosition - shotUnit.GridPosition).magnitude > shooterUnit.Character.RangeShot) return false;

        int randShot = UnityEngine.Random.Range(0, 100); // between 0 and 99
        int randCrit = UnityEngine.Random.Range(0, 100);

        var shotStats = new AbilityStats(0, 0, 1f, 0, 0, shooterUnit);

        if (randShot < shotStats.GetAccuracy(shotUnit, los[shotUnit].cover))
        {
            AttackOnAllyHitOrMiss(shooterUnit, shotUnit, true, shotUnit);

            if (randCrit < shotStats.GetCritRate())
            {
                FriendlyFireDamage(shooterUnit, shotUnit, shotStats.GetDamage() * 1.5f, shotUnit);
            }
            else
            {
                FriendlyFireDamage(shooterUnit, shotUnit, shotStats.GetDamage(), shotUnit);
            }
        }
        else
        {
            AttackOnAllyHitOrMiss(shooterUnit, shotUnit, false, shotUnit);
        }

        return true;
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

    public int GetRelationshipStatus(bool selfToAlly)
    {
        if (_effector == null || (_temporaryChosenAlly == null && _chosenAlly == null)) return -2;

        AllyCharacter ally = _temporaryChosenAlly == null ? _chosenAlly.AllyCharacter : _temporaryChosenAlly.AllyCharacter;
        Relationship relationshipSelfToAlly = this._effector.AllyCharacter.Relationships[ally];
        Relationship relationshipAllyToSelf = ally.Relationships[this._effector.AllyCharacter];

        return selfToAlly ? relationshipSelfToAlly.Status() : relationshipAllyToSelf.Status();
    }
}
