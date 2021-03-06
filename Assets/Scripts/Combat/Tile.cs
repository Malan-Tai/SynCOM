using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;

public class Tile : FastPriorityQueueNode
{
    private EnumCover _cover;
    public EnumCover Cover
    {
        get { return _cover; }
        set
        {
            _cover = value;
            _isWalkable = value != EnumCover.Full;
        }
    }

    private bool _isWalkable;
    public bool IsWalkable { get { return _isWalkable; } }

    private Vector2Int _coords;
    public Vector2Int Coords { get { return _coords; } }

    public Tile(int x, int y, EnumCover cover, bool walkable)
    {
        _coords = new Vector2Int(x, y);
        _cover = cover;
        _isWalkable = walkable;
    }

    public float MoveCost(Vector2Int next)
    {
        Vector2Int difference = next - _coords;

        if (difference.x == 0 || difference.y == 0) return 1f; // straight
        return 1.4f; // diagonal ~= sqrt(2)
    }
}