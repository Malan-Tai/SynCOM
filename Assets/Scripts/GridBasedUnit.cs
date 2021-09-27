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

    public delegate void FinishedMoving(GridBasedUnit movedUnit, Vector2Int finalPos);
    public static event FinishedMoving OnMoveFinish;

    private void Start()
    {
        GridMap gridMap = GameManager.Instance.gridMap;

        _gridPosition = gridMap.WorldToGrid(this.transform.position, true);
        this.transform.position = gridMap.GridToWorld(_gridPosition, this.transform.position.y);
        _targetWorldPosition = this.transform.position;

        _movesLeft = 12;
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
        print(_movesLeft);

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
}
