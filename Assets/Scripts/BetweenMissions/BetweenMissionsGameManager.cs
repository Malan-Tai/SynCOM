using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BetweenMissionsGameManager : MonoBehaviour
{
    #region Singleton
    private static BetweenMissionsGameManager instance;
    public static BetweenMissionsGameManager Instance { get { return instance; } }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
    #endregion

    [SerializeField]
    private List<MissionTypeScriptableObject> _missionTypes;

    private List<AllyCharacter> _newRecruits;

    private Dictionary<RegionName, Mission> _availableMissions;

    private RegionName _selectedRegion;

    [SerializeField]
    private UnitScrollList _missionUnitList;

    [SerializeField]
    private Transform _missionRecapUnits;

    [SerializeField]
    private CharacterSheet _missionCharacterSheet;

    [SerializeField]
    private UnitScrollList _recruitUnitList;

    [SerializeField]
    private CharacterSheet _recruitCharacterSheet;

    [SerializeField]
    private string _combatSceneName = "SampleScene";

    private int _selectedSquadUnit;

    public delegate void NotifyCanvasChange(int x, int y);
    public static event NotifyCanvasChange OnNotifyCanvasChange;

    private void Start()
    {
        _selectedRegion = RegionName.None;

        _availableMissions = new Dictionary<RegionName, Mission>
        {
            { RegionName.Bronx,     Mission.None },
            { RegionName.Brooklyn,  Mission.None },
            { RegionName.Manhattan, Mission.None },
            { RegionName.Queens,    Mission.None },
            { RegionName.Richmond,  Mission.None }
        };

        GenerateMissions(0, 2);

        _missionUnitList.Populate(GlobalGameManager.Instance.allCharacters);

        GlobalGameManager.Instance.SetDefaultSquad();
        InitMissionRecapUnits();

        _missionCharacterSheet.InitEventsWithScrollList(_missionUnitList);

        _newRecruits = new List<AllyCharacter>
        {
            AllyCharacter.GetRandomAllyCharacter(),
            AllyCharacter.GetRandomAllyCharacter(),
            AllyCharacter.GetRandomAllyCharacter()
        };

        _recruitUnitList.Populate(_newRecruits);
        _recruitCharacterSheet.InitEventsWithScrollList(_recruitUnitList, true);
    }

    public void InitMissionRecapUnits()
    {
        int i = 0;
        foreach (MissionRecapUnit unit in _missionRecapUnits.GetComponentsInChildren<MissionRecapUnit>())
        {
            unit.Init();
            unit.SetIndex(i);
            unit.SetCharacter(GlobalGameManager.Instance.currentSquad[i]);
            i++;
        }
    }

    private void OnEnable()
    {
        RegionButton.OnMouseClickEvent += SetSelectedRegion;
        MissionRecapUnit.OnMouseClickEvent += SetSelectedSquadUnitAndUpdateFrozenUnitsInList;

        _missionUnitList.OnMouseClickEvent += ChooseSquadUnit;
    }

    private void OnDisable()
    {
        RegionButton.OnMouseClickEvent -= SetSelectedRegion;
        MissionRecapUnit.OnMouseClickEvent -= SetSelectedSquadUnitAndUpdateFrozenUnitsInList;

        _missionUnitList.OnMouseClickEvent -= ChooseSquadUnit;
    }

    public void GenerateMissions(int progress, int missionNumber)
    {
        _availableMissions = new Dictionary<RegionName, Mission>
        {
            { RegionName.Bronx,     Mission.None },
            { RegionName.Brooklyn,  Mission.None },
            { RegionName.Manhattan, Mission.None },
            { RegionName.Queens,    Mission.None },
            { RegionName.Richmond,  Mission.None }
        };

        int N = Enum.GetNames(typeof(RegionName)).Length - 1;

        List<RegionName> addedRegions = new List<RegionName>();
        while (addedRegions.Count < missionNumber)
        {
            RegionName region;
            while (addedRegions.Contains(region = (RegionName)RandomEngine.Instance.Range(0, N)))
            { }

            _availableMissions[region] = Mission.GenerateMission(progress - 2, progress + 3);
            addedRegions.Add(region);
        }
    }

    public MissionTypeScriptableObject GetMissionType(WinCondition winCondition)
    {
        foreach (MissionTypeScriptableObject data in _missionTypes)
        {
            if (data.winCondition == winCondition) return data;
        }

        return null;
    }

    public Mission GetMissionInRegion(RegionName region)
    {
        return _availableMissions[region];
    }

    public void StartMission()
    {
        if (_selectedRegion == RegionName.None) return;

        print("start mission in " + _selectedRegion);
        GlobalGameManager.Instance.CurrentMission = _availableMissions[_selectedRegion];

        SceneManager.LoadScene(_combatSceneName);
    }

    public void ClearSquad()
    {
        int n = GlobalGameManager.Instance.currentSquad.Length;
        for (int i = 0; i < n; i++)
        {
            GlobalGameManager.Instance.SetSquadUnit(i, null);
        }
    }

    private void SetSelectedRegion(RegionScriptableObject region)
    {
        _selectedRegion = region.regionName;
    }

    private void SetSelectedSquadUnitAndUpdateFrozenUnitsInList(int squadIndex)
    {
        _selectedSquadUnit = squadIndex;
        ChooseSquadUnit(null);
        _missionUnitList.FreezeCharacters(GlobalGameManager.Instance.currentSquad);
    }

    private void ChooseSquadUnit(AllyCharacter character)
    {
        GlobalGameManager.Instance.SetSquadUnit(_selectedSquadUnit, character);
        _missionRecapUnits.GetComponentsInChildren<MissionRecapUnit>()[_selectedSquadUnit].SetCharacter(character);

        //if (OnNotifyCanvasChange != null) OnNotifyCanvasChange(0, -1);
    }

    public void GoToRecruitCanvas()
    {
        if (OnNotifyCanvasChange != null) OnNotifyCanvasChange(-1, 0);
    }

    public void GoFromRecruitToMapCanvas()
    {
        if (OnNotifyCanvasChange != null) OnNotifyCanvasChange(1, 0);
    }

    public void RecruitUnit(AllyCharacter rookie)
    {
        if (_newRecruits.Remove(rookie))
        {
            GlobalGameManager.Instance.AddCharacter(rookie);

            _missionUnitList.Populate(GlobalGameManager.Instance.allCharacters);
            _recruitUnitList.Populate(_newRecruits);
        }
    }
}
