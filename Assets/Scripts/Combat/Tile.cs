using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;

public class Tile : FastPriorityQueueNode
{
    private EnumCover _cover;
    public EnumCover Cover { get { return _cover; } }

    private Vector2Int _coords;
    public Vector2Int Coords { get { return _coords; } }

    private TileComponent _tileComponent;

    public Tile(int x, int y, EnumCover cover)
    {
        _coords = new Vector2Int(x, y);
        _cover = cover;
    }

    public float MoveCost(Vector2Int next)
    {
        Vector2Int difference = next - _coords;

        if (difference.x == 0 || difference.y == 0) return 1f; // straight
        return 1.4f; // diagonal ~= sqrt(2)
    }

    public void SetTileComponent(TileComponent tileComp)
    {
        _tileComponent = tileComp;
    }

    public void BecomeReachable()
    {
        _tileComponent.BecomeReachable();
    }

    public void BecomeUnreachable()
    {
        _tileComponent.BecomeUnreachable();
    }

}