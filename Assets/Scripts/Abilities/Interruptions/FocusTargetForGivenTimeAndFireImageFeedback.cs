using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusTargetForGivenTimeAndFireImageFeedback : Interruption
{
    private float _time;
    private GridBasedUnit _target;
    private Sprite _sprite;

    protected override void Init(InterruptionParameters parameters)
    {
        _target = parameters.target;
        _time = parameters.time;
        _sprite = parameters.sprite;
    }

    protected override IEnumerator InterruptionCoroutine()
    {
        CombatGameManager.Instance.Camera.SwitchParenthood(_target);
        _target.DisplayRaisingImageFeedback(_sprite);
        yield return new WaitForSeconds(_time);
        IsDone = true;
    }
}
