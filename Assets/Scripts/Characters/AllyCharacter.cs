using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AllyCharacter : Character
{
    private static int _commonAbilitiesCount = 5;

    private static Dictionary<EnumClasses, List<Trait>> s_mandatoryTraits = new Dictionary<EnumClasses, List<Trait>>(){
        {EnumClasses.Berserker, new List<Trait> {new Brave()}},
        {EnumClasses.Engineer, new List<Trait> {new Brave()}},
        {EnumClasses.Alchemist, new List<Trait> {new Handsome(),new Contemptuous()}},
        {EnumClasses.Sniper, new List<Trait> {new Handsome(), new Contemptuous(), new Lucky()}},
        {EnumClasses.Bodyguard, new List<Trait> {new Ugly(), new Fearless()}},
        {EnumClasses.Smuggler, new List<Trait> {new Ugly(), new Fearless()}}
    };

    private static Dictionary<EnumClasses, List<Trait>> s_commonPossibleTraits = new Dictionary<EnumClasses, List<Trait>>(){
        {EnumClasses.Berserker, new List<Trait> {new Ugly(),new Fearful(),new Cold(), new Antisocial()}},
        {EnumClasses.Engineer, new List<Trait> {new Ugly(),new Fearful(),new Cold(), new Antisocial()}},
        {EnumClasses.Alchemist, new List<Trait> {new Brave(), new Nice() ,new Fearful(),new Cold()}},
        {EnumClasses.Sniper, new List<Trait> {new Brave(),new Nice(),new Fearful(),new Cold()}},
        {EnumClasses.Bodyguard, new List<Trait> {new Antisocial(), new Brave(), new Cold(), new Sensitive()}},
        {EnumClasses.Smuggler, new List<Trait> {new Antisocial(), new Brave(), new Cold(), new Sensitive()}}
    };

    //Character's archetype
    private EnumClasses _class;
    public EnumClasses CharacterClass { get => _class; }

    public List<BaseAllyAbility> Abilities { get; protected set; }
    public List<BaseAllyAbility> SpecialAbilities
    {
        get
        {
            return Abilities.GetRange(_commonAbilitiesCount, Abilities.Count - _commonAbilitiesCount);
        }
    }

    private List<Trait> _traits = new List<Trait>();
    public List<Trait> Traits { get { return _traits; } }
   
    private Dictionary<AllyCharacter, Relationship> _relationships;
    public Dictionary<AllyCharacter, Relationship> Relationships { get { return _relationships; } }

    public AllyCharacter(EnumClasses characterClass, float maxHealth, float damage, float accuracy, float dodge, float critChances, float rangeShot, float movementPoints, float weight, bool addTraits = true) :
        base(maxHealth, damage, accuracy, dodge, critChances, rangeShot, movementPoints, weight)
    {
        _class = characterClass;
        if (addTraits)
        {
            AddMandatoryTraits(_class);
            AddRandomTrait(_class);
        }
    }

   public static AllyCharacter GetRandomAllyCharacter()
   {
        EnumClasses characterClass = (EnumClasses)Random.Range(0, 6);
        AllyCharacter instance = new AllyCharacter(characterClass, 0, 0, 0, 0, 0, 0, 0, 0, false);

        instance.Abilities = new List<BaseAllyAbility>
        {
            new SkipTurn(),
            new BasicShot(),
            new HunkerDown(),
            new BasicDuoShot(),
            new FirstAid(),
            new PepTalk()
        };

        _commonAbilitiesCount = instance.Abilities.Count;

        switch (characterClass)
        {
            case EnumClasses.Berserker:
                instance._maxHealth         = 30;
                instance._damage            = 7;
                instance._accuracy          = 75;
                instance._dodge             = 10;
                instance._critChances       = 15;
                instance._rangeShot         = 3;
                instance._movementPoints    = 15;
                instance._weigth            = 90;

                instance.Abilities.Add(new Devouring());
                instance.Abilities.Add(new DwarfTossing());
                break;
            case EnumClasses.Engineer:
                instance._maxHealth         = 25;
                instance._damage            = 3;
                instance._accuracy          = 60;
                instance._dodge             = 10;
                instance._critChances       = 5;
                instance._rangeShot         = 20;
                instance._movementPoints    = 11;
                instance._weigth            = 90;

                instance.Abilities.Add(new GrenadeTossEngineer());
                instance.Abilities.Add(new Mortar());
                break;
            case EnumClasses.Sniper:
                instance._maxHealth         = 10;
                instance._damage            = 5;
                instance._accuracy          = 85;
                instance._dodge             = 10;
                instance._critChances       = 20;
                instance._rangeShot         = 30;
                instance._movementPoints    = 7;
                instance._weigth            = 65;

                instance.Abilities.Add(new SuppressiveFire());
                instance.Abilities.Add(new LongShot());
                break;
            case EnumClasses.Alchemist:
                instance._maxHealth         = 15;
                instance._damage            = 3;
                instance._accuracy          = 60;
                instance._dodge             = 20;
                instance._critChances       = 5;
                instance._rangeShot         = 20;
                instance._movementPoints    = 13;
                instance._weigth            = 65;

                instance.Abilities.Add(new HealingRain());
                break;
            case EnumClasses.Bodyguard:
                instance._maxHealth         = 40;
                instance._damage            = 4;
                instance._accuracy          = 55;
                instance._dodge             = 5;
                instance._critChances       = 5;
                instance._rangeShot         = 15;
                instance._movementPoints    = 11;
                instance._weigth            = 150;

                instance.Abilities.Add(new ShieldAndStrike());
                instance.Abilities.Add(new WildCharge());
                break;
            case EnumClasses.Smuggler:
                instance._maxHealth         = 35;
                instance._damage            = 5;
                instance._accuracy          = 65;
                instance._dodge             = 5;
                instance._critChances       = 10;
                instance._rangeShot         = 20;
                instance._movementPoints    = 12;
                instance._weigth            = 150;

                instance.Abilities.Add(new Smuggle());
                break;
        }

        instance = Character.GetRandomCharacter(instance) as AllyCharacter;

        instance.AddMandatoryTraits(characterClass);
        instance.AddRandomTrait(characterClass);

        //Debug.Log(instance._traits.Count);

        //for (int i = 0; i < instance._traits.Count; i++)
        //{
        //    Debug.Log(instance._traits[i].GetName());
        //}

        return instance;
   }

    public void InitializeRelationships(List<AllyCharacter> characters, bool alsoAddRelationshipToAllies = false)
    {
        _relationships = new Dictionary<AllyCharacter, Relationship>();
        foreach (AllyCharacter ally in characters)
        {
            if (ally != this)
            {
                _relationships.Add(ally, new Relationship(this, ally));

                if (alsoAddRelationshipToAllies) ally.InitializeRelationshipWithOneAlly(this);
            }
        }
    }

    private void InitializeRelationshipWithOneAlly(AllyCharacter ally)
    {
        _relationships.Add(ally, new Relationship(this, ally));
    }

    private void AddMandatoryTraits(EnumClasses characterClass)
    {
        for (int i = 0; i < s_mandatoryTraits[characterClass].Count; i++)
        {
            _traits.Add(s_mandatoryTraits[characterClass][i].GetClone(this));
        }
    }

    private void AddRandomTrait(EnumClasses characterClass)
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

    public override Sprite GetSprite()
    {
        return GlobalGameManager.Instance.GetClassSprite(_class);
    }

    public override Sprite GetPortrait()
    {
        return GlobalGameManager.Instance.GetClassPortrait(_class);
    }
}
