using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridInitializer : MonoBehaviour
{
    [SerializeField] private Transform[] _gridPos;
    [SerializeField] private LayerMask _mapElementLayer;

    private Tile[,] _grid;
    private int _minX = int.MaxValue;
    private int _minZ = int.MaxValue;
    private int _maxX = int.MinValue;
    private int _maxZ = int.MinValue;

    public Vector2Int DetermineGridSize()
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

        return new Vector2Int(_maxX - _minX, _maxZ - _minZ);
    }

    public Tile[,] CreateGrid(float cellSize)
    {
        Vector2Int size = DetermineGridSize();

        _grid = new Tile[size.x, size.y];
        Vector3 overlapSize = new Vector3(cellSize * 0.45f, cellSize * 2f, cellSize * 0.45f);
        Collider[] overlap;

        for (float x = 0; x < size.x; x += cellSize)
        {
            for (float z = 0; z < size.y; z += cellSize)
            {
                int coordX = (int) (x / cellSize);
                int coordY = (int) (z / cellSize);
                bool walkable = true;
                EnumCover coverValue = EnumCover.None;

                overlap = Physics.OverlapBox(new Vector3(_minX + x + cellSize / 2f, 0f, _minZ + z + cellSize / 2f), overlapSize, Quaternion.identity, _mapElementLayer);

                for (int i = 0; i < overlap.Length; i++)
                {
                    MapElement mapElement = overlap[i].GetComponent<MapElement>();

                    if (mapElement != null)
                    {

                        walkable &= mapElement.IsWalkable;
                        coverValue = coverValue < mapElement.CoverValue ? mapElement.CoverValue : coverValue;
                    }
                }

                _grid[coordX, coordY] = new Tile(coordX, coordY, coverValue, walkable);
            }
        }

        return _grid;
    }

    public Vector2 GetMinimums()
    {
        return new Vector2(_minX, _minZ);
    }
}
