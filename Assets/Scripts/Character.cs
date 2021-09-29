using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    //Character's archetype

    [SerializeField] private EnumClasses _class;
    
    //Character's statistics

    [SerializeField] private float _healthPoints;
    [SerializeField] private float _damages;
    [SerializeField] private float _accuracy;
    [SerializeField] private float _dodge;
    [SerializeField] private int _movementPoints;
    [SerializeField] private int _weigth;

    Dictionary<Character, Relationship> _relationships;
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
