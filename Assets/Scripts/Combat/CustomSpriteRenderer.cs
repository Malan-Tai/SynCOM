using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomSpriteRenderer : GeneralRenderer
{
    private SpriteRenderer _renderer;

    private void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _originalColor = _renderer.color;
    }

    public override void SetColor(Color color)
    {
        _renderer.color = color;
    }
}
