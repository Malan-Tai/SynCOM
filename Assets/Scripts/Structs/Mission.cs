using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Mission
{
    public static Mission None;

    public int moneyReward;
    public AllyCharacter recruitReward;
    public int difficulty;
    public MissionTypeScriptableObject missionTypeData;

    public static Mission GenerateMission(int minDiff, int maxDiff)
    {
        /// TODO : change formulas for actually balanced formulas
        Mission mission;
        mission.difficulty = RandomEngine.Instance.Range(minDiff, maxDiff + 1);
        mission.moneyReward = 0; //RandomEngine.Instance.Range(mission.difficulty * 10, mission.difficulty * 15);
        if (mission.difficulty >= 4 && RandomEngine.Instance.Range(0, 10) < mission.difficulty)
        {
            /// TODO : random character with some levels
            mission.recruitReward = new AllyCharacter(EnumClasses.Sniper, 20, 2, 65, 10, 15, 20, 4, 60);
        }
        else
        {
            mission.recruitReward = null;
        }
        WinCondition winCondition = (WinCondition)RandomEngine.Instance.Range(0, Enum.GetNames(typeof(WinCondition)).Length);
        mission.missionTypeData = BetweenMissionsGameManager.Instance.GetMissionType(winCondition);

        return mission;
    }
}
