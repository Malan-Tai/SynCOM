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

    private Dictionary<GridBasedUnit, Cover> _linesOfSight;

    public delegate void FinishedMoving(GridBasedUnit movedUnit, Vector2Int finalPos);
    public static event FinishedMoving OnMoveFinish;

    private void Start()
    {
        GridMap gridMap = GameManager.Instance.gridMap;

        _gridPosition = gridMap.WorldToGrid(this.transform.position, true);
        this.transform.position = gridMap.GridToWorld(_gridPosition, this.transform.position.y);
        _targetWorldPosition = this.transform.position;

        _movesLeft = 20;

        _linesOfSight = new Dictionary<GridBasedUnit, Cover>();
    }

    private void Update()
    {
        if (_updatePathfinder)
        {
            _pathfinder.Dijkstra(_movesLeft, _gridPosition);
            _pathToFollow = new List<Vector2Int>();
            _updatePathfinder = false;
            _followingPath = false;
        }

        Vector3 difference = _targetWorldPosition - this.transform.position;
        if (difference.sqrMagnitude > GameManager.Instance.gridMap.CellSize / 100f)
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
            if (OnMoveFinish != null) OnMoveFinish(this, _gridPosition);
            UpdateLineOfSights();
        }
    }

    public void MoveToNeighbor(Vector2Int deltaGrid)
    {
        _gridPosition += deltaGrid;
        _targetWorldPosition = GameManager.Instance.gridMap.GridToWorld(_gridPosition, this.transform.position.y);
    }

    public void MoveToCell(Vector2Int cell)
    {
        _gridPosition = cell;
        _targetWorldPosition = GameManager.Instance.gridMap.GridToWorld(_gridPosition, this.transform.position.y);
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
            GameManager.Instance.gridMap.UpdateOccupiedTiles(_gridPosition, cell);
        }
    }

    public void NeedsPathfinderUpdateIfCellReachable(Vector2Int cell)
    {
        _updatePathfinder = _updatePathfinder || _pathfinder.CanReachCell(cell);
    }

    // needs to check for visibility too
    public void UpdateLineOfSights(bool targetEnemies = true)
    {
        GridMap map = GameManager.Instance.gridMap;
        _linesOfSight = new Dictionary<GridBasedUnit, Cover>();

        List<GridBasedUnit> listToCycle;
        if (targetEnemies)
        {
            listToCycle = GameManager.Instance.EnemyUnits;
        }
        else
        {
            listToCycle = GameManager.Instance.ControllableUnits;
        }

        foreach (GridBasedUnit unit in listToCycle)
        {
            Vector3 lineEnd = unit.transform.position;
            List<CoverPlane> planes = map.GetCoverPlanes(unit._gridPosition);

            Cover bestCover = Cover.None;
            foreach (CoverPlane plane in planes)
            {
                if (plane.IntersectsSegment(this.transform.position, lineEnd))
                {
                    if (plane.cover == Cover.Full)
                    {
                        bestCover = Cover.Full;
                        break;
                    }
                    else if (plane.cover == Cover.Half && bestCover == Cover.None)
                    {
                        bestCover = Cover.Half;
                    }
                }
            }

            _linesOfSight.Add(unit, bestCover);
            print("i see unit at " + unit._gridPosition + " with cover = " + (int)bestCover);
        }
    }
}
