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
[SerializeField] private float _maxHealth;
    [SerializeField] private float _healthPoints;
    [SerializeField] private float _damage;                  //amount of damages dealt
    [SerializeField] private float _accuracy;                //value between 0 and 1
    [SerializeField] protected float _dodge;                 // value between 0 and 1. Probability of successful attack is accuracy-dodge
    [SerializeField] private float _movementPoints;          // how far can a charcater move in one turn
    [SerializeField] private float _weigth;                  //can be a condition for some actions
    [SerializeField] private float _critChances;
    [SerializeField] private float _rangeShot;
    [SerializeField] private EnumGender _gender;
    [SerializeField] private string _name;

    

public delegate void DieEvent();
    public event DieEvent OnDeath;

    //constructor 
    public Character()  //random values
    {
        _maxHealth = Random.Range(17,23);
        _healthPoints = _maxHealth;
        _damage = Random.Range(2,5);
        _accuracy = Random.Range(55,75);
        _dodge = Random.Range(10, 20);
        _critChances = Random.Range(10, 20);
        _movementPoints = 20;
        _weigth = Random.Range(50,110);
        _rangeShot = Random.Range(17,25);
        _gender = (EnumGender) Random.Range(0, 3);
        switch (_gender)
        {

            case EnumGender.Female:
                _name = _femaleNames[Random.Range(0, _femaleNames.Length)];
                break;
            case EnumGender.Male:
                _name = _maleNames[Random.Range(0, _maleNames.Length)];
                break;
            default:
                int coin = Random.Range(0, 2);
                if (coin == 0)
                    _name = _femaleNames[Random.Range(0, _femaleNames.Length)];
                else
                    _name = _maleNames[Random.Range(0, _maleNames.Length)];
                break;
        }
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
