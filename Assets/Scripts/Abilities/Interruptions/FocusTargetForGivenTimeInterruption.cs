using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusTargetForGivenTimeInterruption : Interruption
{
    private float _time;
    private GridBasedUnit _target;

    protected override void Init(InterruptionParameters parameters)
    {
        _target = parameters.target;
        _time = parameters.time;
    }

    protected override IEnumerator InterruptionCoroutine()
    {
        CombatGameManager.Instance.Camera.SwitchParenthood(_target);
        yield return new WaitForSeconds(_time);
        IsDone = true;
    }
}
