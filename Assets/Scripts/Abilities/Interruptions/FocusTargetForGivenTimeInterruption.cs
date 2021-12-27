using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusTargetForGivenTimeInterruption : Interruption
{
    private float _time;

    public void Init(GridBasedUnit target, float time)
    {
        CombatGameManager.Instance.Camera.SwitchParenthood(target);

        _time = time;
    }

    protected override IEnumerator InterruptionCoroutine()
    {
        yield return _time;
        IsDone = true;
    }
}
