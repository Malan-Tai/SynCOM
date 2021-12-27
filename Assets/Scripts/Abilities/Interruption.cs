using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interruption
{
    public bool HasStarted { get; private set; }
    public bool IsDone { get; protected set; }

    public static Interruption GetInterruption(InterruptionType type)
    {
        Interruption interruption;

        switch (type)
        {
            case InterruptionType.FocusTargetForGivenTime:
                interruption = new FocusTargetForGivenTimeInterruption();
                break;

            default:
                return null;
        }

        interruption.HasStarted = false;
        interruption.IsDone = false;

        return interruption;
    }

    public void StartCoroutine(MonoBehaviour starter)
    {
        HasStarted = true;
        starter.StartCoroutine(InterruptionCoroutine());
    }

    protected abstract IEnumerator InterruptionCoroutine();
}
