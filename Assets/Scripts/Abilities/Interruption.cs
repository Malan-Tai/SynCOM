using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interruption
{
    public const float FOCUS_TARGET_TIME = 1f;

    public bool HasStarted { get; private set; }
    public bool IsDone { get; protected set; }

    public static Interruption GetInitializedInterruption(InterruptionParameters parameters)
    {
        Interruption interruption;

        switch (parameters.interruptionType)
        {
            case InterruptionType.FocusTargetForGivenTime:
                interruption = new FocusTargetForGivenTimeInterruption();
                break;

            case InterruptionType.FocusTargetUntilEndOfMovement:
                interruption = new FocusTargetUntilEndOfMovementInterruption();
                break;

            case InterruptionType.FocusTargetForGivenTimeAndFireTextFeedback:
                interruption = new FocusTargetForGivenTimeAndFireTextFeedback();
                break;

            default:
                return null;
        }

        interruption.HasStarted = false;
        interruption.IsDone = false;

        interruption.Init(parameters);

        return interruption;
    }

    public void StartCoroutine(MonoBehaviour starter)
    {
        HasStarted = true;
        starter.StartCoroutine(InterruptionCoroutine());
    }

    protected abstract IEnumerator InterruptionCoroutine();

    protected abstract void Init(InterruptionParameters parameters);
}
