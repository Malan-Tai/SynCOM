using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridMap : MonoBehaviour
{
    [SerializeField]
    private GridInitializer _gridInitializer;
    public float CellSize { get => _cellSize; }
    [SerializeField, Range(1, 25)] private float _cellSize = 1f;

#if UNITY_EDITOR
    [Header("Gizmos")]
    public bool ShowGridGizmos = true;
    public bool ShowWalkableGizmos = true;
    public bool ShowCoversGizmos = true;
#endif

    /*[SerializeField]
    private GameObject[] _tilePrefabs;*/

    private Tile[,] _map;
    // max x coordinate (exclusive)
    /*private int _maxX;
    // max y coordinate (exclusive)
    private int _maxY;*/

    private List<Vector2Int> _occupiedTiles = new List<Vector2Int>();

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
        _map = _gridInitializer.CreateGrid(_cellSize);
        _occupiedTiles = new List<Vector2Int>();
        Vector2 minimums = _gridInitializer.GetMinimums();
        transform.position = new Vector3(minimums.x, 0f, minimums.y);

        /*_maxX = 20;
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
                _map[x, y] = new Tile(x, y, (EnumCover)cover);

                if (cover < _tilePrefabs.Length)
                {
                    GameObject obj = Instantiate(_tilePrefabs[cover], this.transform);
                    obj.transform.position = GridToWorld(new Vector2Int(x, y), 0);
                    obj.transform.localScale = new Vector3(CellSize, obj.transform.localScale.y, CellSize);

                    var tileComp = obj.GetComponent<TileComponent>();
                    tileComp.SetTile(_map[x, y]);
                    _map[x, y].SetTileComponent(tileComp);
                }
            }
        }*/
    }

    public Vector3 GridToWorld(Vector2Int grid, float y)
    {
        return new Vector3(transform.position.x + grid.x * _cellSize + _cellSize / 2, y, transform.position.z + grid.y * _cellSize + _cellSize / 2);
    }

    public Vector3 GridToWorld(int gridX, int gridY, float y)
    {
        return new Vector3(transform.position.x + gridX * _cellSize + _cellSize / 2, y, transform.position.z + gridY * _cellSize + _cellSize / 2);
    }

    public Vector2Int WorldToGrid(Vector3 world, bool addToOccupiedTiles = false)
    {
        Vector2Int gridPos = new Vector2Int();
        gridPos.x = (int)Mathf.Floor((world.x - transform.position.x) / _cellSize - 0.5f);
        gridPos.y = (int)Mathf.Floor((world.z - transform.position.z) / _cellSize - 0.5f);

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
        Tile[] neighbors = new Tile[4];
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

        if ((CellIsValid(x + 1, y) && this[x + 1, y].Cover != EnumCover.None) || (CellIsValid(x - 1, y) && this[x - 1, y].Cover != EnumCover.None))
        {
            if (CellIsValid(x, y + 1) && this[x, y + 1].Cover == EnumCover.None)
            {
                positions.Add(new Vector2Int(x, y + 1));
            }
            if (CellIsValid(x, y - 1) && this[x, y - 1].Cover == EnumCover.None)
            {
                positions.Add(new Vector2Int(x, y - 1));
            }
        }

        if ((CellIsValid(x, y + 1) && this[x, y + 1].Cover != EnumCover.None) || (CellIsValid(x, y - 1) && this[x, y - 1].Cover != EnumCover.None))
        {
            if (CellIsValid(x + 1, y) && this[x + 1, y].Cover == EnumCover.None)
            {
                positions.Add(new Vector2Int(x + 1, y));
            }
            if (CellIsValid(x - 1, y) && this[x - 1, y].Cover == EnumCover.None)
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

        return _map[cellB.x, cellB.y].IsWalkable && !_occupiedTiles.Contains(cellB) &&
                ((tileA.Cover == EnumCover.None && (tileB.Cover == EnumCover.Half || tileB.Cover == EnumCover.None)) ||
                (tileA.Cover == EnumCover.Half && tileB.Cover == EnumCover.None));
    }

    public bool CellIsValid(Vector2Int u)
    {
        return CellIsValid(u.x, u.y);
    }

    public bool CellIsValid(int x, int y)
    {
        return x >= 0 && x < _map.GetLength(0) && y >= 0 && y < _map.GetLength(1);
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
            if (tile.Cover != EnumCover.None)
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

    public EnumCover GetBestCoverAt(Vector2Int cell)
    {
        EnumCover best = EnumCover.None;
        foreach (Tile tile in CoverNeighbors(cell))
        {
            if ((int)tile.Cover > (int)best)
            {
                best = tile.Cover;
            }
        }

        return best;
    }


#if UNITY_EDITOR

#region Gizmos
    private void OnDrawGizmos()
    {
        if (_gridInitializer is null)
        {
            Debug.LogError("GridMap needs a GridInitializer to initialize the grid!");
            return;
        }

        if (_map is null)
        {
            _map = _gridInitializer.CreateGrid(_cellSize);
        }

        _gridInitializer.DetermineGridSize();
        Vector2 minimums = _gridInitializer.GetMinimums();
        transform.position = new Vector3(minimums.x, 0f, minimums.y);

        for (int x = 0; x < _map.GetLength(0); x++)
        {
            for (int z = 0; z < _map.GetLength(1); z++)
            {
                Vector3 cellPosition = GridToWorld(x, z, 0f);
                //Vector3 cellPosition = new Vector3(_minX + x + _cellSize / 2f, 0f, _minZ + z + _cellSize / 2f);

                if (ShowGridGizmos)
                {
                    Gizmos.color = Color.gray;
                    Vector3 displacement = new Vector3(_cellSize / 2f, 0f, _cellSize / 2f);
                    Vector3 minX = GridToWorld(0, z, 0.5f) - displacement;
                    Vector3 maxX = GridToWorld(_map.GetLength(0), z, 0.5f) - displacement;
                    Vector3 minZ = GridToWorld(x, 0, 0.5f) - displacement;
                    Vector3 maxZ = GridToWorld(x, _map.GetLength(1), 0.5f) - displacement;
                    Gizmos.DrawLine(minX, maxX);
                    Gizmos.DrawLine(minZ, maxZ);
                    /*Gizmos.DrawLine(new Vector3(_minX, 0.5f, z + _minZ), new Vector3(_maxX, 0.5f, z + _minZ));
                    Gizmos.DrawLine(new Vector3(x + _minX, 0.5f, _minZ), new Vector3(x + _minX, 0.5f, _maxZ));*/
                }

                if (_map != null && ShowWalkableGizmos)
                {
                    if (_map[x, z].IsWalkable)
                    {
                        Gizmos.color = Color.blue;
                    }
                    else
                    {
                        Gizmos.color = Color.red;
                    }
                    Gizmos.DrawCube(cellPosition + Vector3.up * 3f, new Vector3(_cellSize, 0.01f, _cellSize));
                }

                if (_map != null && ShowCoversGizmos)
                {
                    switch (_map[x, z].Cover)
                    {
                        default:
                        case EnumCover.None:
                            break;
                        case EnumCover.Half:
                            Gizmos.DrawIcon(cellPosition + Vector3.up * 4f, "half_cover_icon.png", true, Color.yellow);
                            //Gizmos.DrawWireCube(cellPosition, new Vector3(gizmosSize.x * 0.5f, gizmosSize.y * 1.5f, gizmosSize.z * 0.5f));
                            break;
                        case EnumCover.Full:
                            Gizmos.DrawIcon(cellPosition + Vector3.up * 4f, "full_cover_icon.png", true, Color.green);
                            //Gizmos.DrawWireCube(cellPosition, new Vector3(gizmosSize.x * 0.5f, gizmosSize.y * 2f, gizmosSize.z * 0.5f));
                            break;
                    }
                }
            }
        }
    }

#endregion
#endif
}
