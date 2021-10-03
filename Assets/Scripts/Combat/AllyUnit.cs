using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllyUnit : GridBasedUnit
{
    [SerializeField] private Character _character;

    private BaseAbility _currentAbility = null;
    public BaseAbility CurrentAbility { get { return _currentAbility; } }

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
        ability.OnAbilityEnded += StopUsingAbility;
    }

    private void StopUsingAbility()
    {
        _currentAbility.OnAbilityEnded -= StopUsingAbility;
        _currentAbility = null;
    }
}
