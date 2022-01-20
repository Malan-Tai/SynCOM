using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusTargetForGivenTimeAndFireTextFeedback : Interruption
{
    private float _time;
    private GridBasedUnit _target;
    private string _text;

    protected override void Init(InterruptionParameters parameters)
    {
        _target = parameters.target;
        _time = parameters.time;
        _text = parameters.text;
    }

    protected override IEnumerator InterruptionCoroutine()
    {
        CombatGameManager.Instance.Camera.SwitchParenthood(_target);
        _target.DisplayFeedback(_text);
        yield return new WaitForSeconds(_time);
        IsDone = true;
    }
}
