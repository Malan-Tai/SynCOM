using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridInitializer : MonoBehaviour
{
    [SerializeField, Range(1, 25)] private int _cellSize;
    [SerializeField] private Transform[] _gridPos;
    [SerializeField] private LayerMask _mapElementLayer;

    [Header("Gizmos")]
    [SerializeField] private bool _showGridGizmos = true;
    [SerializeField] private bool _showWalkableGizmos = true;
    [SerializeField] private bool _showCoversGizmos = true;

    private struct GridTile
    {
        public bool IsWalkable;
        public EnumCover CoverValue;
    }


    private GridTile[,] _grid;
    private int _minX = int.MaxValue;
    private int _minZ = int.MaxValue;
    private int _maxX = int.MinValue;
    private int _maxZ = int.MinValue;

    private void Start()
    {
        DetermineZone();
        CreateGrid();
    }

    private void DetermineZone()
    {
        if (_cellSize < 1) _cellSize = 1;

        for (int i = 0; i < _gridPos.Length; i++)
        {
            _minX = Mathf.Min(_minX, Mathf.FloorToInt(_gridPos[i].position.x));
            _maxX = Mathf.Max(_maxX, Mathf.FloorToInt(_gridPos[i].position.x));
            _minZ = Mathf.Min(_minZ, Mathf.FloorToInt(_gridPos[i].position.z));
            _maxZ = Mathf.Max(_maxZ, Mathf.FloorToInt(_gridPos[i].position.z));
        }
    }

    private void CreateGrid()
    {
        _grid = new GridTile[_maxX - _minX, _maxZ - _minZ];
        Vector3 overlapSize = new Vector3(_cellSize * 0.45f, _cellSize * 2f, _cellSize * 0.45f);

        for (int x = 0; x < _maxX - _minX; x += _cellSize)
        {
            for (int z = 0; z < _maxZ - _minZ; z += _cellSize)
            {
                Collider[] overlap = Physics.OverlapBox(new Vector3(_minX + x + _cellSize / 2f, 0f, _minZ + z + _cellSize / 2f), overlapSize, Quaternion.identity, _mapElementLayer);
                _grid[x, z] = new GridTile { IsWalkable = true, CoverValue = EnumCover.None };

                for (int i = 0; i < overlap.Length; i++)
                {
                    MapElement mapElement = overlap[i].GetComponent<MapElement>();

                    if (mapElement != null)
                    {
                        _grid[x, z].IsWalkable &= mapElement.IsWalkable;
                        _grid[x, z].CoverValue = _grid[x, z].CoverValue < mapElement.CoverValue ? mapElement.CoverValue : _grid[x, z].CoverValue;
                    }
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        DetermineZone();

        for (int x = 0; x < _maxX - _minX; x += _cellSize)
        {
            for (int z = 0; z < _maxZ - _minZ; z += _cellSize)
            {
                Vector3 cellPosition = new Vector3(_minX + x + _cellSize / 2f, 0f, _minZ + z + _cellSize / 2f);

                if (_showGridGizmos)
                {
                    Gizmos.color = Color.gray;
                    Gizmos.DrawLine(new Vector3(_minX, 0f, z + _minZ), new Vector3(_maxX, 0f, z + _minZ));
                    Gizmos.DrawLine(new Vector3(x + _minX, 0f, _minZ), new Vector3(x + _minX, 0f, _maxZ));
                }

                if (_grid != null && _showWalkableGizmos)
                {
                    if (_grid[x, z].IsWalkable)
                    {
                        Gizmos.color = Color.blue;
                    }
                    else
                    {
                        Gizmos.color = Color.red;
                    }
                    Gizmos.DrawCube(cellPosition + Vector3.up * 3f, new Vector3(_cellSize, 0.01f, _cellSize));
                }

                if (_grid != null && _showCoversGizmos)
                {
                    switch (_grid[x, z].CoverValue)
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
}
