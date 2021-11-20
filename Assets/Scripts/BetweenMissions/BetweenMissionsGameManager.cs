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
        int i = 0;
        foreach (MissionRecapUnit unit in _missionRecapUnits.GetComponentsInChildren<MissionRecapUnit>())
        {
            unit.SetCharacter(i, GlobalGameManager.Instance.currentSquad[i]);
            i++;
        }
    }

    private void OnEnable()
    {
        RegionButton.OnMouseClickEvent += SetSelectedRegion;
    }

    private void OnDisable()
    {
        RegionButton.OnMouseClickEvent -= SetSelectedRegion;
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
            while (addedRegions.Contains(region = (RegionName)UnityEngine.Random.Range(0, N)))
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

        SceneManager.LoadScene("SampleScene");
    }

    public void SetSelectedRegion(RegionScriptableObject region)
    {
        _selectedRegion = region.regionName;
    }
}
