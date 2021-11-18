using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private Dictionary<RegionName, int> _control;

    private void Start()
    {
        _control = new Dictionary<RegionName, int>
        {
            { RegionName.Bronx,     0 },
            { RegionName.Brooklyn,  0 },
            { RegionName.Manhattan, 0 },
            { RegionName.Queens,    0 },
            { RegionName.Richmond,  0 }
        };

        _availableMissions = new Dictionary<RegionName, Mission>
        {
            { RegionName.Bronx,     Mission.None },
            { RegionName.Brooklyn,  Mission.None },
            { RegionName.Manhattan, Mission.None },
            { RegionName.Queens,    Mission.None },
            { RegionName.Richmond,  Mission.None }
        };

        GenerateMissions(0, 2);
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

        int N = Enum.GetNames(typeof(RegionName)).Length;

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

    public int GetRegionControl(RegionName region)
    {
        return _control[region];
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
}
