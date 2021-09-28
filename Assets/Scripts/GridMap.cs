using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct CoverPlane
{
    public Plane plane;
    public Cover cover;

    public bool IntersectsSegment(Vector3 start, Vector3 end)
    {
        return !plane.SameSide(start, end);
    }
}

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
                else if (y == 10 || y == _maxY - 2) cover = 1;

                //int cover = Random.Range(0, 3);
                _map[x, y] = new Tile(x, y, (Cover)cover);

                if (cover < _tilePrefabs.Length)
                {
                    GameObject obj = Instantiate(_tilePrefabs[cover], this.transform);
                    obj.transform.position = GridToWorld(new Vector2Int(x, y), 0);
                    obj.transform.localScale = new Vector3(_cellSize, obj.transform.localScale.y, _cellSize);

                    var tileComp = obj.GetComponent<TileComponent>();
                    tileComp.SetTile(_map[x, y]);
                    _map[x, y].SetTileComponent(tileComp);
                }
            }
        }

        _occupiedTiles = new List<Vector2Int>();
    }

    public Vector3 GridToWorld(Vector2Int grid, float y)
    {
        return new Vector3(grid.x * _cellSize + _cellSize / 2, y, grid.y * _cellSize + _cellSize / 2);
    }

    public Vector3 GridToWorld(int gridX, int gridY, float y)
    {
        return new Vector3(gridX * _cellSize + _cellSize / 2, y, gridY * _cellSize + _cellSize / 2);
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

    public Tile[] MovementNeighbors(Vector2Int centerCell)
    {
        Tile[] neighbors = new Tile[8];
        int i = 0;

        int x = centerCell.x;
        int y = centerCell.y;

        Vector2Int[] neighborCoords = new Vector2Int[]
        {
            new Vector2Int(x - 1, y - 1),
            new Vector2Int(x - 1, y    ),
            new Vector2Int(x - 1, y + 1),
            new Vector2Int(x    , y - 1),
            new Vector2Int(x    , y + 1),
            new Vector2Int(x + 1, y - 1),
            new Vector2Int(x + 1, y    ),
            new Vector2Int(x + 1, y + 1)
        };

        foreach (Vector2Int coords in neighborCoords)
        {
            if (CanMoveFromCellToCell(centerCell, coords))
            {
                neighbors[i] = this[coords];
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

    public Tile[] CoverNeighbors(Vector2Int centerCell)
    {
        Tile[] neighbors = new Tile[8];
        int i = 0;

        int x = centerCell.x;
        int y = centerCell.y;

        Vector2Int[] neighborCoords = new Vector2Int[]
        {
            new Vector2Int(x - 1, y    ),
            new Vector2Int(x    , y - 1),
            new Vector2Int(x    , y + 1),
            new Vector2Int(x + 1, y    ),
        };

        foreach (Vector2Int coords in neighborCoords)
        {
            if (CellIsValid(coords))
            {
                neighbors[i] = this[coords];
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

    public List<Vector2Int> SidestepPositions(Vector2Int centerCell)
    {
        List<Vector2Int> positions = new List<Vector2Int>();

        int x = centerCell.x;
        int y = centerCell.y;

        if ((CellIsValid(x + 1, y) && this[x + 1, y].Cover != Cover.None) || (CellIsValid(x - 1, y) && this[x - 1, y].Cover != Cover.None))
        {
            if (CellIsValid(x, y + 1) && this[x, y + 1].Cover == Cover.None)
            {
                positions.Add(new Vector2Int(x, y + 1));
            }
            if (CellIsValid(x, y - 1) && this[x, y - 1].Cover == Cover.None)
            {
                positions.Add(new Vector2Int(x, y - 1));
            }
        }

        if ((CellIsValid(x, y + 1) && this[x, y + 1].Cover != Cover.None) || (CellIsValid(x, y - 1) && this[x, y - 1].Cover != Cover.None))
        {
            if (CellIsValid(x + 1, y) && this[x + 1, y].Cover == Cover.None)
            {
                positions.Add(new Vector2Int(x + 1, y));
            }
            if (CellIsValid(x - 1, y) && this[x - 1, y].Cover == Cover.None)
            {
                positions.Add(new Vector2Int(x - 1, y));
            }
        }

        return positions;
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

    public List<CoverPlane> GetCoverPlanes(Vector2Int cell)
    {
        Tile[] neighbors = CoverNeighbors(cell);
        var planes = new List<CoverPlane>();
        
        foreach (Tile tile in neighbors)
        {
            if (tile.Cover != Cover.None)
            {
                var plane = new CoverPlane();
                plane.cover = tile.Cover;

                Vector3 normal = GridToWorld(tile.Coords, 0) - GridToWorld(cell, 0);
                Vector3 point = GridToWorld(cell, 0f) + normal / 2;

                plane.plane = new Plane(normal, point);

                planes.Add(plane);
            }
        }

        return planes;
    }
}
