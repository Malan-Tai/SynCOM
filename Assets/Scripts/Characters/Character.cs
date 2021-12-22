using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Character
{
    protected const float _fullCoverDodgeBonus = 40f;
    protected const float _halfCoverDodgeBonus = 20f;
    
    //Character's statistics
    [SerializeField] private float _maxHealth;
    [SerializeField] private float _healthPoints;
    [SerializeField] private float _damage;                  //amount of damages dealt
    [SerializeField] private float _accuracy;                //value between 0 and 1
    [SerializeField] protected float _dodge;                 // value between 0 and 1. Probability of successful attack is accuracy-dodge
    [SerializeField] private float _movementPoints;          // how far can a charcater move in one turn
    [SerializeField] private float _weigth;                  //can be a condition for some actions
    [SerializeField] private float _critChances;
    [SerializeField] private float _rangeShot;
    [SerializeField] private float _name;

    public delegate void DieEvent();
    public event DieEvent OnDeath;

    //constructor 
    public Character()
    {
    }

    public Character(float maxHealth,float damage, float accuracy, float dodge,float critChances,float rangeShot, float movementPoints, float weight)
    {
        _maxHealth = maxHealth;
        _healthPoints = maxHealth;
        _damage = damage;
        _accuracy = accuracy;
        _dodge = dodge;
        _critChances = critChances;
        _movementPoints = movementPoints;
        _weigth = weight;
        _rangeShot = rangeShot;

    }


    //Getters and setters
    public float MaxHealth
    {
        get { return this._maxHealth; }
        set { this._maxHealth = value; }
    }

    public float HealthPoints
    {
        get { return this._healthPoints; }
        set { this._healthPoints = value; }
    }

    public bool IsAlive
    {
        get => _healthPoints > 0;
    }

    public float Damage
    {
        get { return this._damage; }
        set { this._damage = value; }
    }

    public float Accuracy
    {
        get { return this._accuracy; }
        set { this._accuracy = value; }
    }

    //public float Dodge
    //{
    //    get { return this._dodge; }
    //    private set { this._dodge = value; }
    //}

    public virtual float GetDodge(EnumCover cover)
    {
        float dodge = _dodge;
        switch (cover)
        {
            case EnumCover.Full:
                dodge += _fullCoverDodgeBonus;
                break;
            case EnumCover.Half:
                dodge += _halfCoverDodgeBonus;
                break;
            default:
                break;
        }

        return dodge;
    }

    public float CritChances
    {
        get { return this._critChances; }
        set { this._critChances = value; }
    }

    public float MovementPoints
    {
        get { return this._movementPoints; }
        set { this._movementPoints = value; }
    }

    public float Weigth
    {
        get { return this._weigth; }
        set { this._weigth = value; }
    }

    public float RangeShot
    {
        get { return this._rangeShot; }
        set { this._rangeShot = value; }
    }

    public void TakeDamage(float damage)
    {
        Debug.Log("oof, took " + damage + " dmg");
        _healthPoints -= damage;
        if (_healthPoints <= 0)
        {
            Die();
        }
    }

    public void Kill()
    {
        _healthPoints = 0;
        Die();
    }

    public void Heal(float heal)
    {
        Debug.Log("mmh, recovered " + heal + " HP");
        _healthPoints = Mathf.Min(_maxHealth, _healthPoints + heal);
        if (_healthPoints == _maxHealth)
        {
            Debug.Log("Health full");
        }
    }

    public void Die()
    {
        if (OnDeath != null) OnDeath();
    }

    public virtual Sprite GetSprite()
    {
        return null;
    }

    public virtual Sprite GetPortrait()
    {
        return null;
    }
}
