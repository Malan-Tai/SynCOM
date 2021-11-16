using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RegionButton : MonoBehaviour
{
    [SerializeField]
    private RegionScriptableObject _regionData;
    [SerializeField]
    private Color _hoverColor;

    private Image _sprite;

    public delegate void EventMouseEnter(RegionScriptableObject data);
    public static event EventMouseEnter OnMouseEnterEvent;

    public delegate void EventMouseExit();
    public static event EventMouseExit OnMouseExitEvent;

    public delegate void EventMouseClick();
    public static event EventMouseClick OnMouseClickEvent;

    private void Start()
    {
        _sprite = GetComponent<Image>();
    }

    private void OnMouseEnter()
    {
        _sprite.color = _hoverColor;
        if (OnMouseEnterEvent != null) OnMouseEnterEvent(_regionData);
    }

    private void OnMouseExit()
    {
        _sprite.color = Color.white;
        if (OnMouseExitEvent != null) OnMouseExitEvent();
    }

    private void OnMouseUpAsButton()
    {
        print("clicked " + _regionData.regionName);
        if (OnMouseClickEvent != null) OnMouseClickEvent();
    }
}
