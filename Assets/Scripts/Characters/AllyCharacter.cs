using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllyCharacter : Character
{
    private static Dictionary<EnumClasses, List<Trait>> s_mandatoryTraits = new Dictionary<EnumClasses, List<Trait>>(){
        {EnumClasses.Berserker, new List<Trait> { } },//EnumTraits.Stocky}},
        {EnumClasses.Engineer, new List<Trait> {} },//EnumTraits.Stocky}},
        {EnumClasses.Hitman, new List<Trait> {} },//EnumTraits.Racist}},
        {EnumClasses.Sniper, new List<Trait> {} },//EnumTraits.Racist}},
        {EnumClasses.HoundMaster, new List<Trait> {} },//EnumTraits.Ugly}},
        {EnumClasses.Smuggler, new List<Trait> {} },//EnumTraits.Ugly}}
    };

    private static Dictionary<EnumClasses, List<Trait>> s_commonPossibleTraits = new Dictionary<EnumClasses, List<Trait>>(){
        {EnumClasses.Berserker, new List<Trait> {} },//EnumTraits.Fearful,EnumTraits.Brave}},
        {EnumClasses.Engineer, new List<Trait> {} },//EnumTraits.Fearful,EnumTraits.Brave}},
        {EnumClasses.Hitman, new List<Trait> {} },//EnumTraits.Lovely, EnumTraits.Sprinter}},
        {EnumClasses.Sniper, new List<Trait> {} },//EnumTraits.Lovely,EnumTraits.Sprinter}},
        {EnumClasses.HoundMaster, new List<Trait> {} },//EnumTraits.Brave,EnumTraits.Sprinter}},
        {EnumClasses.Smuggler, new List<Trait> {} },//EnumTraits.Brave,EnumTraits.Sprinter}}
    };

    //Character's archetype
    private EnumClasses _class;
    private List<Trait> _traits = new List<Trait>();
    public List<Trait> Traits
    {
        get { return _traits; }
    }

    private Dictionary<AllyCharacter, Relationship> _relationships;
    public Dictionary<AllyCharacter, Relationship> Relationships { get { return _relationships; } }

    public AllyCharacter(EnumClasses characterClass, float maxHealth, float damage, float accuracy, float dodge, float critChances, float rangeShot, float movementPoints, float weight) :
        base(maxHealth, damage, accuracy, dodge, critChances, rangeShot, movementPoints, weight)
    {
        _class = characterClass;
        addMandatoryTraits(_class);
        addRandomTrait(_class);
    }

    public void InitializeRelationships()
    {
        _relationships = new Dictionary<AllyCharacter, Relationship>();
        foreach (AllyUnit ally in CombatGameManager.Instance.AllAllyUnits)
        {
            if (ally.Character != this)
            {
                _relationships.Add(ally.AllyCharacter, new Relationship(this, ally.AllyCharacter));
            }
        }
    }

    private Trait GetRandomTraitsFromClass(EnumClasses characterClass)
    {

        int indice = Random.Range(0, s_mandatoryTraits[characterClass].Count);
        Trait newTrait = s_mandatoryTraits[characterClass][indice];
        return newTrait;
    }

    private void addMandatoryTraits(EnumClasses characterClass)
    {
        for (int i = 0; i < s_mandatoryTraits[characterClass].Count; i++)
        {
            _traits.Add(s_mandatoryTraits[characterClass][i]);
        }
    }

    private void addRandomTrait(EnumClasses characterClass)
    {
        _traits.Add(GetRandomTraitsFromClass(characterClass));
    }

    public override float GetDodge(EnumCover cover)
    {
        float dodge = _dodge;
        switch (cover)
        {
            case EnumCover.Full:
                if (HaveTrait(new Tall())) dodge += _halfCoverDodgeBonus;
                else dodge += _fullCoverDodgeBonus;
                break;
            case EnumCover.Half:
                if (HaveTrait(new Stubby())) dodge += _fullCoverDodgeBonus;
                dodge += _halfCoverDodgeBonus;
                break;
            default:
                break;
        }

        return dodge;
    }

    private bool HaveTrait(Trait traitToFind)
    {
        bool traitFound = false;
        foreach (Trait trait in _traits)
        {
            if (trait.GetType() == typeof(Trait) )
            {
                traitFound = true;
                break;
            }
        }

        return traitFound;
    }
}
