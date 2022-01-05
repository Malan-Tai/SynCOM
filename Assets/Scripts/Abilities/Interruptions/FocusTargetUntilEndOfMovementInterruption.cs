using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusTargetUntilEndOfMovementInterruption : Interruption
{
    private GridBasedUnit _target;
    private bool _receivedEvent = false;

    protected override void Init(InterruptionParameters parameters)
    {
        _target = parameters.target;
        GridBasedUnit.OnMoveFinish += UnitFinishedMoving;
    }

    protected override IEnumerator InterruptionCoroutine()
    {
        CombatGameManager.Instance.Camera.SwitchParenthood(_target);
        
        while (!_receivedEvent)
        {
            yield return null;
        }

        IsDone = true;
    }

    private void UnitFinishedMoving(GridBasedUnit unit)
    {
        if (unit == _target)
        {
            _receivedEvent = true;
            GridBasedUnit.OnMoveFinish -= UnitFinishedMoving;
        }
    }
}
