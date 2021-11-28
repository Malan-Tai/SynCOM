using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// TODO replace Text elements with TextMeshPro Text element
public class MissionEndCanvas : MonoBehaviour
{
    [SerializeField] private GameObject _missionSuccessGO;
    [SerializeField] private GameObject _missionFailureGO;

    private void Start()
    {
        _missionSuccessGO.SetActive(false);
        _missionFailureGO.SetActive(false);

        CombatGameManager.OnMissionEnd += OnMissionEnd;
    }

    public void OnMissionEnd(CombatGameManager.MissionEndEventArgs missionEndEventArgs)
    {
        if (missionEndEventArgs.Success)
        {
            _missionSuccessGO.SetActive(true);
        }
        else
        {
            _missionFailureGO.SetActive(true);
        }
    }

    public void FinishMission()
    {
        SceneManager.LoadScene(0);
    }
}
