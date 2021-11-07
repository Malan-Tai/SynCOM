using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridInitializer : MonoBehaviour
{
    [SerializeField] private Transform[] _gridPos;
    [SerializeField] private LayerMask _mapElementLayer;

    private int _minX = int.MaxValue;
    private int _minZ = int.MaxValue;
    private int _maxX = int.MinValue;
    private int _maxZ = int.MinValue;

    public Vector2Int DetermineGridSize(float cellSize)
    {
        _minX = int.MaxValue;
        _minZ = int.MaxValue;
        _maxX = int.MinValue;
        _maxZ = int.MinValue;

        for (int i = 0; i < _gridPos.Length; i++)
        {
            _minX = Mathf.Min(_minX, Mathf.FloorToInt(_gridPos[i].position.x));
            _maxX = Mathf.Max(_maxX, Mathf.FloorToInt(_gridPos[i].position.x));
            _minZ = Mathf.Min(_minZ, Mathf.FloorToInt(_gridPos[i].position.z));
            _maxZ = Mathf.Max(_maxZ, Mathf.FloorToInt(_gridPos[i].position.z));
        }

        return new Vector2Int(Mathf.RoundToInt((_maxX - _minX) / cellSize), Mathf.RoundToInt((_maxZ - _minZ) / cellSize));
    }

    public Tile[,] CreateGrid(float cellSize)
    {
        Vector2Int size = DetermineGridSize(cellSize);

        Tile[,] grid = new Tile[size.x, size.y];
        Vector3 overlapSize = new Vector3(cellSize * 0.45f, cellSize * 2f, cellSize * 0.45f);
        Collider[] overlap;

        // For each tile in the delimited zone, we detect ground, obstacles and covers with overlaps
        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z < size.y; z++)
            {
                bool walkable = true;
                EnumCover coverValue = EnumCover.None;

                overlap = Physics.OverlapBox(new Vector3(_minX + x * cellSize + cellSize / 2f, 0f, _minZ + z * cellSize + cellSize / 2f), overlapSize, Quaternion.identity, _mapElementLayer);

                for (int i = 0; i < overlap.Length; i++)
                {
                    MapElement mapElement = overlap[i].GetComponent<MapElement>();

                    if (mapElement != null)
                    {

                        walkable &= mapElement.IsWalkable;
                        coverValue = coverValue < mapElement.CoverValue ? mapElement.CoverValue : coverValue;
                    }
                }

                grid[x, z] = new Tile(x, z, coverValue, walkable);
            }
        }

        return grid;
    }

    public Vector2 GetMinimums()
    {
        return new Vector2(_minX, _minZ);
    }
}
