using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseAbility
{
    public delegate void EndAbility(bool executed);
    public event EndAbility OnAbilityEnded;

    protected AllyUnit _effector;
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

        if (Input.GetKeyDown(KeyCode.Return) && CanExecute())
        {
            Execute();
            FinalizeAbility(true);
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log("cant use this ability");
            FinalizeAbility(false);
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("cancelled ability");
            CombatGameManager.Instance.Camera.SwitchParenthood(_effector);
            FinalizeAbility(false);
        }
    }
}

public abstract class BaseDuoAbility : BaseAbility
{
    private AllyUnit _temporaryChosenAlly = null;
    protected AllyUnit _chosenAlly = null;
    private List<AllyUnit> _possibleAllies = null;

    // Modifiers used in damage, protection, aim, crit rate calculations
    // that depends on the Emotions felt by me towards the chosenAlly...
    protected float selfEmotionDamageModifier = 1;
    protected float selfEmotionProtectionModifier = 1.6f;   // in [1.2, 2]
    protected float selfEmotionSuccessModifier = 1;         // for debuffs only : in [0.5, 1]
    protected float selfEmotionMissModifier = 1;            // for buffs only : in [0.5, 1]
    protected float selfEmotionCritRateModifier = 1;        // for debuffs only : in [0.5, 1]
    protected float selfEmotionCritFailModifier = 1;        // for buffs only : in [0.5, 1]
    // ... and that depends on the Emotions felt by the chosenAlly towards me
    protected float allyEmotionDamageModifier = 1;
    protected float allyEmotionProtectionModifier = 1.6f;
    protected float allyEmotionSuccessModifier = 1;
    protected float allyEmotionMissModifier = 1;
    protected float allyEmotionCritRateModifier = 1;
    protected float allyEmotionCritFailModifier = 1;

    protected abstract bool IsAllyCompatible(AllyUnit unit);
    protected abstract void ChooseAlly();

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
        _chosenAlly.StopUsingAbilityAsAlly(executed);

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

        if (Input.GetKeyDown(KeyCode.Return) && CanExecute())
        {
            Execute();
            FinalizeAbility(true);
        }
        else if (Input.GetKeyDown(KeyCode.Return) && _temporaryChosenAlly != null && _chosenAlly == null)
        {
            _chosenAlly = _temporaryChosenAlly;
            // _chosenAlly.UseAbilityAsAlly(this);
            ChooseAlly();
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log("cant use this ability");
            FinalizeAbility(false);
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("cancelled ability");
            CombatGameManager.Instance.Camera.SwitchParenthood(_effector);
            FinalizeAbility(false);
        }
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

        if (changedUnitThisFrame) CombatGameManager.Instance.Camera.SwitchParenthood(_temporaryChosenAlly);
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

    protected void UpdateSelfEmotionModifiers()
    {
        Relationship relationshipSelfToAlly = this._effector.AllyCharacter.Relationships[_chosenAlly.AllyCharacter];
        List<EnumEmotions> listEmotions = relationshipSelfToAlly.ListEmotions;
        // TODO: Stacking : highest buff - highest debuf; two buffs don't stack
        // For the moment everything stacks
        foreach (EnumEmotions emotion in listEmotions)
        {
            switch (emotion)
            {
                case (EnumEmotions.Scorn):
                    selfEmotionProtectionModifier -= 0.4f;
                    break;
                case (EnumEmotions.Esteem):
                    selfEmotionProtectionModifier += 0.4f;
                    break;
                case (EnumEmotions.Prejudice):
                    selfEmotionProtectionModifier -= 0.4f;
                    break;
                case (EnumEmotions.Submission):
                    selfEmotionProtectionModifier += 0.2f;
                    selfEmotionSuccessModifier -= 0.25f;
                    selfEmotionCritFailModifier -= 0.25f;
                    break;
                case (EnumEmotions.Terror):
                    selfEmotionSuccessModifier -= 0.25f;
                    break;
                case (EnumEmotions.ConflictedFeelings):
                    break;
                case (EnumEmotions.Faith):
                    selfEmotionMissModifier -= 0.5f;
                    selfEmotionProtectionModifier += 0.4f;
                    break;
                case (EnumEmotions.Respect):
                    selfEmotionMissModifier -= 0.5f;
                    break;
                case (EnumEmotions.Condescension):
                    selfEmotionProtectionModifier -= 0.6f;
                    selfEmotionDamageModifier -= 0.5f;
                    break;
                case (EnumEmotions.Recognition):
                    selfEmotionMissModifier -= 0.25f;
                    selfEmotionProtectionModifier += 0.2f;
                    selfEmotionDamageModifier -= 0.5f;
                    break;
                case (EnumEmotions.Hate):
                    selfEmotionDamageModifier -= 0.25f;
                    selfEmotionSuccessModifier -= 0.75f;
                    break;
                case (EnumEmotions.ReluctantTrust):
                    selfEmotionMissModifier -= 0.5f;
                    selfEmotionDamageModifier -= 0.5f;
                    break;
                case (EnumEmotions.Hostility):
                    selfEmotionDamageModifier -= 0.5f;
                    break;
                case (EnumEmotions.Pity):
                    selfEmotionProtectionModifier += 0.2f;
                    break;
                case (EnumEmotions.Devotion):
                    selfEmotionProtectionModifier += 0.4f;
                    break;
                case (EnumEmotions.Apprehension):
                    selfEmotionSuccessModifier -= 0.25f;
                    selfEmotionCritFailModifier -= 0.25f;
                    break;
                case (EnumEmotions.Friendship):
                    selfEmotionMissModifier -= 0.25f;
                    break;
                case (EnumEmotions.Empathy):
                    int test = Random.Range(0, 100);
                    if (test < 50) { Debug.Log("Action gratuite !"); }
                    else { Debug.Log("Pas d'action gratuite..."); }
                    break;
                default:
                    break;
            }
        }
    }

    protected void updateAllyEmotionModifiers()
    {

    }
}
