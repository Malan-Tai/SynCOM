using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllyUnit : GridBasedUnit
{
    [SerializeField] private Character _character;

    public void UpdateEnemyVisibilities()
    {
        foreach (var pair in _linesOfSight)
        {
            var enemy = (EnemyUnit)pair.Key;
            enemy.UpdateVisibility(true, pair.Value.cover);
        }
    }
}
