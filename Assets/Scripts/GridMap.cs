using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridMap : MonoBehaviour
{
    [SerializeField]
    private float _gridSize;

    public Vector3 GridToWorld(Vector2 grid, float y)
    {
        return new Vector3(grid.x * _gridSize + _gridSize / 2, y, grid.y * _gridSize + _gridSize / 2);
    }

    public Vector2 WorldToGrid(Vector3 world)
    {
        Vector2 gridPos = new Vector2();
        gridPos.x = Mathf.Floor(world.x / _gridSize - 0.5f);
        gridPos.y = Mathf.Floor(world.z / _gridSize - 0.5f);
        return gridPos;
    }
}
