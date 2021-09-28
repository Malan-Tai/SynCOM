using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileComponent : MonoBehaviour
{
    private Tile _tile;
    public Tile Tile { get { return _tile; } }

    private MeshRenderer _renderer;

    [SerializeField]
    private Material _reachableMat;
    private Material _originalMat;

    private void Start()
    {
        _renderer = GetComponent<MeshRenderer>();
        _originalMat = _renderer.material;
    }

    public void SetTile(Tile tile)
    {
        _tile = tile;
    }

    public void BecomeReachable()
    {
        if (_reachableMat == null) return;

        _renderer.material = _reachableMat;
    }

    public void BecomeUnreachable()
    {
        _renderer.material = _originalMat;
    }
}
