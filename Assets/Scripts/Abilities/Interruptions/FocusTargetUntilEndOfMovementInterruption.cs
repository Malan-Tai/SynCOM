using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusTargetUntilEndOfMovementInterruption : Interruption
{
    private GridBasedUnit _target;
    private Vector2Int _movementTarget;
    private bool _receivedEvent = false;
    private PathfindingMoveType _pathfinding;

    protected override void Init(InterruptionParameters parameters)
    {
        _target = parameters.target;
        _movementTarget = parameters.position;
        _pathfinding = parameters.pathfinding;
        GridBasedUnit.OnMoveFinish += UnitFinishedMoving;
    }

    protected override IEnumerator InterruptionCoroutine()
    {
        CombatGameManager.Instance.Camera.SwitchParenthood(_target);

        switch (_pathfinding)
        {
            case PathfindingMoveType.Astar:
                _target.ChooseAstarPathTo(_movementTarget);
                break;
            case PathfindingMoveType.Linear:
                CombatGameManager.Instance.GridMap.UpdateOccupiedTiles(_target.GridPosition, _movementTarget);
                _target.MoveToCell(_movementTarget, true);
                CombatGameManager.Instance.UpdatePathfinders(_target, _movementTarget);
                break;
            default:
                _target.ChooseAstarPathTo(_movementTarget);
                break;
        }

        
        
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
