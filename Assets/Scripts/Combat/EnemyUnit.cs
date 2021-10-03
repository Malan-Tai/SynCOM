using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyUnit : GridBasedUnit
{
    private MeshRenderer _renderer;
    private Color _originalColor;

    private new void Start()
    {
        base.Start();

        _renderer = GetComponent<MeshRenderer>();
        _originalColor = _renderer.material.GetColor("_Color");
    }

    protected override bool IsEnemy()
    {
        return true;
    }

    public void UpdateVisibility(bool seen, EnumCover cover = EnumCover.Full)
    {
        if (!seen)
        {
            _renderer.material.SetColor("_Color", Color.black);
        }
        else
        {
            switch (cover)
            {
                case EnumCover.None:
                    _renderer.material.SetColor("_Color", Color.green);
                    break;
                case EnumCover.Half:
                    _renderer.material.SetColor("_Color", Color.yellow);
                    break;
                case EnumCover.Full:
                    _renderer.material.SetColor("_Color", Color.red);
                    break;
                default:
                    _renderer.material.SetColor("_Color", _originalColor);
                    break;
            }
        }
    }
}
