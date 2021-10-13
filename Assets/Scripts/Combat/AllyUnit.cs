using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllyUnit : GridBasedUnit
{
    private BaseAbility _currentAbility = null;
    public BaseAbility CurrentAbility { get { return _currentAbility; } }

    public AllyCharacter AllyCharacter { get => (AllyCharacter)_character; }

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
        _currentAbility = ability;
        ability.SetEffector(this);
        ability.OnAbilityEnded += StopUsingAbility;
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
}
