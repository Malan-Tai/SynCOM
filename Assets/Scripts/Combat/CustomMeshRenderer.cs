using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomMeshRenderer : GeneralRenderer
{
    private MeshRenderer _renderer;

    private void Start()
    {
        _renderer = GetComponent<MeshRenderer>();
        _originalColor = _renderer.material.GetColor("_Color");
    }

    public override void SetColor(Color color)
    {
        _renderer.material.SetColor("_Color", color);
    }
}
