using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalGameManager : MonoBehaviour
{
    #region Singleton
    private static GlobalGameManager _instance;
    public static GlobalGameManager Instance { get { return _instance; } }
    private bool _toNullify = true;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);

            currentSquad = new AllyCharacter[] { null, null, null, null };
            GenerateCharacters();

            InCombat = false;
        }
        else
        {
            _toNullify = false;
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (_toNullify) _instance = null;
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

    public bool InCombat { get; set; }

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

        for (int i = 0; i < allCharacters.Count; i++)
        {
            int j = RandomEngine.Instance.Range(0, allCharacters.Count - 1);
            j = j >= i ? j + 1 : j;
            RandomizeRelationship(i, j, 2);

            for (int k = 0; k < allCharacters.Count; k++)
            {
                if (k == i || k == j) continue;
                RandomizeRelationship(i, k);
            }
        }
    }

    private void RandomizeRelationship(int self, int ally, int level = 1)
    {
        Relationship relationship = allCharacters[self].Relationships[allCharacters[ally]];
        EnumSentiment sentiment = (EnumSentiment)RandomEngine.Instance.Range(0, 3);
        int sign = RandomEngine.Instance.Range(0, 2) == 0 ? -1 : 1;

        for (int i = 0; i < level; i++)
            relationship.IncreaseSentiment(sentiment, 100 * sign);
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
            i++;
        }
    }

    public void SetSquadUnit(int i, AllyCharacter character)
    {
        if (i >= currentSquad.Length) return;
        currentSquad[i] = character;
    }

    public Sprite GetClassSprite(EnumClasses charClass, EnumGender gender)
    {
        int g;
        switch (gender)
        {
            case EnumGender.Male:
                g = 0;
                break;
            case EnumGender.Female:
                g = 1;
                break;
            default:
                g = RandomEngine.Instance.Range(0, 2);
                break;
        }

        int i = 2 * (int)charClass + g;
        if (i < 0 || i >= _classSprites.Length) return null;
        return _classSprites[i];
    }

    public Sprite GetClassPortrait(EnumClasses charClass, EnumGender gender)
    {
        int g;
        switch (gender)
        {
            case EnumGender.Male:
                g = 0;
                break;
            case EnumGender.Female:
                g = 1;
                break;
            default:
                g = RandomEngine.Instance.Range(0, 2);
                break;
        }

        int i = 2 * (int)charClass + g;
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
        InCombat = true;
        CombatGameManager.OnMissionEnd += OnCurrentMissionEnd;
    }

    private void OnCurrentMissionEnd(CombatGameManager.MissionEndEventArgs missionEndEventArgs)
    {
        InCombat = false;
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
