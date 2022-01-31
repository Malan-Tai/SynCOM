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
    private Color _nonHoverColor;

    private Image _star;

    private Image _sprite;

    public delegate void EventMouseEnter(RegionScriptableObject data);
    public static event EventMouseEnter OnMouseEnterEvent;

    public delegate void EventMouseExit();
    public static event EventMouseExit OnMouseExitEvent;

    public delegate void EventMouseClick(RegionScriptableObject data);
    public static event EventMouseClick OnMouseClickEvent;

    private void Start()
    {
        _nonHoverColor = _hoverColor; //new Color(_hoverColor.r, _hoverColor.g, _hoverColor.b, 0.5f);
        _hoverColor = Color.Lerp(_nonHoverColor, Color.black, 0.3f);

        _sprite = GetComponent<Image>();
        _sprite.color = _nonHoverColor;

        _star = transform.Find("Image").GetComponent<Image>();
        _star.enabled = false;
    }

    private void OnEnable()
    {
        BetweenMissionsGameManager.OnMissionGenerated += UpdateMissionSymbol;
    }

    private void OnDisable()
    {
        BetweenMissionsGameManager.OnMissionGenerated -= UpdateMissionSymbol;
    }

    private void OnMouseEnter()
    {
        _sprite.color = _hoverColor;
        if (OnMouseEnterEvent != null) OnMouseEnterEvent(_regionData);
    }

    private void OnMouseExit()
    {
        _sprite.color = _nonHoverColor;
        if (OnMouseExitEvent != null) OnMouseExitEvent();
    }

    private void OnMouseUpAsButton()
    {
        if (BetweenMissionsGameManager.Instance.GetMissionInRegion(_regionData.regionName).Equals(Mission.None)) return;

        print("clicked " + _regionData.regionName);
        if (OnMouseClickEvent != null) OnMouseClickEvent(_regionData);
    }

    private void UpdateMissionSymbol(RegionName region)
    {
        if (region == _regionData.regionName) _star.enabled = true;
    }
}
