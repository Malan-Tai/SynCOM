using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllyCharacter : Character
{
    private Dictionary<AllyCharacter, Relationship> _relationships;
    public Dictionary<AllyCharacter, Relationship> Relationships { get { return _relationships; } }

    public AllyCharacter(EnumClasses characterClass, float maxHealth, float damage, float accuracy, float dodge, float critChances, float movementPoints, float weight) :
        base(characterClass, maxHealth, damage, accuracy, dodge, critChances, movementPoints, weight)
    {

    }

    public void InitializeRelationships()
    {
        _relationships = new Dictionary<AllyCharacter, Relationship>();
        foreach (AllyUnit ally in CombatGameManager.Instance.AllAllyUnits)
        {
            if (ally.Character != this)
            {
                _relationships.Add(ally.AllyCharacter, new Relationship());
            }
        }
    }
}
