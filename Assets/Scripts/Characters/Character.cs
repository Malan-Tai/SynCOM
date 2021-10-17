using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character
{
    //Character's statistics

    [SerializeField] private float _maxHealth;
    [SerializeField] private float _healthPoints;
    [SerializeField] private float _damage;    //amount of damages dealt
    [SerializeField] private float _accuracy;  //value between 0 and 1
    [SerializeField] private float _dodge;     // value between 0 and 1. Probability of successful attack is accuracy-dodge
    [SerializeField] private float _movementPoints; // how far can a charcater move in one turn
    [SerializeField] private float _weigth; //can be a condition for some actions
    [SerializeField] private float _critChances;

    //constructor 

    public Character()
    {
    }

    public Character(float maxHealth,float damage, float accuracy, float dodge,float critChances, float movementPoints, float weight)
    {
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
