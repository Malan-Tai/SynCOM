using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Character
{
    protected const float _fullCoverDodgeBonus = 40f;
    protected const float _halfCoverDodgeBonus = 20f;
    private static string[] _maleNames = new string[] { "Carlo 'Roulette' Palagi",
                        "Jamel 'The Beast' Musacchio",
                        "Geronzio 'The Hulk' Puzzo",
                        "Igino 'The Duke' Cunico",
                        "Ruben 'The Boot' Capurro",
                        "Albert 'The Duke' Wear",
                        "Speranzio 'The Undertaker' Shackley",
                        "Clifton 'Crazy Eyes' Carville",
                        "Gaven 'The Beast' Esmond",
                        "Paolo 'The Rat' Cocklin",
                        "Cornelio 'The Saint' Crea",
                        "Santos 'The Saint' Mora",
                        "Geminiano 'Deaf' Accetta",
                        "Vladimiro 'The Humpback' Antonini",
                        "Quinn 'Bullettooth' Nigrelli",
                        "Vilfredo 'Smokes' Smiles",
                        "Alamanno 'One Armed' Lunt",
                        "Caio 'Action Jackson' Parcel",
                        "Jeremy 'The Owl' Jan",
                        "Emilio 'The Shadow' Brining"
    };
    private static string[] _femaleNames = new string[] {
            "Estrella 'The Dapper' Cambio",
            "Lorena 'Queen Bee' Martorelli",
            "Zelinda 'Machine Gun' Cicco",
            "Loretta 'Coughing' Guglielmetti",
            "Madyson 'The Fat' Mastrandrea",
            "Adrienne 'Toughness' Gorton",
            "Caitlyn 'The Wild' Jordison",
            "Kaitlyn 'Poison' Popple",
            "Lola 'The Brain' Swepston",
            "Kayleigh 'Iceman' Riches",
            "Allyson 'The Quiet' Bassano",
            "Pelagia 'Razor' Patruno",
            "Tristan 'The Dwarf' Zummo",
            "Tori 'The Trigger' Picardi",
            "Monica 'Iron' Meglio",
            "Eliana 'Angel Wings' Dudgeon",
            "Lyric 'One Eye' Sheriff",
            "Tessa 'Crazy' Colver",
            "Lillie 'The Bull' Paternoster",
            "Kasandra 'The Clown' Bolter",
    };

    //Character's statistics
    [SerializeField] protected float _maxHealth;
    [SerializeField] protected float _healthPoints;
    [SerializeField] protected float _damage;                  //amount of damages dealt
    [SerializeField] protected float _accuracy;                //value between 0 and 1
    [SerializeField] protected float _dodge;                 // value between 0 and 1. Probability of successful attack is accuracy-dodge
    [SerializeField] protected float _movementPoints;          // how far can a charcater move in one turn
    [SerializeField] protected float _weigth;                  //can be a condition for some actions
    [SerializeField] protected float _critChances;
    [SerializeField] protected float _rangeShot;
    [SerializeField] private EnumGender _gender;
    [SerializeField] private string _name;

    public delegate void DieEvent();
    public event DieEvent OnDeath;

    private List<Buff> _currentBuffs = new List<Buff>();
    public List<Buff> CurrentBuffs
    {
        get { return _currentBuffs; }
    }

    public List<BaseAbility> Abilities { get; private set; }

    //constructor 
    public static Character GetRandomCharacter(Character preinstanciated = null)  //random values
    {
        if (preinstanciated == null) preinstanciated = new Character(6, 2, 65, 10, 15, 20, 0, 60);

        // keep range shot and movement points as is
        preinstanciated._maxHealth      += Random.Range(-10, 11);
        preinstanciated._damage         += Random.Range(-2, 3);
        preinstanciated._accuracy       += Random.Range(-10, 11);
        preinstanciated._dodge          += Random.Range(-5, 6);
        preinstanciated._critChances    += Random.Range(-5, 6);
        preinstanciated._weigth         += Random.Range(-25, 26);
        preinstanciated._gender         = (EnumGender) Random.Range(0, 3);
        preinstanciated._healthPoints = preinstanciated._maxHealth;
        switch (preinstanciated._gender)
        {

            case EnumGender.Female:
                preinstanciated._name = _femaleNames[Random.Range(0, _femaleNames.Length)];
                break;
            case EnumGender.Male:
                preinstanciated._name = _maleNames[Random.Range(0, _maleNames.Length)];
                break;
            default:
                int coin = Random.Range(0, 2);
                if (coin == 0)
                    preinstanciated._name = _femaleNames[Random.Range(0, _femaleNames.Length)];
                else
                    preinstanciated._name = _maleNames[Random.Range(0, _maleNames.Length)];
                break;
        }

        return preinstanciated;
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

    public float Dodge
    {
        get { return this._dodge; }
        set { this._dodge = value; }
    }

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

    public string Name
    {
        get { return this._name; }  
        set { this._name = value; }
    }

    public bool TakeDamage(float damage)
    {
        Debug.Log("oof, took " + damage + " dmg");
        _healthPoints -= damage;
        if (_healthPoints <= 0)
        {
            Die();
            return true;
        }
        return false;
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
