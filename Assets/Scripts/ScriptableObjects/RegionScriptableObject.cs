using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Region", menuName = "Region")]
public class RegionScriptableObject : ScriptableObject
{
    public RegionName regionName;

    [TextArea(15, 20)]
    public string description;
}
