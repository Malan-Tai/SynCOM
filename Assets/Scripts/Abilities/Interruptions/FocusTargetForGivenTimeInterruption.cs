using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusTargetForGivenTimeInterruption : Interruption
{
    private float _time;

    protected override void Init(InterruptionParameters parameters)
    {
        CombatGameManager.Instance.Camera.SwitchParenthood(parameters.target);
        _time = parameters.time;
    }

    protected override IEnumerator InterruptionCoroutine()
    {
        yield return new WaitForSeconds(_time);
        IsDone = true;
    }
}
