using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridMap : MonoBehaviour
{
    [SerializeField]
    private float _cellSize;
    public float CellSize { get { return _cellSize; } }

    [SerializeField]
    private GameObject[] _tilePrefabs;

    private Tile[,] _map;
    // max x coordinate (exclusive)
    private int _maxX;
    // max y coordinate (exclusive)
    private int _maxY;

    private List<Vector2Int> _occupiedTiles;

    public Tile this[Vector2Int u]
    {
        get
        {
            if (!CellIsValid(u))
            {
                return null;
            }
            return _map[u.x, u.y];
        }
    }

    public Tile this[int x, int y]
    {
        get
        {
            if (!CellIsValid(x, y))
            {
                return null;
            }
            return _map[x, y];
        }
    }

    private void Awake()
    {
        _maxX = 20;
        _maxY = 20;
        _map = new Tile[_maxX, _maxY];

        for (int x = 0; x < _maxX; x++)
        {
            for (int y = 0; y < _maxY; y++)
            {
                int cover = 0;
                if (x == 0 || x == _maxX - 1 || y == 0 || y == _maxY - 1 || (x <= 9 && y == 13)) cover = 2;
                else if (x == 10 || y == 10 || y == _maxY - 2) cover = 1;

                //int cover = Random.Range(0, 3);
                _map[x, y] = new Tile(x, y, (Cover)cover);

                if (cover < _tilePrefabs.Length)
                {
                    GameObject obj = Instantiate(_tilePrefabs[cover], this.transform);
                    obj.transform.position = GridToWorld(new Vector2Int(x, y), 0);
                    obj.transform.localScale = new Vector3(_cellSize, obj.transform.localScale.y, _cellSize);
                    var tileComp = obj.GetComponent<TileComponent>();
                    tileComp.SetTile(_map[x, y]);
                }
            }
        }

        _occupiedTiles = new List<Vector2Int>();
    }

    public Vector3 GridToWorld(Vector2Int grid, float y)
    {
        return new Vector3(grid.x * _cellSize + _cellSize / 2, y, grid.y * _cellSize + _cellSize / 2);
    }

    public Vector2Int WorldToGrid(Vector3 world, bool addToOccupiedTiles = false)
    {
        Vector2Int gridPos = new Vector2Int();
        gridPos.x = (int)Mathf.Floor(world.x / _cellSize - 0.5f);
        gridPos.y = (int)Mathf.Floor(world.z / _cellSize - 0.5f);

        if (addToOccupiedTiles)
        {
            _occupiedTiles.Add(gridPos);
        }

        return gridPos;
    }

    public Tile[] Neighbors(Vector2Int centerCell)
    {
        Tile[] neighbors = new Tile[8];
        int i = 0;

        int x = centerCell.x;
        int y = centerCell.y;

        bool canUp = (centerCell.y > 0);
        bool canDown = (centerCell.y < _maxY - 1);
        bool canLeft = (centerCell.x > 0);
        bool canRight = (centerCell.x < _maxX - 1);

        if (canUp)
        {
            Vector2Int nextCell = new Vector2Int(x, y - 1);
            if (CanMoveFromCellToCell(centerCell, nextCell))
            {
                neighbors[i] = this[nextCell];
                i++;
            }
        }
        if (canDown)
        {
            Vector2Int nextCell = new Vector2Int(x, y + 1);
            if (CanMoveFromCellToCell(centerCell, nextCell))
            {
                neighbors[i] = this[nextCell];
                i++;
            }
        }
        if (canLeft)
        {
            Vector2Int nextCell = new Vector2Int(x - 1, y);
            if (CanMoveFromCellToCell(centerCell, nextCell))
            {
                neighbors[i] = this[nextCell];
                i++;
            }
        }
        if (canRight)
        {
            Vector2Int nextCell = new Vector2Int(x + 1, y);
            if (CanMoveFromCellToCell(centerCell, nextCell))
            {
                neighbors[i] = this[nextCell];
                i++;
            }
        }
        if (canUp && canLeft)
        {
            Vector2Int nextCell = new Vector2Int(x - 1, y - 1);
            if (CanMoveFromCellToCell(centerCell, nextCell))
            {
                neighbors[i] = this[nextCell];
                i++;
            }
        }
        if (canUp && canRight)
        {
            Vector2Int nextCell = new Vector2Int(x + 1, y - 1);
            if (CanMoveFromCellToCell(centerCell, nextCell))
            {
                neighbors[i] = this[nextCell];
                i++;
            }
        }
        if (canDown && canLeft)
        {
            Vector2Int nextCell = new Vector2Int(x - 1, y + 1);
            if (CanMoveFromCellToCell(centerCell, nextCell))
            {
                neighbors[i] = this[nextCell];
                i++;
            }
        }
        if (canDown && canRight)
        {
            Vector2Int nextCell = new Vector2Int(x + 1, y + 1);
            if (CanMoveFromCellToCell(centerCell, nextCell))
            {
                neighbors[i] = this[nextCell];
                i++;
            }
        }

        Tile[] finalNeigh = new Tile[i];
        int j;
        for (j = 0; j < i; j++)
        {
            finalNeigh[j] = neighbors[j];
        }

        return finalNeigh;
    }

    public bool CanMoveFromCellToCell(Vector2Int cellA, Vector2Int cellB)
    {
        if (!CellIsValid(cellA) || !CellIsValid(cellB))
        {
            return false;
        }

        Tile tileA = _map[cellA.x, cellA.y];
        Tile tileB = _map[cellB.x, cellB.y];

        return ((tileA.Cover == Cover.None && (tileB.Cover == Cover.Half || tileB.Cover == Cover.None)) ||
                (tileA.Cover == Cover.Half && tileB.Cover == Cover.None)) &&
                !_occupiedTiles.Contains(cellB);
    }

    public bool CellIsValid(Vector2Int u)
    {
        return u.x >= 0 && u.x < _maxX && u.y >= 0 && u.y < _maxY;
    }

    public bool CellIsValid(int x, int y)
    {
        return x >= 0 && x < _maxX && y >= 0 && y < _maxY;
    }

    public void UpdateOccupiedTiles(Vector2Int from, Vector2Int to)
    {
        _occupiedTiles.Remove(from);
        _occupiedTiles.Add(to);
    }
}
