using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllyUnit : GridBasedUnit
{
    private BaseAbility _currentAbility = null;
    public BaseAbility CurrentAbility { get { return _currentAbility; } }

    public AllyCharacter AllyCharacter { get => (AllyCharacter)_character; }

    public delegate void EventStartUsingAbility(BaseAbility ability);
    public static event EventStartUsingAbility OnStartedUsingAbility;

    public delegate void EventStopUsingAbility();
    public static event EventStopUsingAbility OnStoppedUsingAbility;

    protected override bool IsEnemy()
    {
        return false;
    }

    public void UpdateEnemyVisibilities()
    {
        foreach (var pair in _linesOfSight)
        {
            var enemy = (EnemyUnit)pair.Key;
            enemy.UpdateVisibility(true, pair.Value.cover);
        }
    }

    public void UseAbility(BaseAbility ability)
    {
        if (ability == _currentAbility) return;

        _currentAbility = ability;
        ability.SetEffector(this);
        ability.OnAbilityEnded += StopUsingAbility;
        if (OnStartedUsingAbility != null) OnStartedUsingAbility(ability);
    }

    //public void UseAbilityAsAlly(BaseAbility ability)
    //{
    //    _currentAbility = ability;
    //    ability.OnAbilityEnded += StopUsingAbility;
    //}

    private void StopUsingAbility(bool executed)
    {
        _currentAbility.OnAbilityEnded -= StopUsingAbility;
        _currentAbility = null;

        if (executed)
        {
            CombatGameManager.Instance.FinishAllyUnitTurn(this);
        }

        if (OnStoppedUsingAbility != null) OnStoppedUsingAbility();
    }

    public void StopUsingAbilityAsAlly(bool executed)
    {
        //_currentAbility.OnAbilityEnded -= StopUsingAbility;
        //_currentAbility = null;

        if (executed)
        {
            CombatGameManager.Instance.FinishAllyUnitTurn(this, true);
        }
    }

    public void NewTurn()
    {
        _movesLeft = 10f;
        NeedsPathfinderUpdate();
        UpdateLineOfSights(!IsEnemy());
    }

    public void UseCharacterSprite()
    {
        SpriteRenderer renderer = GetComponentInChildren<SpriteRenderer>();
        renderer.sprite = GlobalGameManager.Instance.GetClassTexture(AllyCharacter.CharacterClass);
    }
}
