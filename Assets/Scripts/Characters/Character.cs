using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character
{
    //Character's archetype

    private EnumClasses _class;
    private List<EnumTraits> _traits = new List<EnumTraits>();

    //Character's statistics

    [SerializeField] private float _maxHealth;
    [SerializeField] private float _healthPoints;
    [SerializeField] private float _damage;    //amount of damages dealt
    [SerializeField] private float _accuracy;  //value between 0 and 1
    [SerializeField] private float _dodge;     // value between 0 and 1. Probability of successful attack is accuracy-dodge
    [SerializeField] private float _movementPoints; // how far can a charcater move in one turn
    [SerializeField] private float _weigth; //can be a condition for some actions

    Dictionary<Character, Relationship> _relationships = new Dictionary<Character, Relationship>();
    public Dictionary<Character, Relationship> Relationships { get { return _relationships; } }

    private static Dictionary<EnumClasses, List<EnumTraits>> s_mandatoryTraits = new Dictionary<EnumClasses, List<EnumTraits>>(){
        {EnumClasses.Berserker, new List<EnumTraits> {EnumTraits.Stocky}},
        {EnumClasses.Engineer, new List<EnumTraits> {EnumTraits.Stocky}},
        {EnumClasses.Hitman, new List<EnumTraits> {EnumTraits.Racist}},
        {EnumClasses.Sniper, new List<EnumTraits> {EnumTraits.Racist}},
        {EnumClasses.HoundMaster, new List<EnumTraits> {EnumTraits.Ugly}},
        {EnumClasses.Smuggler, new List<EnumTraits> {EnumTraits.Ugly}}
    };

    //constructor 

    public Character()
    {
        _traits.Add(GetRandomTraitsFromClass(_class));
    }

    public Character(EnumClasses characterClass,float maxHealth,float damage, float accuracy, float dodge, float movementPoints, float weight)
    {
        _class = characterClass;
        _traits.Add(GetRandomTraitsFromClass(characterClass));

        _maxHealth = maxHealth;
        _damage = damage;
        _accuracy = accuracy;
        _dodge = dodge;
        _movementPoints = movementPoints;
        _weigth = weight;

    }


    //Getters and setters
    public float MaxHealth
    {
        get { return this._maxHealth; }
        private set { this._maxHealth = value; }
    }
    public float HealthPoints
    {
        get { return this._healthPoints; }
        private set { this._healthPoints = value; }
    }

    public float Damages
    {
        get { return this._damage; }
        private set { this._damage = value; }
    }

    public float Accuracy
    {
        get { return this._accuracy; }
        private set { this._accuracy = value; }
    }

    public float Dodge
    {
        get { return this._dodge; }
        private set { this._dodge = value; }
    }

    public float MovementPoints
    {
        get { return this._movementPoints; }
        private set { this._movementPoints = value; }
    }

    public float Weigth
    {
        get { return this._weigth; }
        private set { this._weigth = value; }
    }

    public EnumTraits GetRandomTraitsFromClass(EnumClasses characterClass)
    {

        int indice = Random.Range(0,s_mandatoryTraits[characterClass].Count);
        EnumTraits newTrait = s_mandatoryTraits[characterClass][indice];
        return newTrait;
    }
    public void TakeDamages(float damages)
    {
        _healthPoints -= damages;
        if (_healthPoints <= 0)
        {
            Die();
        }
    }

    public void Die()
    {

    }

    public void InitializeRelationships()
    {
        foreach (AllyUnit ally in CombatGameManager.Instance.AllAllyUnits)
        {
            if (ally.Character != this)
            {
                _relationships.Add(ally.Character, new Relationship());
            }
        }
    }
}
