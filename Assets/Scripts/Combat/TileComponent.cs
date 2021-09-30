using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileComponent : MonoBehaviour
{
    private Tile _tile;
    public Tile Tile { get { return _tile; } }

    private MeshRenderer _renderer;

    [SerializeField]
    private Color _reachableColor;
    private Color _originalColor;

    private void Start()
    {
        _renderer = GetComponent<MeshRenderer>();
        _originalColor = _renderer.material.GetColor("_Color");
    }

    public void SetTile(Tile tile)
    {
        _tile = tile;
    }

    public void BecomeReachable()
    {
        _renderer.material.SetColor("_Color", _reachableColor);
    }

    public void BecomeUnreachable()
    {
        _renderer.material.SetColor("_Color", _originalColor);
    }
}
