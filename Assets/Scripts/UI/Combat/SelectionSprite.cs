using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionSprite : MonoBehaviour
{
    private SpriteRenderer _renderer;

    [SerializeField] private Color _allySelectedColor;
    [SerializeField] private Color _allyUnselectedColor;
    [SerializeField] private Color _enemySelectedColor;

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    public void SetEnabled(bool status)
    {
        _renderer.enabled = status;
    }

    public void SetAlly()
    {
        _renderer.color = _allyUnselectedColor;
    }

    public void SetAllySelected()
    {
        _renderer.color = _allySelectedColor;
    }

    public void SetEnemy()
    {
        _renderer.color = _enemySelectedColor;
    }
}
