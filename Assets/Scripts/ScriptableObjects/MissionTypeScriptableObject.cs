using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MissionType", menuName = "MissionType")]
public class MissionTypeScriptableObject : ScriptableObject
{
    public WinCondition winCondition;

    public string summary;

    [TextArea(15, 20)]
    public string description;
}
