using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RegionDescription : MonoBehaviour
{
    private TMP_Text _regionName;
    private TMP_Text _regionDescription;
    private TMP_Text _mission;
    private TMP_Text _missionDescription;

    private bool _eraseOnExit;
    private bool _setOnEnter;

    private void Start()
    {
        _regionName         = this.transform.Find("RegionName").GetComponent<TMP_Text>();
        _regionDescription  = this.transform.Find("RegionDescription").GetComponent<TMP_Text>();
        _mission            = this.transform.Find("Mission").GetComponent<TMP_Text>();
        _missionDescription = this.transform.Find("MissionDescription").GetComponent<TMP_Text>();

        _eraseOnExit = true;
        _setOnEnter = true;

        EraseDescription();
    }

    private void OnEnable()
    {
        RegionButton.OnMouseExitEvent += EraseDescription;
        RegionButton.OnMouseEnterEvent += SetDescription;
        RegionButton.OnMouseClickEvent += FreezeDescription;
        BackToMapButton.OnMouseClickEvent += UnfreezeDescription;
    }

    private void OnDisable()
    {
        RegionButton.OnMouseExitEvent -= EraseDescription;
        RegionButton.OnMouseEnterEvent -= SetDescription;
        RegionButton.OnMouseClickEvent -= FreezeDescription;
        BackToMapButton.OnMouseClickEvent -= UnfreezeDescription;
    }

    public void SetDescription(RegionScriptableObject data)
    {
        if (!_setOnEnter) return;

        _regionName.text = data.regionName.ToString();
        _regionDescription.text = data.description;

        Mission mission = BetweenMissionsGameManager.Instance.GetMissionInRegion(data.regionName);
        if (!mission.Equals(Mission.None))
        {
            _mission.text = mission.missionTypeData.winCondition.ToString();
            _missionDescription.text = mission.missionTypeData.description;
        }
    }

    public void EraseDescription()
    {
        if (!_eraseOnExit) return;

        _regionName.text = "";
        _regionDescription.text = "";
        _mission.text = "";
        _missionDescription.text = "";
    }

    public void FreezeDescription(RegionScriptableObject region)
    {
        _eraseOnExit = false;
        _setOnEnter = false;
    }

    public void UnfreezeDescription()
    {
        _eraseOnExit = true;
        _setOnEnter = true;

        EraseDescription();
    }
}
