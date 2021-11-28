using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseAbility
{
    public delegate void EndAbility(bool executed);
    public event EndAbility OnAbilityEnded;

    protected bool _uiConfirmed = false;
    protected bool _uiCancelled = false;

    protected AllyUnit _effector;

    public delegate void EventRequestDescriptionUpdate(BaseAbility ability);
    public static event EventRequestDescriptionUpdate OnDescriptionUpdateRequest;

    protected void RequestDescriptionUpdate()
    {
        if (OnDescriptionUpdateRequest != null) OnDescriptionUpdateRequest(this);
    }

    public void SetUIConfirmed()
    {
        _uiConfirmed = true;
    }

    public void SetUICancelled()
    {
        _uiCancelled = true;
    }

    public virtual void SetEffector(AllyUnit effector)
    {
        _effector = effector;
    }

    protected abstract void EnemyTargetingInput();
    protected abstract bool CanExecute();
    protected abstract void Execute();

    protected virtual void FinalizeAbility(bool executed)
    {
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

    /// <summary>
    /// Returns the name of the ability.
    /// The name of the ability is not an attribute : it's determined by the return value of this function.
    /// </summary>
    public abstract string GetName();

    /// <summary>
    /// Returns a short description of the ability.
    /// The description of the ability is not an attribute : it's determined by the return value of this function.
    /// </summary>
    public abstract string GetDescription();
}

public abstract class BaseDuoAbility : BaseAbility
{
    private AllyUnit _temporaryChosenAlly = null;
    protected AllyUnit _chosenAlly = null;
    private List<AllyUnit> _possibleAllies = null;

    protected abstract bool IsAllyCompatible(AllyUnit unit);
    protected abstract void ChooseAlly();

    public abstract string GetAllyDescription();

    public override void SetEffector(AllyUnit effector)
    {
        base.SetEffector(effector);

        _possibleAllies = new List<AllyUnit>();
        foreach (AllyUnit unit in CombatGameManager.Instance.ControllableUnits)
        {
            if (unit != effector && IsAllyCompatible(unit))
            {
                _possibleAllies.Add(unit);
            }
        }

        if (_possibleAllies.Count > 0)
        {
            _temporaryChosenAlly = _possibleAllies[0];
            CombatGameManager.Instance.Camera.SwitchParenthood(_temporaryChosenAlly);
        }
    }

    protected override void FinalizeAbility(bool executed)
    {
        if (_chosenAlly != null) _chosenAlly.StopUsingAbilityAsAlly(executed);

        _temporaryChosenAlly = null;
        _chosenAlly = null;
        _possibleAllies = null;
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
            _chosenAlly = _temporaryChosenAlly;
            ChooseAlly();
            RequestDescriptionUpdate();
            // TODO: check if 1) ally refuse to cooperate and 2) Emotion gives a free action
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
        }
    }

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
            AllyToSelfModifySentiment(_chosenAlly, EnumSentiment.Admiration, 5);

            if (randCrit < selfShotStats.GetCritRate())
            {
                target.Character.TakeDamage(selfShotStats.GetDamage() * 1.5f);
                SelfToAllyModifySentiment(_chosenAlly, EnumSentiment.Sympathy, 5);
                AllyToSelfModifySentiment(_chosenAlly, EnumSentiment.Sympathy, 5);
            }
            else
            {
                target.Character.TakeDamage(selfShotStats.GetDamage());
            }
        }
        else
        {
            Debug.Log("self missed");
            SelfToAllyModifySentiment(_chosenAlly, EnumSentiment.Admiration, -5);
            AllyToSelfModifySentiment(_chosenAlly, EnumSentiment.Admiration, -5);
        }
    }

    protected virtual void AllyShoot(GridBasedUnit target, AbilityStats allyShotStats)
    {
        int randShot = UnityEngine.Random.Range(0, 100); // between 0 and 99
        int randCrit = UnityEngine.Random.Range(0, 100);

        Debug.Log("ally to hit: " + randShot + " for " + allyShotStats.GetAccuracy(target, _chosenAlly.LinesOfSight[target].cover));

        if (randShot < allyShotStats.GetAccuracy(target, _chosenAlly.LinesOfSight[target].cover))
        {
            SelfToAllyModifySentiment(_chosenAlly, EnumSentiment.Admiration, 5);

            if (randCrit < allyShotStats.GetCritRate())
            {
                target.Character.TakeDamage(allyShotStats.GetDamage() * 1.5f);
                SelfToAllyModifySentiment(_chosenAlly, EnumSentiment.Sympathy, 5);
            }
            else
            {
                target.Character.TakeDamage(allyShotStats.GetDamage());
            }
        }
        else
        {
            Debug.Log("ally missed");
            SelfToAllyModifySentiment(_chosenAlly, EnumSentiment.Admiration, -5);
        }
    }
}
