using System;
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

            currentSquad = new AllyCharacter[] { null, null, null, null };
            GenerateCharacters();
        }
        else
        {
            Destroy(this);
        }
    }
    #endregion

    public List<AllyCharacter> allCharacters { get; private set; }
    [SerializeField]
    public AllyCharacter[] currentSquad { get; private set; }

    public Mission CurrentMission { get; set; }

    private int _money;

    private Dictionary<RegionName, int> _controlStatus;

    [SerializeField]
    private Sprite[] _classSprites;
    [SerializeField]
    private Sprite[] _classPortraits;

    private void GenerateCharacters()
    {
        allCharacters = new List<AllyCharacter>
        {
            new AllyCharacter(EnumClasses.Berserker, 20, 2, 65, 10, 15, 20, 4, 60),
            new AllyCharacter(EnumClasses.Engineer, 20, 2, 65, 10, 15, 20, 4, 60),
            new AllyCharacter(EnumClasses.Sniper, 20, 2, 65, 10, 15, 20, 4, 60),
            new AllyCharacter(EnumClasses.Hitman, 20, 2, 65, 10, 15, 20, 4, 60),
            new AllyCharacter(EnumClasses.Smuggler, 20, 2, 65, 10, 15, 20, 4, 60),
            new AllyCharacter(EnumClasses.HoundMaster, 20, 2, 65, 10, 15, 20, 4, 60),
            new AllyCharacter(EnumClasses.Berserker, 20, 2, 65, 10, 15, 20, 4, 60),
            new AllyCharacter(EnumClasses.Engineer, 20, 2, 65, 10, 15, 20, 4, 60),
            new AllyCharacter(EnumClasses.Sniper, 20, 2, 65, 10, 15, 20, 4, 60),
            new AllyCharacter(EnumClasses.Hitman, 20, 2, 65, 10, 15, 20, 4, 60),
            new AllyCharacter(EnumClasses.Smuggler, 20, 2, 65, 10, 15, 20, 4, 60),
            new AllyCharacter(EnumClasses.HoundMaster, 20, 2, 65, 10, 15, 20, 4, 60),
            new AllyCharacter(EnumClasses.Berserker, 20, 2, 65, 10, 15, 20, 4, 60),
            new AllyCharacter(EnumClasses.Engineer, 20, 2, 65, 10, 15, 20, 4, 60),
            new AllyCharacter(EnumClasses.Sniper, 20, 2, 65, 10, 15, 20, 4, 60),
            new AllyCharacter(EnumClasses.Hitman, 20, 2, 65, 10, 15, 20, 4, 60),
            new AllyCharacter(EnumClasses.Smuggler, 20, 2, 65, 10, 15, 20, 4, 60),
            new AllyCharacter(EnumClasses.HoundMaster, 20, 2, 65, 10, 15, 20, 4, 60),
        };

        foreach (AllyCharacter character in allCharacters)
        {
            character.InitializeRelationships(allCharacters);
        }
    }

    public void SetDefaultSquad()
    {
        int i = 0;
        while (i < currentSquad.Length && i < allCharacters.Count)
        {
            currentSquad[i] = allCharacters[i];
            i++;
        }

        while (i < currentSquad.Length)
        {
            currentSquad[i] = null;
        }
    }

    public void SetSquadUnit(int i, AllyCharacter character)
    {
        if (i >= currentSquad.Length) return;
        currentSquad[i] = character;
    }

    public Sprite GetClassTexture(EnumClasses charClass)
    {
        int i = (int)charClass;
        if (i < 0 || i >= _classSprites.Length) return null;
        return _classSprites[i];
    }

    public Sprite GetClassPortrait(EnumClasses charClass)
    {
        int i = (int)charClass;
        if (i < 0 || i >= _classPortraits.Length) return null;
        return _classPortraits[i];
    }
}
