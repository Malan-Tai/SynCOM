using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBasedUnit : MonoBehaviour
{
    private Vector2Int _gridPosition;
    private Vector3 _targetWorldPosition;

    [SerializeField]
    private float _moveSpeed;

    private float _movesLeft;

    private Pathfinder _pathfinder = new Pathfinder();
    private bool _updatePathfinder = true;
    private List<Vector2Int> _pathToFollow;
    private bool _followingPath;

    private Dictionary<GridBasedUnit, LineOfSight> _linesOfSight;
    private float _sightDistance;

    public delegate void StartedMoving(GridBasedUnit movedUnit, Vector2Int finalPos);
    public static event StartedMoving OnMoveStart;

    private void Start()
    {
        GridMap gridMap = CombatGameManager.Instance.gridMap;

        _gridPosition = gridMap.WorldToGrid(this.transform.position, true);
        this.transform.position = gridMap.GridToWorld(_gridPosition, this.transform.position.y);
        _targetWorldPosition = this.transform.position;

        _movesLeft = 10;
        _sightDistance = 20;

        _linesOfSight = new Dictionary<GridBasedUnit, LineOfSight>();
    }

    private void Update()
    {
        if (_updatePathfinder)
        {
            _pathfinder.Dijkstra(_movesLeft, _gridPosition);
            _pathToFollow = new List<Vector2Int>();
            _updatePathfinder = false;
            _followingPath = false;

            if (CombatGameManager.Instance.CurrentUnit == this) CombatGameManager.Instance.UpdateReachableTiles();
        }

        Vector3 difference = _targetWorldPosition - this.transform.position;
        if (difference.sqrMagnitude > CombatGameManager.Instance.gridMap.CellSize / 100f)
        {
            Vector3 movement = difference.normalized * _moveSpeed * Time.deltaTime;
            this.transform.position += movement;
        }
        else if (_pathToFollow.Count > 0)
        {
            MoveToCell(_pathToFollow[0]);
            _pathToFollow.RemoveAt(0);
        }
        else if (_followingPath)
        {
            _updatePathfinder = true;
            UpdateLineOfSights();
        }
    }

    public void MoveToNeighbor(Vector2Int deltaGrid)
    {
        _gridPosition += deltaGrid;
        _targetWorldPosition = CombatGameManager.Instance.gridMap.GridToWorld(_gridPosition, this.transform.position.y);
    }

    public void MoveToCell(Vector2Int cell)
    {
        _gridPosition = cell;
        _targetWorldPosition = CombatGameManager.Instance.gridMap.GridToWorld(_gridPosition, this.transform.position.y);
    }

    public void ChoosePathTo(Vector2Int cell)
    {
        if (_followingPath) return;

        float cost;
        _pathToFollow = _pathfinder.GetPathToTile(cell, out cost);
        _movesLeft -= cost;

        if (_pathToFollow.Count > 0)
        {
            _followingPath = true;
            CombatGameManager.Instance.gridMap.UpdateOccupiedTiles(_gridPosition, cell);

            if (OnMoveStart != null) OnMoveStart(this, cell);
        }
    }

    public void NeedsPathfinderUpdate()
    {
        _updatePathfinder = true;
    }

    public void UpdateLineOfSights(bool targetEnemies = true)
    {
        GridMap map = CombatGameManager.Instance.gridMap;
        _linesOfSight = new Dictionary<GridBasedUnit, LineOfSight>();

        List<GridBasedUnit> listToCycle;
        if (targetEnemies)
        {
            listToCycle = CombatGameManager.Instance.EnemyUnits;
        }
        else
        {
            listToCycle = CombatGameManager.Instance.ControllableUnits;
        }

        List<Vector2Int> shooterSidesteps = map.SidestepPositions(_gridPosition);
        shooterSidesteps.Insert(0, _gridPosition);

        foreach (GridBasedUnit unit in listToCycle)
        {
            List<CoverPlane> planes = map.GetCoverPlanes(unit._gridPosition);
            List<Vector2Int> targetSidesteps = map.SidestepPositions(unit._gridPosition);
            targetSidesteps.Insert(0, unit._gridPosition);

            LineOfSight bestLine = new LineOfSight();
            bestLine.seen = false;
            bestLine.cover = EnumCover.Full; // worst case for the shooter

            foreach (Vector2Int shooterSidestep in shooterSidesteps)
            {
                foreach (Vector2Int targetSidestep in targetSidesteps)
                {
                    LineOfSight newLine = ComputeLineOfSight(planes, shooterSidestep, targetSidestep, unit.transform.position.y);

                    if (newLine.seen && (!bestLine.seen || (int)newLine.cover < (int)bestLine.cover))
                    {
                        bestLine = newLine;
                    }
                }
            }

            if (bestLine.seen)
            {
                _linesOfSight.Add(unit, bestLine);
                print("i see unit at " + unit._gridPosition + " with cover " + (int)bestLine.cover + " by sidestepping by " + (bestLine.sidestepCell - _gridPosition));
            }
        }
    }

    private LineOfSight ComputeLineOfSight(List<CoverPlane> targetCoverPlanes, Vector2Int shooterPosition, Vector2Int targetPosition, float targetY)
    {
        GridMap map = CombatGameManager.Instance.gridMap;
        LayerMask layerMask = 1 << 6;

        LineOfSight lineOfSight = new LineOfSight();

        Vector3 lineEnd = map.GridToWorld(targetPosition, targetY);
        Vector3 lineStart = map.GridToWorld(shooterPosition, this.transform.position.y);
        Vector3 line = lineEnd - lineStart;
        bool seen = !Physics.Raycast(lineStart, line, Mathf.Min(line.magnitude, _sightDistance), layerMask);

        lineOfSight.seen = seen;

        if (!seen)
        {
            return lineOfSight;
        }

        EnumCover bestCover = EnumCover.None; // best cover for the target
        foreach (CoverPlane plane in targetCoverPlanes)
        {
            if (plane.IntersectsSegment(lineStart, lineEnd))
            {
                if (plane.cover == EnumCover.Full)
                {
                    bestCover = EnumCover.Full;
                    break;
                }
                else if (plane.cover == EnumCover.Half && bestCover == EnumCover.None)
                {
                    bestCover = EnumCover.Half;
                }
            }
        }

        lineOfSight.cover = bestCover;
        lineOfSight.sidestepCell = shooterPosition;

        return lineOfSight;
    }

    public List<Tile> GetReachableTiles()
    {
        return _pathfinder.GetReachableTiles();
    }
}