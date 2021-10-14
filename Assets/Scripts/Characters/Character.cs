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
    [SerializeField] private float _critChances;

    private static Dictionary<EnumClasses, List<EnumTraits>> s_mandatoryTraits = new Dictionary<EnumClasses, List<EnumTraits>>(){
        {EnumClasses.Berserker, new List<EnumTraits> {EnumTraits.Stocky}},
        {EnumClasses.Engineer, new List<EnumTraits> {EnumTraits.Stocky}},
        {EnumClasses.Hitman, new List<EnumTraits> {EnumTraits.Racist}},
        {EnumClasses.Sniper, new List<EnumTraits> {EnumTraits.Racist}},
        {EnumClasses.HoundMaster, new List<EnumTraits> {EnumTraits.Ugly}},
        {EnumClasses.Smuggler, new List<EnumTraits> {EnumTraits.Ugly}}
    };

    private static Dictionary<EnumClasses, List<EnumTraits>> s_commonPossibleTraits = new Dictionary<EnumClasses, List<EnumTraits>>(){
        {EnumClasses.Berserker, new List<EnumTraits> {EnumTraits.Fearful,EnumTraits.Brave}},
        {EnumClasses.Engineer, new List<EnumTraits> {EnumTraits.Fearful,EnumTraits.Brave}},
        {EnumClasses.Hitman, new List<EnumTraits> {EnumTraits.Lovely, EnumTraits.Sprinter}},
        {EnumClasses.Sniper, new List<EnumTraits> {EnumTraits.Lovely,EnumTraits.Sprinter}},
        {EnumClasses.HoundMaster, new List<EnumTraits> {EnumTraits.Brave,EnumTraits.Sprinter}},
        {EnumClasses.Smuggler, new List<EnumTraits> {EnumTraits.Brave,EnumTraits.Sprinter}}
    };

    //constructor 

    public Character()
    {
        addMandatoryTraits(_class);
        addRandomTrait(_class);
    }

    public Character(EnumClasses characterClass,float maxHealth,float damage, float accuracy, float dodge,float critChances, float movementPoints, float weight)
    {
        _class = characterClass;
        addMandatoryTraits(_class);
        addRandomTrait(_class);

        _maxHealth = maxHealth;
        _damage = damage;
        _accuracy = accuracy;
        _dodge = dodge;
        _critChances = critChances;
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

    public float Damage
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

    public float CritChances
    {
        get { return this._critChances; }
        private set { this._critChances = value; }
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

    private EnumTraits GetRandomTraitsFromClass(EnumClasses characterClass)
    {

        int indice = Random.Range(0,s_mandatoryTraits[characterClass].Count);
        EnumTraits newTrait = s_mandatoryTraits[characterClass][indice];
        return newTrait;
    }

    private void addMandatoryTraits(EnumClasses characterClass) 
    {
        for (int i = 0;i< s_mandatoryTraits[characterClass].Count;i++)
        {
            _traits.Add(s_mandatoryTraits[characterClass][i]);
        }
    }

    public void addRandomTrait(EnumClasses characterClass)
    {
        _traits.Add(GetRandomTraitsFromClass(characterClass));
    }
    public void TakeDamage(float damage)
    {
        _healthPoints -= damage;
        if (_healthPoints <= 0)
        {
            Die();
        }
    }

    public void Die()
    {

    }
}
