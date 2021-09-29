using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    //Character's archetype

    [SerializeField] private EnumClasses _class;

    //Character's statistics

    [SerializeField] private float _maxHealth;
    [SerializeField] private float _healthPoints;
    [SerializeField] private float _damage;    //amount of damages dealt
    [SerializeField] private float _accuracy;  //value between 0 and 1
    [SerializeField] private float _dodge;     // value between 0 and 1. Probability of successful attack is accuracy-dodge
    [SerializeField] private float _movementPoints; // how far can a charcater move in one turn
    [SerializeField] private float _weigth; //can be a condition for some actions

    Dictionary<Character, Relationship> _relationships;

    //Getters and setters
    public float MaxHealth
    {
        get { return this._maxHealth; }
        private set { this._maxHealth = value; }
    }
    public float HealthPoints
    {
        get { return this._healthPoints; }
        private set { this._healthPoints= value; }
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
        get { return this._movementPoints ; }
        private set { this._movementPoints = value; }
    }

    public float Weigth
    {
        get { return this._weigth; }
        private set { this._weigth = value; }
    }

    void Start()    
    {
        
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

    // Update is called once per frame
    void Update()
    {
        
    }
}
