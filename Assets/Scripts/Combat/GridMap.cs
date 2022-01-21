using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridMap : MonoBehaviour
{
    [SerializeField]
    private GridInitializer _gridInitializer;

    [Range(0.2f, 10f)] public float CellSize = 1f;

#if UNITY_EDITOR
    [Header("Gizmos")]
    public bool ShowGridGizmos = true;
    public bool ShowWalkableGizmos = true;
    public bool ShowCoversGizmos = true;
#endif

    public int GridTileWidth { get => _map.GetLength(0); }
    public int GridTileHeight { get => _map.GetLength(1); }
    public float GridWorldWidth { get => CellSize * _map.GetLength(0); }
    public float GridWorldHeight { get => CellSize * _map.GetLength(1); }
    public Vector3 GridWorldCenter
    {
        get => new Vector3(_gridOrigin.x + GridWorldWidth / 2f, 0f, _gridOrigin.z + GridWorldHeight / 2f);
    }

    private Tile[,] _map;
    private List<Vector2Int> _occupiedTiles = new List<Vector2Int>();
    private Vector3 _gridOrigin;

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
        _map = _gridInitializer.CreateGrid(CellSize);

        _occupiedTiles = new List<Vector2Int>();
        Vector2 minimums = _gridInitializer.GetMinimums();
        _gridOrigin = new Vector3(minimums.x, 0f, minimums.y);
    }

    public Vector3 GridToWorld(Vector2Int grid, float y)
    {
        return new Vector3(_gridOrigin.x + grid.x * CellSize + CellSize / 2, y, _gridOrigin.z + grid.y * CellSize + CellSize / 2);
    }

    public Vector3 GridToWorld(int gridX, int gridY, float y)
    {
        return new Vector3(_gridOrigin.x + gridX * CellSize + CellSize / 2, y, _gridOrigin.z + gridY * CellSize + CellSize / 2);
    }

    public Vector2Int WorldToGrid(Vector3 world, bool addToOccupiedTiles = false)
    {
        Vector2Int gridPos = new Vector2Int();
        gridPos.x = (int)Mathf.Floor((world.x - _gridOrigin.x) / CellSize);
        gridPos.y = (int)Mathf.Floor((world.z - _gridOrigin.z) / CellSize);

        if (addToOccupiedTiles)
        {
            _occupiedTiles.Add(gridPos);
        }

        return gridPos;
    }

    // /!\ Just enumerate neighbors regardless of if they're empty or not!
    // Still doesn't return out of grid tiles as they can't exist
    public Tile[] TileNeighbors(Vector2Int centerCell)
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

    public Tile[] MovementNeighbors(Vector2Int centerCell, Vector2Int goalCell)
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
            if (CanMoveFromCellToCell(centerCell, coords, goalCell))
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

    public Tile GetRandomFreeNeighbor(Vector2Int center)
    {
        Tile[] all = CoverNeighbors(center);
        List<Tile> possibleTiles = new List<Tile>();

        foreach (Tile tile in all)
        {
            if (!_occupiedTiles.Contains(tile.Coords) && tile.Cover == EnumCover.None)
            {
                possibleTiles.Add(tile);
            }
        }

        if (possibleTiles.Count <= 0) return null;
        return possibleTiles[UnityEngine.Random.Range(0, possibleTiles.Count)];
    }

    public bool CanMoveFromCellToCell(Vector2Int cellA, Vector2Int cellB)
    {
        if (!CellIsValid(cellA) || !CellIsValid(cellB))
        {
            return false;
        }

        Tile tileA = _map[cellA.x, cellA.y];
        Tile tileB = _map[cellB.x, cellB.y];

        Tile sideTileA = _map[cellA.x, cellB.y];
        Tile sideTileB = _map[cellB.x, cellA.y];

        return _map[cellB.x, cellB.y].IsWalkable && !_occupiedTiles.Contains(cellB) &&
                ((tileA.Cover == EnumCover.None && (tileB.Cover == EnumCover.Half || tileB.Cover == EnumCover.None)) ||
                (tileA.Cover == EnumCover.Half && tileB.Cover == EnumCover.None)) &&
                (sideTileA.Cover != EnumCover.Full || sideTileB.Cover != EnumCover.Full);
    }

    public bool CanMoveFromCellToCell(Vector2Int cellA, Vector2Int cellB, Vector2Int goalCell)
    {
        if (!CellIsValid(cellA) || !CellIsValid(cellB))
        {
            return false;
        }

        Tile tileA = _map[cellA.x, cellA.y];
        Tile tileB = _map[cellB.x, cellB.y];

        Tile sideTileA = _map[cellA.x, cellB.y];
        Tile sideTileB = _map[cellB.x, cellA.y];

        return _map[cellB.x, cellB.y].IsWalkable && (cellB == goalCell || !_occupiedTiles.Contains(cellB)) &&
                ((tileA.Cover == EnumCover.None && (tileB.Cover == EnumCover.Half || tileB.Cover == EnumCover.None)) ||
                (tileA.Cover == EnumCover.Half && tileB.Cover == EnumCover.None)) &&
                (sideTileA.Cover != EnumCover.Full || sideTileB.Cover != EnumCover.Full);
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

    public void FreeOccupiedTile(Vector2Int freed)
    {
        _occupiedTiles.Remove(freed);
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

    public void RecalculateGrid()
    {
        _map = _gridInitializer.CreateGrid(CellSize);
    }

    public List<Tile> GetAreaOfEffectDiamond(Vector2Int center, int radius)
    {
        List<Tile> diamond = new List<Tile>();

        for (int i = -radius; i <= radius; i++)
        {
            for (int j = Mathf.Abs(i) - radius; j <= radius - Mathf.Abs(i); j++)
            {
                Tile tile = this[center.x + i, center.y + j];
                if (tile != null)
                {
                    diamond.Add(tile);
                }
            }
        }

        return diamond;
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
            _map = _gridInitializer.CreateGrid(CellSize);
        }

        _gridInitializer.DetermineGridSize(CellSize);
        Vector2 minimums = _gridInitializer.GetMinimums();
        _gridOrigin = new Vector3(minimums.x, 0f, minimums.y);

        for (int x = 0; x < _map.GetLength(0); x++)
        {
            for (int z = 0; z < _map.GetLength(1); z++)
            {
                Vector3 cellPosition = GridToWorld(x, z, 0f);

                // Draw grid in gizmos
                if (ShowGridGizmos)
                {
                    Gizmos.color = Color.gray;
                    Vector3 displacement = new Vector3(CellSize / 2f, 0f, CellSize / 2f);
                    Vector3 minX = GridToWorld(0, z, 0.5f) - displacement;
                    Vector3 maxX = GridToWorld(_map.GetLength(0), z, 0.5f) - displacement;
                    Vector3 minZ = GridToWorld(x, 0, 0.5f) - displacement;
                    Vector3 maxZ = GridToWorld(x, _map.GetLength(1), 0.5f) - displacement;
                    Gizmos.DrawLine(minX, maxX);
                    Gizmos.DrawLine(minZ, maxZ);
                }

                // Draw grid tiles : blue = walkable, red = not walkable
                if (_map[x, z] != null && ShowWalkableGizmos)
                {
                    if (_map[x, z].IsWalkable)
                    {
                        Gizmos.color = Color.blue;
                    }
                    else
                    {
                        Gizmos.color = Color.red;
                    }
                    Gizmos.DrawCube(cellPosition + Vector3.up * 3f, new Vector3(CellSize, 0.01f, CellSize));
                }

                // Draw covers
                if (_map[x, z] != null && ShowCoversGizmos)
                {
                    switch (_map[x, z].Cover)
                    {
                        default:
                        case EnumCover.None:
                            break;
                        case EnumCover.Half:
                            Gizmos.DrawIcon(cellPosition + Vector3.up * 4f, "half_cover_icon.png", true, Color.yellow);
                            break;
                        case EnumCover.Full:
                            Gizmos.DrawIcon(cellPosition + Vector3.up * 4f, "full_cover_icon.png", true, Color.green);
                            break;
                    }
                }
            }
        }
    }

#endregion
#endif
}
