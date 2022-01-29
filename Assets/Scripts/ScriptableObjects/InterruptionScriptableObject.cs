using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Interruption", menuName = "Interruption")]
public class InterruptionScriptableObject : ScriptableObject
{
    public InterruptionType interruptionType;

    public float time;
    public string text;
    public Sprite sprite;
    public SoundManager.Sound sound;

    public InterruptionParameters ToParameters(GridBasedUnit currentUnit, GridBasedUnit sourceUnit, bool onCurrent = true)
    {
        return new InterruptionParameters
        {
            interruptionType = interruptionType,
            time = time,
            text = text,
            sprite = sprite,
            sound = sound,
            target = onCurrent ? currentUnit : sourceUnit,
            position = onCurrent ? sourceUnit.GridPosition : currentUnit.GridPosition
        };
    }
}
