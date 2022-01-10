using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Interruption", menuName = "Interruption")]
public class InterruptionScriptableObject : ScriptableObject
{
    public InterruptionType interruptionType;

    public float time;

    public InterruptionParameters ToParameters(GridBasedUnit currentUnit, GridBasedUnit sourceUnit)
    {
        return new InterruptionParameters
        {
            interruptionType = interruptionType,
            time = time,
            target = currentUnit,
            position = sourceUnit.GridPosition
        };
    }
}