using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileComponent : MonoBehaviour
{
    private Tile _tile;
    public Tile Tile { get { return _tile; } }

    public void SetTile(Tile tile)
    {
        _tile = tile;
    }
}
