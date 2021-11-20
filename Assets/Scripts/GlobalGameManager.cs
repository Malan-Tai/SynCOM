using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalGameManager : MonoBehaviour
{
    #region Singleton
    private static GlobalGameManager instance;
    public static GlobalGameManager Instance { get { return instance; } }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            GenerateCharacters();
        }
        else
        {
            Destroy(this);
        }
    }
    #endregion

    public List<AllyCharacter> allCharacters { get; private set; }

    private int _money;

    private Dictionary<RegionName, int> _controlStatus;

    private void GenerateCharacters()
    {
        allCharacters = new List<AllyCharacter>
        {
            new AllyCharacter(EnumClasses.Sniper, 20, 2, 65, 10, 15, 20, 4, 60),
            new AllyCharacter(EnumClasses.Sniper, 20, 2, 65, 10, 15, 20, 4, 60),
            new AllyCharacter(EnumClasses.Sniper, 20, 2, 65, 10, 15, 20, 4, 60),
            new AllyCharacter(EnumClasses.Sniper, 20, 2, 65, 10, 15, 20, 4, 60),
            new AllyCharacter(EnumClasses.Sniper, 20, 2, 65, 10, 15, 20, 4, 60),
            new AllyCharacter(EnumClasses.Sniper, 20, 2, 65, 10, 15, 20, 4, 60),
            new AllyCharacter(EnumClasses.Sniper, 20, 2, 65, 10, 15, 20, 4, 60),
            new AllyCharacter(EnumClasses.Sniper, 20, 2, 65, 10, 15, 20, 4, 60),
            new AllyCharacter(EnumClasses.Sniper, 20, 2, 65, 10, 15, 20, 4, 60),
            new AllyCharacter(EnumClasses.Sniper, 20, 2, 65, 10, 15, 20, 4, 60),
            new AllyCharacter(EnumClasses.Sniper, 20, 2, 65, 10, 15, 20, 4, 60),
            new AllyCharacter(EnumClasses.Sniper, 20, 2, 65, 10, 15, 20, 4, 60),
            new AllyCharacter(EnumClasses.Sniper, 20, 2, 65, 10, 15, 20, 4, 60),
            new AllyCharacter(EnumClasses.Sniper, 20, 2, 65, 10, 15, 20, 4, 60),
            new AllyCharacter(EnumClasses.Sniper, 20, 2, 65, 10, 15, 20, 4, 60),
            new AllyCharacter(EnumClasses.Sniper, 20, 2, 65, 10, 15, 20, 4, 60),
            new AllyCharacter(EnumClasses.Sniper, 20, 2, 65, 10, 15, 20, 4, 60),
            new AllyCharacter(EnumClasses.Sniper, 20, 2, 65, 10, 15, 20, 4, 60),
            new AllyCharacter(EnumClasses.Sniper, 20, 2, 65, 10, 15, 20, 4, 60),
            new AllyCharacter(EnumClasses.Sniper, 20, 2, 65, 10, 15, 20, 4, 60),
        };
    }
}
