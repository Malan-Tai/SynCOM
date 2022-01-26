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

    public int Money { get; private set; }

    private Dictionary<RegionName, int> _controlStatus;

    [SerializeField]
    private Sprite[] _classSprites;
    [SerializeField]
    private Sprite[] _classPortraits;
    [SerializeField]
    private Sprite _deadAllySprite;

    [SerializeField]
    private Sprite _enemySprite;
    [SerializeField]
    private Sprite _enemyPortrait;
    [SerializeField]
    private Sprite _deadEnemySprite;

    private void GenerateCharacters()
    {
        allCharacters = new List<AllyCharacter>
        {
            AllyCharacter.GetRandomAllyCharacter(),
            AllyCharacter.GetRandomAllyCharacter(),
            AllyCharacter.GetRandomAllyCharacter(),
            AllyCharacter.GetRandomAllyCharacter(),
            AllyCharacter.GetRandomAllyCharacter(),
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

    public Sprite GetClassSprite(EnumClasses charClass)
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

    public Sprite GetDeadAllySprite()
    {
        return _deadAllySprite;
    }

    public Sprite GetEnemySprite()
    {
        return _enemySprite;
    }

    public Sprite GetEnemyPortrait()
    {
        return _enemyPortrait;
    }

    public Sprite GetDeadEnemySprite()
    {
        return _deadEnemySprite;
    }

    public void AddCharacter(AllyCharacter character)
    {
        allCharacters.Add(character);
        character.InitializeRelationships(allCharacters, true);
    }

    public void StartCurrentMission()
    {
        CombatGameManager.OnMissionEnd += OnCurrentMissionEnd;
    }

    private void OnCurrentMissionEnd(CombatGameManager.MissionEndEventArgs missionEndEventArgs)
    {
        if (missionEndEventArgs.Success)
        {
            Debug.Log(CurrentMission.moneyReward);
            Money += CurrentMission.moneyReward;
        }
        else
        {
            /// TODO Failure things
        }
    }
}
