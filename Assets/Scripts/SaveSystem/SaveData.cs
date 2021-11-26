using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class SaveData
{
    //here is where data that needs to be saved is stored
    
    [SerializeField]
    public List<AllyCharacter> characters;
    public AllyCharacter[] allyCharacters;
    public int money;
    public Dictionary<RegionName, int> _controlStatus;


}
