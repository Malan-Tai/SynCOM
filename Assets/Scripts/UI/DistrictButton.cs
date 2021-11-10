using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DistrictButton : MonoBehaviour
{
    [SerializeField]
    private string _name;
    [SerializeField]
    private Color _hoverColor;

    private Image _sprite;

    private void Start()
    {
        _sprite = GetComponent<Image>();
    }

    private void OnMouseEnter()
    {
        _sprite.color = _hoverColor;
    }

    private void OnMouseExit()
    {
        _sprite.color = Color.white;
    }

    private void OnMouseUpAsButton()
    {
        print("clicked " + _name);
    }
}
