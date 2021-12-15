using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class AllyCharacter : Character
{
    private static Dictionary<EnumClasses, List<Trait>> s_mandatoryTraits = new Dictionary<EnumClasses, List<Trait>>(){
        {EnumClasses.Berserker, new List<Trait> {new Brave()}},
        {EnumClasses.Engineer, new List<Trait> {new Brave()}},
        {EnumClasses.Hitman, new List<Trait> {new Handsome(),new Contemptuous()}},
        {EnumClasses.Sniper, new List<Trait> {new Handsome(), new Contemptuous()}},
        {EnumClasses.Bodyguard, new List<Trait> {new Ugly(), new Fearless()}},
        {EnumClasses.Smuggler, new List<Trait> {new Ugly(), new Fearless()}}
    };

    private static Dictionary<EnumClasses, List<Trait>> s_commonPossibleTraits = new Dictionary<EnumClasses, List<Trait>>(){
        {EnumClasses.Berserker, new List<Trait> {new Ugly(),new Fearful(),new Cold(), new Antisocial()}},
        {EnumClasses.Engineer, new List<Trait> {new Ugly(),new Fearful(),new Cold(), new Antisocial()}},
        {EnumClasses.Hitman, new List<Trait> {new Brave(), new Nice() ,new Fearful(),new Cold()}},
        {EnumClasses.Sniper, new List<Trait> {new Brave(),new Nice(),new Fearful(),new Cold()}},
        {EnumClasses.Bodyguard, new List<Trait> {new Antisocial(), new Brave(), new Cold(), new Sensitive()}},
        {EnumClasses.Smuggler, new List<Trait> {new Antisocial(), new Brave(), new Cold(), new Sensitive()}}
    };

    //Character's archetype
    private EnumClasses _class;
    public EnumClasses CharacterClass { get => _class; }

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

        Debug.Log(_traits.Count);

        for (int i = 0; i < _traits.Count; i++)
        {
            Debug.Log(_traits[i].GetName());
        }        
    }

    public void InitializeRelationships()
    {
        // TODO : deprecated soon

        _relationships = new Dictionary<AllyCharacter, Relationship>();
        foreach (AllyUnit ally in CombatGameManager.Instance.AllAllyUnits)
        {
            if (ally.Character != this)
            {
                _relationships.Add(ally.AllyCharacter, new Relationship(this, ally.AllyCharacter));
            }
        }
    }

    public void InitializeRelationships(List<AllyCharacter> characters)
    {
        _relationships = new Dictionary<AllyCharacter, Relationship>();
        foreach (AllyCharacter ally in characters)
        {
            if (ally != this)
            {
                _relationships.Add(ally, new Relationship(this, ally));
            }
        }
    }

    private void addMandatoryTraits(EnumClasses characterClass)
    {
        for (int i = 0; i < s_mandatoryTraits[characterClass].Count; i++)
        {
            _traits.Add(s_mandatoryTraits[characterClass][i].GetClone(this));
        }
    }

    private void addRandomTrait(EnumClasses characterClass)
    {
        int indice = Random.Range(0, s_commonPossibleTraits[characterClass].Count);
        _traits.Add(s_commonPossibleTraits[characterClass][indice].GetClone(this));
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

    public bool HaveTrait(Trait traitToFind)
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
